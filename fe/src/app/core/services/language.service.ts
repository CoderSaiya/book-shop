import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { BehaviorSubject } from 'rxjs';

type Lang = 'en' | 'vi';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly KEY = 'bookshop-language';
  private platformId = inject(PLATFORM_ID);
  private subject = new BehaviorSubject<Lang>(this.loadInitial());

  currentLanguage$ = this.subject.asObservable();

  get current(): Lang {
    return this.subject.value;
  }

  private load(): Lang {
    if (isPlatformBrowser(this.platformId)) {
      return (localStorage.getItem(this.KEY) as Lang) || 'en';
    }
    return 'en';
  }

  setLanguage(lang: Lang) {
    this.subject.next(lang);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.KEY, lang);
    }
    if (typeof document !== 'undefined') {
      document.documentElement.setAttribute('lang', lang);
    }
  }

  getCurrentLanguage(): string {
    return localStorage.getItem("bookshop-language") === 'en' ? 'en' : 'vi';
  }

  private loadInitial(): Lang {
    if (!isPlatformBrowser(this.platformId)) return 'en';
    const saved = localStorage.getItem(this.KEY);
    return saved === 'vi' || saved === 'en' ? (saved as Lang) : 'en';
  }
}
