import { Injectable } from "@angular/core"

@Injectable({
  providedIn: "root",
})
export class TranslationMappingService {
  private categoryMapping: Record<string, { vi: string; en: string }> = {
    "sach-thieu-nhi": { vi: "Sách thiếu nhi", en: "Children Books" },
    "van-hoc": { vi: "Văn học", en: "Literature" },
    "khoa-hoc": { vi: "Khoa học", en: "Science" },
    "lich-su": { vi: "Lịch sử", en: "History" },
    "kinh-te": { vi: "Kinh tế", en: "Economics" },
    "cong-nghe": { vi: "Công nghệ", en: "Technology" },
  }

  translateCategory(vietnameseName: string, language: "vi" | "en" = "vi"): string {
    const key = this.findKeyByVietnameseName(vietnameseName)
    return key ? this.categoryMapping[key][language] : vietnameseName
  }

  private findKeyByVietnameseName(name: string): string | null {
    for (const [key, value] of Object.entries(this.categoryMapping)) {
      if (value.vi === name) return key
    }
    return null
  }

  getAllCategories(language: "vi" | "en" = "vi") {
    return Object.entries(this.categoryMapping).map(([key, value]) => ({
      id: key,
      name: value[language],
      originalName: value.vi,
    }))
  }
}
