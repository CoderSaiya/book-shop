import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly THEME_KEY = 'bookshop-theme';
  private themeSubject = new BehaviorSubject<string>('light');
  currentTheme$ = this.themeSubject.asObservable();

  private platformId = inject(PLATFORM_ID);
  private document = inject(DOCUMENT);

  constructor() {
    this.loadTheme();
  }

  private loadTheme(): void {
    if (isPlatformBrowser(this.platformId)) {
      const savedTheme = localStorage.getItem(this.THEME_KEY) || 'light';
      this.setTheme(savedTheme);
    } else {
      this.setTheme('light');
    }
  }

  setTheme(theme: string): void {
    this.themeSubject.next(theme);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.THEME_KEY, theme);
      this.document.documentElement.setAttribute('data-theme', theme);
    }
  }

  toggleTheme(): void {
    const cur = this.themeSubject.value;
    this.setTheme(cur === 'light' ? 'dark' : 'light');
  }

  getCurrentTheme(): string {
    return this.themeSubject.value;
  }
}
