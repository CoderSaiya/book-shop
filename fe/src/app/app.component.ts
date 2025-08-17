import { Component, type OnInit, type OnDestroy } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterOutlet } from "@angular/router"
import { TranslateService } from "@ngx-translate/core"
import { ThemeService } from "./core/services/theme.service"
import { LanguageService } from "./core/services/language.service"
import { Subject } from "rxjs"
import { takeUntil } from "rxjs/operators"
import {ToastContainerComponent} from './shared/components/toast-notify/toast-container.component';

@Component({
  selector: "app-root",
  standalone: true,
  imports: [CommonModule, RouterOutlet, ToastContainerComponent],
  template: `
    <div [attr.data-theme]="currentTheme">
      <router-outlet></router-outlet>
      <app-toast-container></app-toast-container>
    </div>
  `,
  styleUrls: ["app.component.scss"],
})
export class AppComponent implements OnInit, OnDestroy {
  currentTheme = "light"
  private destroy$ = new Subject<void>()

  constructor(
      private translate: TranslateService,
      private themeService: ThemeService,
      private languageService: LanguageService,
  ) {
    this.translate.setDefaultLang("en")
  }

  ngOnInit(): void {
    this.themeService.currentTheme$.pipe(takeUntil(this.destroy$)).subscribe((theme) => {
      this.currentTheme = theme
    })

    this.languageService.currentLanguage$.pipe(takeUntil(this.destroy$)).subscribe((language) => {
      this.translate.use(language)
    })
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }
}
