export type LangCode = 'vi' | 'en';

export interface PriceConfig {
  /** Nếu dữ liệu là "nghìn VND" -> 25; nếu là "VND đầy đủ" -> 25000 */
  usdDivisor: number;
  prefixes: Record<LangCode, string>;
  suffixes: Record<LangCode, string>;
  getLang?: () => LangCode;
}

const cfg: PriceConfig = {
  usdDivisor: 25000,
  prefixes: { en: '$', vi: '' },
  suffixes: { en: '', vi: ' ₫' },
  getLang: () => 'vi',
};

export function setPriceConfig(partial: Partial<PriceConfig>) {
  Object.assign(cfg, partial);
}

export function toDisplay(value?: number | null, lang?: LangCode): number {
  if (value == null) return 0;
  const l = lang ?? cfg.getLang?.() ?? 'vi';
  return l === 'en' ? Math.round(value / cfg.usdDivisor) : value;
}

/** Format đầy đủ kèm prefix/suffix + tách hàng nghìn (2 chữ số thập phân) */
export function formatPrice(value?: number | null, lang?: LangCode): string {
  const l = lang ?? cfg.getLang?.() ?? 'vi';
  const num = toDisplay(value, l);
  const nf = new Intl.NumberFormat(l === 'en' ? 'en-US' : 'vi-VN', { maximumFractionDigits: 2 });
  return `${cfg.prefixes[l]}${nf.format(num)}${cfg.suffixes[l]}`;
}

/** Phần trăm giảm giá */
export function discountPercent(original?: number | null, current?: number | null): number {
  if (!original || current == null) return 0;
  return Math.round(((original - current) / original) * 100);
}
