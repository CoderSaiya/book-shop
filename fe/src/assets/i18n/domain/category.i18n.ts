export type Lang = 'vi' | 'en';

export const CATEGORY_I18N: Record<
  string,
  { vi: string; en: string }
> = {
  'tieu-thuyet': { vi: 'Tiểu thuyết', en: 'Fiction' },
  'kinh-dien':   { vi: 'Kinh điển',   en: 'Classics' },
  'khoa-hoc':    { vi: 'Khoa học',    en: 'Science' },
  'tam-ly':      { vi: 'Tâm lý',      en: 'Psychology' },
  'sach-thieu-nhi': { vi: 'Sách thiếu nhi', en: 'Children' },
};
