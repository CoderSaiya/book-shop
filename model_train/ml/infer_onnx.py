from __future__ import annotations
import argparse, json, numpy as np, onnxruntime as ort
from tokenizers import Tokenizer

def softmax(x):
    e = np.exp(x - np.max(x, axis=-1, keepdims=True))
    return e / e.sum(axis=-1, keepdims=True)

def _pad_id_from_tokenizer_json(tokenizer_json_path: str, default: int = 0) -> int:
    try:
        with open(tokenizer_json_path, "r", encoding="utf-8") as f:
            obj = json.load(f)
        pad_id = obj.get("padding", {}).get("pad_id", None)
        if isinstance(pad_id, int):
            return pad_id
    except Exception:
        pass
    return default

def run(model_dir: str, text: str, max_len: int = 128):
    labels = json.load(open(f"{model_dir}/labels.json","r",encoding="utf-8"))
    tok = Tokenizer.from_file(f"{model_dir}/hf_model/tokenizer.json")
    pad_id = _pad_id_from_tokenizer_json(f"{model_dir}/hf_model/tokenizer.json", default=0)

    enc = tok.encode(text)
    ids = enc.ids[:max_len]
    attn = [1]*len(ids)
    if len(ids) < max_len:
        pad_len = max_len - len(ids)
        ids  += [pad_id] * pad_len
        attn += [0] * pad_len

    sess = ort.InferenceSession(f"{model_dir}/onnx/model.onnx", providers=["CPUExecutionProvider"])
    input_names = [inp.name for inp in sess.get_inputs()]
    feeds = {
        "input_ids": np.array([ids], dtype=np.int64),
        "attention_mask": np.array([attn], dtype=np.int64),
    }
    if "token_type_ids" in input_names and "token_type_ids" not in feeds:
        feeds["token_type_ids"] = np.zeros_like(feeds["input_ids"], dtype=np.int64)

    logits = sess.run(None, feeds)[0]
    p = softmax(logits)[0]; idx = int(np.argmax(p))
    print({"label": labels[idx], "confidence": float(p[idx])})

if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--model_dir", default="models/intent_llm")
    ap.add_argument("--text", required=True)
    ap.add_argument("--max_len", type=int, default=128)
    args = ap.parse_args()
    run(args.model_dir, args.text, args.max_len)