import {APP_INITIALIZER, ApplicationConfig, importProvidersFrom, inject, PLATFORM_ID} from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import {provideHttpClient, withFetch, withInterceptors} from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import {provideTranslateService, TranslateModule, TranslateService} from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import {provideStore} from "@ngrx/store";
import {provideEffects} from "@ngrx/effects";
import { categoryReducer } from "./store/category/category.reducer"
import { CategoryEffects } from "./store/category/category.effects"
import {provideStoreDevtools} from "@ngrx/store-devtools";
import {environment} from "../environments/environment";
import {DOCUMENT, isPlatformBrowser} from "@angular/common";
import { AuthInterceptor } from './core/interceptors/auth.interceptor';

function initLanguage() {
  return () => {
    const translate = inject(TranslateService);
    const platformId = inject(PLATFORM_ID);
    const doc = inject(DOCUMENT);

    let lang: 'en' | 'vi' = 'en';
    if (isPlatformBrowser(platformId)) {
      const saved = localStorage.getItem('bookshop-language');
      if (saved === 'vi' || saved === 'en') lang = saved as 'en' | 'vi';
    }

    translate.setDefaultLang('en');
    translate.use(lang);
    doc?.documentElement?.setAttribute('lang', lang);
  };
}


export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([AuthInterceptor])
    ),
    provideAnimations(),

    // i18n
    provideTranslateService({
      loader: provideTranslateHttpLoader({
        prefix: '/assets/i18n/',
        suffix: '.json',
      }),
      lang: 'en',
      fallbackLang: 'en',
    }),

    { provide: APP_INITIALIZER, useFactory: initLanguage, multi: true },

    // NgRx
    provideStore({
      category: categoryReducer,
    }),
    provideEffects([CategoryEffects]),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: environment.production,
    })
  ],
};
