export type LangCode = 'vi' | 'en';

export interface LangText {
  vi: string;
  en: string;
}

/** Lấy lang hiện tại: ưu tiên <html lang>, sau đó localStorage('bookshop-language'), mặc định 'en' */
export function getCurrentLang(): LangCode {
  try {
    const docLang =
      typeof document !== 'undefined' ? document.documentElement.getAttribute('bookshop-language') : null;
    if (docLang === 'vi' || docLang === 'en') return docLang;

    const saved =
      typeof localStorage !== 'undefined' ? localStorage.getItem('bookshop-language') : null;
    if (saved === 'vi' || saved === 'en') return saved as LangCode;
  } catch {
    // ignore
  }
  return 'en';
}

/** Chọn text theo lang hiện tại (hoặc lang truyền vào). Có fallback sang ngôn ngữ còn lại nếu rỗng. */
export function getLangText(text: LangText, lang: LangCode = getCurrentLang()): string {
  const chosen = text[lang];
  if (typeof chosen === 'string' && chosen.trim().length > 0) return chosen;

  // fallback: nếu thiếu/chuỗi rỗng, dùng ngôn ngữ còn lại
  const fallback = lang === 'vi' ? text.en : text.vi;
  return fallback ?? '';
}

/** (Tùy chọn) Tạo 1 picker gắn với hàm lấy lang của bạn (vd: LanguageService.getCurrentLanguage) */
export function makeGetLangText(getLang: () => LangCode) {
  return (text: LangText) => getLangText(text, getLang());
}
