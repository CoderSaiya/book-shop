from __future__ import annotations
import argparse, json, math
from pathlib import Path
from collections import Counter
from typing import List, Tuple, Optional

import numpy as np
import torch
import torch.nn.functional as F
from torch import nn
from sklearn.model_selection import train_test_split

from datasets import Dataset
from transformers import (
    AutoTokenizer,
    AutoModelForSequenceClassification,
    DataCollatorWithPadding,
    Trainer,
    TrainingArguments,
    EarlyStoppingCallback,
    set_seed,
)

def read_jsonl(path: str):
    X,y=[],[]
    with open(path,"r",encoding="utf-8") as f:
        for line in f:
            if not line.strip(): continue
            o=json.loads(line)
            t=o.get("text") or o.get("message") or ""
            l=o.get("label") or o.get("intent") or ""
            if t and l: X.append(t); y.append(l)
    if not X: raise RuntimeError(f"No data in {path}")
    return X,y

def oversample_min_classes(X,y,min_count=5):
    cnt=Counter(y)
    if min(cnt.values())>=min_count: return X,y,False
    X2,y2=list(X),list(y)
    for lb,c in cnt.items():
        if c<min_count:
            need=min_count-c
            idx=[i for i,t in enumerate(y) if t==lb]
            for k in range(need):
                i=idx[k%len(idx)]
                X2.append(X[i]); y2.append(y[i])
    return X2,y2,True

def build_label_maps(labels):
    uniq=sorted(set(labels))
    return uniq,{l:i for i,l in enumerate(uniq)},{i:l for i,l in enumerate(uniq)}

def tokenize_fn_builder(tok,max_len:int):
    def fn(batch):
        return tok(batch["text"], truncation=True, max_length=max_len, padding=False)
    return fn

def export_onnx(model, tok, out_file:Path, max_len:int=128):
    out_file.parent.mkdir(parents=True, exist_ok=True)
    model.eval()
    with torch.no_grad():
        enc=tok("dummy", return_tensors="pt", truncation=True, max_length=max_len, padding="max_length")
        input_names=["input_ids","attention_mask","token_type_ids"]
        if "token_type_ids" not in enc: enc["token_type_ids"]=torch.zeros_like(enc["input_ids"])
        dyn={"input_ids":{0:"batch",1:"seq"},"attention_mask":{0:"batch",1:"seq"},"token_type_ids":{0:"batch",1:"seq"},"logits":{0:"batch"}}
        torch.onnx.export(model, (enc["input_ids"],enc["attention_mask"],enc["token_type_ids"]),
                          str(out_file), input_names=input_names, output_names=["logits"],
                          dynamic_axes=dyn, opset_version=17, do_constant_folding=True)

class WeightedFocalTrainer(Trainer):
    def __init__(self,*args,class_weights:Optional[List[float]]=None,focal_gamma:float=2.0,label_smoothing:float=0.1,**kwargs):
        super().__init__(*args,**kwargs)
        self.focal_gamma=focal_gamma
        self.label_smoothing=label_smoothing
        self.class_weights=torch.tensor(class_weights,dtype=torch.float) if class_weights is not None else None

    def compute_loss(self, model, inputs, return_outputs=False):
        labels=inputs.pop("labels")
        outputs=model(**inputs)
        logits=outputs.logits
        # label smoothing
        if self.label_smoothing and self.label_smoothing>0:
            num_classes=logits.size(-1)
            with torch.no_grad():
                smoothed=(1-self.label_smoothing)*torch.nn.functional.one_hot(labels,num_classes=num_classes)+self.label_smoothing/num_classes
                smoothed=smoothed.to(logits.dtype)
        else:
            smoothed=None

        if self.class_weights is not None:
            ce=F.cross_entropy(logits, labels, weight=self.class_weights.to(logits.device), reduction="none")
        else:
            ce=F.cross_entropy(logits, labels, reduction="none")

        if self.focal_gamma and self.focal_gamma>0:
            with torch.no_grad():
                p=torch.softmax(logits,dim=-1).gather(1, labels.unsqueeze(1)).squeeze(1).clamp_min(1e-6)
            loss=((1-p)**self.focal_gamma)*ce
        else:
            loss=ce

        if smoothed is not None:
            logp=torch.log_softmax(logits,dim=-1)
            ls_loss=-(smoothed*logp).sum(dim=-1)
            loss=0.8*loss + 0.2*ls_loss
        loss=loss.mean()
        return (loss, outputs) if return_outputs else loss

