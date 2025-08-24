# Book Advisor Service (FULL SOURCE)

## Cấu trúc
```
book-advisor-service-full/
├─ app/
│  ├─ main.py
│  ├─ router.py
│  ├─ nlp.py
│  ├─ backend_client.py
│  ├─ recommender.py
│  ├─ state_store.py
│  └─ config.py
├─ ml/
│  ├─ train_intent_hf.py
│  ├─ infer_onnx.py
│  └─ intent_train.sample.jsonl
├─ data/
│  ├─ mock_catalog.json
│  └─ mock_cart.json
├─ models/                 # sẽ chứa model sau khi train
├─ requirements.txt
├─ requirements-ml.txt
└─ .env.example
```

## Train LLM & Export ONNX
```bash
pip install -r requirements-ml.txt
python ml/train_intent_hf.py --data ml/intent_train.sample.jsonl --out models/intent_llm --model xlm-roberta-base
# output: models/intent_llm/onnx/model.onnx + hf_model/tokenizer.json + labels.json
```

## Test ONNX nhanh
```bash
python ml/infer_onnx.py --model_dir models/intent_llm --text "thêm 2 cuốn sherlock vào giỏ"
```

## Chạy service tư vấn (tuỳ chọn)
```bash
pip install -r requirements.txt
cp .env.example .env
uvicorn app.main:app --reload --port 8000
```
