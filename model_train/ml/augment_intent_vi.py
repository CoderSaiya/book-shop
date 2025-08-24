from __future__ import annotations
import argparse, json, random, unicodedata
from pathlib import Path

random.seed(42)

RECOMMEND_SYNS = [
    "gợi ý", "đề xuất", "tư vấn", "tham khảo", "cho tôi gợi ý", "muốn tham khảo", "tìm giúp tôi"
]
ADD_SYNS = [
    "thêm", "cho vào giỏ", "bỏ vào giỏ", "cộng vào giỏ", "add vào giỏ", "mua", "mua thêm"
]
AFFIRM_SYNS = ["đúng rồi", "ok", "chuẩn", "đồng ý", "chính xác", "phải"]
NEG_SYNS = ["không", "không phải", "hủy", "hủy giúp", "không đâu", "đừng"]
GREET_SYNS = ["chào", "xin chào", "hello", "hi", "chào shop"]
BYE_SYNS = ["tạm biệt", "bye", "cảm ơn, tạm biệt", "bye shop"]
REFINE_SYNS = [
    "giảm xuống", "lọc khoảng", "chỉ lấy trong tầm", "khoảng giá", "từ {} đến {}", "cỡ {}"
]

CATEGORIES = ["văn học", "trinh thám", "kỹ năng", "kinh tế", "thiếu nhi", "tâm lý", "ngôn tình"]
CURRENCIES = ["k", "nghìn", "đ", "vnđ", "vnd"]

def price_phrase():
    v = random.choice([80,100,120,150,180,200,220,250,300])
    unit = random.choice(CURRENCIES)
    if unit == "k": return f"{v}k"
    if unit == "nghìn": return f"{v} nghìn"
    if unit in ["đ","vnđ","vnd"]: return f"{v*1000}{unit}"
    return f"{v}k"

def range_phrase():
    a,b = sorted(random.sample([80,100,120,150,180,200,220,250,300,500],2))
    u1 = random.choice(CURRENCIES); u2 = random.choice(CURRENCIES)
    def fmt(x,u):
        if u=="k": return f"{x}k"
        if u=="nghìn": return f"{x} nghìn"
        return f"{x*1000}{u}"
    return fmt(a,u1), fmt(b,u2)

def norm(s): return unicodedata.normalize("NFC", s)

def gen_for_label(label: str, n: int):
    out = []
    for _ in range(n):
        cat = random.choice(CATEGORIES)
        p1, p2 = range_phrase()
        p = price_phrase()
        qty = random.choice([1,2,3,4])
        if label == "recommend":
            tmpl = random.choice([
                f"{random.choice(RECOMMEND_SYNS)} cho tôi vài cuốn {cat} tầm {p}",
                f"Tôi muốn sách {cat} khoảng {p}",
                f"Cần {cat} giá gần {p}",
                f"{random.choice(RECOMMEND_SYNS)} sách {cat}",
                f"gợi ý sách {cat} trong tầm {p1} đến {p2}",
            ])
        elif label == "add_to_cart":
            tmpl = random.choice([
                f"{random.choice(ADD_SYNS)} {qty} cuốn {cat}",
                f"mua {qty} quyển {cat}",
                f"{random.choice(ADD_SYNS)} {qty} quyển giá {p}",
                f"cho {qty} sách {cat} vào giỏ",
            ])
        elif label == "confirm_yes":
            tmpl = random.choice(AFFIRM_SYNS)
        elif label == "confirm_no":
            tmpl = random.choice(NEG_SYNS)
        elif label == "greeting":
            tmpl = random.choice(GREET_SYNS)
        elif label == "goodbye":
            tmpl = random.choice(BYE_SYNS)
        elif label == "refine":
            tmpl = random.choice([
                f"{random.choice(REFINE_SYNS)} {p1} đến {p2}",
                f"lọc {cat} trong tầm {p1}-{p2}",
                f"chỉ lấy khoảng {p}",
            ]).format(p1, p2)
        else:
            continue
        out.append({"text": norm(tmpl), "label": label})
    return out

def main(seed_path: str, out_path: str, per_label: int):
    seed = []
    with open(seed_path, "r", encoding="utf-8") as f:
        for line in f:
            if line.strip():
                seed.append(json.loads(line))
    # đếm label
    labels = sorted({s["label"] for s in seed})
    data = list(seed)
    for lb in labels:
        data += gen_for_label(lb, per_label)
    # trộn và ghi
    random.shuffle(data)
    with open(out_path, "w", encoding="utf-8") as f:
        for obj in data:
            f.write(json.dumps(obj, ensure_ascii=False) + "\n")
    print(f"Wrote {len(data)} lines -> {out_path}")

if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--seed", required=True) # intent_train.seed.jsonl
    ap.add_argument("--out", required=True)  # intent_train.aug.jsonl
    ap.add_argument("--per_label", type=int, default=1500)  # tổng ~ 7*1500 ~ 10k
    args = ap.parse_args()
    main(args.seed, args.out, args.per_label)