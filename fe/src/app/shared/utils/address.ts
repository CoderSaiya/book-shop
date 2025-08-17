export interface ParsedAddress {
  street?: string;
  wardName?: string;
  districtName?: string;
  provinceName?: string;
}

// Bỏ dấu + chuẩn hoá (an toàn cross-browser)
export function normalizeName(s: string): string {
  return (s || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '') // bỏ dấu
    .replace(/\s+/g, ' ')
    .trim()
    .toLowerCase();
}

// Bỏ tiền tố để SO KHỚP (không dùng cho hiển thị)
export function stripAllPrefixes(part: string): string {
  return (part || '')
    .replace(/^(tinh|thanh pho|tp\.?)/i, '')
    .replace(/^(quan|q\.|huyen|h\.|thi xa|thi tran)/i, '')
    .replace(/^(phuong|p\.|xa)/i, '')
    .trim();
}

// So khớp lỏng: bỏ dấu + bỏ tiền tố ở CẢ 2 phía
export function looseEqualsName(a?: string, b?: string): boolean {
  if (!a || !b) return false;
  const aa = normalizeName(stripAllPrefixes(a));
  const bb = normalizeName(stripAllPrefixes(b));
  return aa === bb;
}

type Level = 'province' | 'district' | 'ward' | 'street';

const PROV = /\b(tinh|thanh pho|tp\.?)\b/;
const DIST = /\b(quan|q\.|huyen|h\.|thi xa|thi tran)\b/;
const WARD = /\b(phuong|p\.|xa|thi tran)\b/;

function classifyPart(raw: string): Level {
  const n = normalizeName(raw);
  if (PROV.test(n)) return 'province';
  if (DIST.test(n)) return 'district';
  if (WARD.test(n)) return 'ward';
  return 'street';
}

/** Parse: "street, Ward, District, City/Province" */
export function parseVietnamAddress(raw: string): ParsedAddress {
  const parts = (raw || '')
    .split(',')
    .map(s => s.trim())
    .filter(Boolean);

  const streetParts: string[] = [];
  let provinceName: string | undefined;
  let districtName: string | undefined;
  let wardName: string | undefined;

  for (const p of parts) {
    switch (classifyPart(p)) {
      case 'province':
        if (!provinceName) provinceName = p;
        break;
      case 'district':
        if (!districtName) districtName = p;
        break;
      case 'ward':
        if (!wardName) wardName = p;
        break;
      default:
        streetParts.push(p);
        break;
    }
  }

  // Fallback theo vị trí nếu thiếu nhãn
  if (!provinceName && parts.length >= 1) provinceName = parts.at(-1)!;
  if (!districtName && parts.length >= 2) districtName = parts.at(-2)!;
  if (!wardName && parts.length >= 3) wardName = parts.at(-3)!;

  return {
    street: streetParts.join(', ').trim() || undefined,
    wardName,
    districtName,
    provinceName,
  };
}

export function joinVietnamAddress(
  street?: string,
  wardName?: string,
  districtName?: string,
  provinceName?: string
): string {
  return [street, wardName, districtName, provinceName].filter(Boolean).join(', ');
}