def main(data,out_dir,base_model,epochs,lr,batch,max_len,gamma,weight_power,eval_ratio):
    set_seed(42)
    out_dir=Path(out_dir); out_dir.mkdir(parents=True, exist_ok=True)
    X,y=read_jsonl(data)
    X,y,_=oversample_min_classes(X,y,5)

    labels,l2i,i2l=build_label_maps(y); K=len(labels); N=len(y)
    test_size=max(int(math.ceil(eval_ratio*N)), K)
    from sklearn.model_selection import train_test_split
    Xtr,Xev,ytr,yev=train_test_split(X,y,test_size=test_size,random_state=42,stratify=y)

    tok=AutoTokenizer.from_pretrained(base_model, use_fast=True)
    model=AutoModelForSequenceClassification.from_pretrained(base_model,num_labels=K,id2label=i2l,label2id=l2i)

    tr=Dataset.from_dict({"text":Xtr,"label":[l2i[t] for t in ytr]})
    ev=Dataset.from_dict({"text":Xev,"label":[l2i[t] for t in yev]})
    fn=tokenize_fn_builder(tok,max_len)
    tr=tr.map(fn,batched=True); ev=ev.map(fn,batched=True)
    coll=DataCollatorWithPadding(tokenizer=tok)

    cnt=Counter(ytr); freqs=np.array([cnt[lb] for lb in labels],dtype=np.float32)
    inv=1.0/np.power(freqs,max(0.0,weight_power)); class_w=(inv*(len(inv)/inv.sum())).tolist()

    def metrics(eval_pred):
        logits, gold = eval_pred
        preds=np.argmax(logits,axis=-1)
        return {"accuracy": float((preds==gold).mean())}

    args=TrainingArguments(
        output_dir=str(out_dir/"hf_ckpt"),
        learning_rate=lr,
        per_device_train_batch_size=batch,
        per_device_eval_batch_size=batch,
        num_train_epochs=epochs,
        evaluation_strategy="epoch",
        save_strategy="epoch",
        logging_steps=50,
        report_to=[],
        load_best_model_at_end=True,           # quan trọng cho EarlyStopping
        metric_for_best_model="eval_accuracy",
        greater_is_better=True,
        remove_unused_columns=True,
        fp16=torch.cuda.is_available(),
        warmup_ratio=0.06,
        save_total_limit=1,
    )

    trainer=WeightedFocalTrainer(
        model=model, args=args,
        train_dataset=tr, eval_dataset=ev,
        tokenizer=tok, data_collator=coll,
        compute_metrics=metrics,
        class_weights=class_w, focal_gamma=2.0, label_smoothing=0.1,
        callbacks=[EarlyStoppingCallback(early_stopping_patience=2)]
    )

    trainer.train()
    # save
    (out_dir/"hf_model").mkdir(parents=True, exist_ok=True)
    model.save_pretrained(out_dir)
    tok.save_pretrained(out_dir/"hf_model")
    with open(out_dir/"labels.json","w",encoding="utf-8") as f: json.dump(labels,f,ensure_ascii=False,indent=2)
    export_onnx(model,tok,out_dir/"onnx"/"model.onnx",max_len=max_len)
    print("DONE ->", out_dir)

if __name__=="__main__":
    ap=argparse.ArgumentParser()
    ap.add_argument("--data", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--model", default="xlm-roberta-large")
    ap.add_argument("--epochs", type=int, default=8)
    ap.add_argument("--lr", type=float, default=2e-5)
    ap.add_argument("--batch", type=int, default=16)
    ap.add_argument("--max_len", type=int, default=128)
    ap.add_argument("--weight_power", type=float, default=0.5)
    ap.add_argument("--eval_ratio", type=float, default=0.1)
    args=ap.parse_args()
    main(args.data,args.out,args.model,args.epochs,args.lr,args.batch,args.max_len,
         gamma=2.0, weight_power=args.weight_power, eval_ratio=args.eval_ratio)