import { Component, type OnInit, type OnDestroy, Inject, PLATFORM_ID } from "@angular/core"
import { CommonModule, isPlatformBrowser } from "@angular/common"
import { RouterModule } from "@angular/router"
import { TranslateModule, TranslateService } from "@ngx-translate/core"
import { ThemeService } from "../../../core/services/theme.service"
import { CartService } from "../../../core/services/cart.service"
import { AuthService } from "../../../core/services/auth.service"
import { Subject } from "rxjs"
import { takeUntil } from "rxjs/operators"

@Component({
  selector: "app-header",
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  template: `
    <header class="header">
      <div class="container">
        <div class="header-content">
          <div class="logo">
            <a routerLink="/" class="logo-link">
              <span class="logo-text">BookShop</span>
            </a>
          </div>

          <nav class="nav" [class.nav-open]="isMenuOpen">
            <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" class="nav-link">
              {{ 'nav.home' | translate }}
            </a>
            <a routerLink="/books" routerLinkActive="active" class="nav-link">
              {{ 'nav.books' | translate }}
            </a>
            <a routerLink="/cart" routerLinkActive="active" class="nav-link cart-link">
              {{ 'nav.cart' | translate }}
              <span class="cart-badge" *ngIf="cartItemCount > 0">{{ cartItemCount }}</span>
            </a>
          </nav>

          <div class="header-actions">
            <button
              class="action-btn language-btn"
              (click)="toggleLanguage()"
              [title]="currentLang === 'en' ? 'Switch to Vietnamese' : 'Chuyển sang tiếng Anh'"
            >
              {{ currentLang === 'en' ? 'VI' : 'EN' }}
            </button>

            <button
              class="action-btn theme-toggle"
              (click)="toggleTheme()"
              [title]="currentTheme === 'light' ? 'Switch to dark mode' : 'Switch to light mode'"
            >
              <span class="theme-icon">{{ currentTheme === 'light' ? '🌙' : '☀️' }}</span>
            </button>

            <div class="user-menu" *ngIf="currentUser; else loginButton">
              <div class="user-dropdown" [class.open]="isUserMenuOpen">
                <button class="action-btn user-btn" (click)="toggleUserMenu()">
                  <img
                    *ngIf="currentUser.avatar; else userIcon"
                    [src]="currentUser.avatar"
                    [alt]="currentUser.firstName"
                    class="user-avatar"
                  />
                  <ng-template #userIcon>
                    <span class="user-icon">👤</span>
                  </ng-template>
                </button>

                <div class="dropdown-menu" *ngIf="isUserMenuOpen">
                  <div class="dropdown-header">
                    <span class="user-name">{{ currentUser.firstName }} {{ currentUser.lastName }}</span>
                    <span class="user-email">{{ currentUser.email }}</span>
                  </div>
                  <div class="dropdown-divider"></div>
                  <a routerLink="/profile" class="dropdown-item" (click)="closeUserMenu()">
                    {{ 'nav.profile' | translate }}
                  </a>
                  <a routerLink="/orders" class="dropdown-item" (click)="closeUserMenu()">
                    {{ 'nav.orders' | translate }}
                  </a>
                  <div class="dropdown-divider"></div>
                  <button class="dropdown-item logout-btn" (click)="logout()">
                    {{ 'nav.logout' | translate }}
                  </button>
                </div>
              </div>
            </div>

            <ng-template #loginButton>
              <a routerLink="/auth/login" class="action-btn login-btn">
                {{ 'nav.login' | translate }}
              </a>
            </ng-template>

            <button class="mobile-menu-toggle" (click)="toggleMobileMenu()">
              <span class="hamburger" [class.active]="isMenuOpen"></span>
            </button>
          </div>
        </div>
      </div>
    </header>
  `,
  styleUrls: ["./header.component.scss"],
})
export class HeaderComponent implements OnInit, OnDestroy {
  currentTheme = "light"
  currentLang = "en"
  cartItemCount = 0
  isMenuOpen = false
  isUserMenuOpen = false
  currentUser: any = null
  private destroy$ = new Subject<void>();

  constructor(
    private themeService: ThemeService,
    private translate: TranslateService,
    private cartService: CartService,
    private authService: AuthService,
    @Inject(PLATFORM_ID) private platformId: any,
  ) {}

  ngOnInit(): void {
    this.themeService.currentTheme$.pipe(takeUntil(this.destroy$)).subscribe((theme) => {
      this.currentTheme = theme
    })

    this.cartService
      .getCartItemCount()
      .pipe(takeUntil(this.destroy$))
      .subscribe((count) => {
        this.cartItemCount = count
      })

    this.authService.currentUser$.pipe(takeUntil(this.destroy$)).subscribe((user) => {
      this.currentUser = user
    })

    this.currentLang = this.translate.currentLang || this.translate.defaultLang || 'en';

    this.translate.onLangChange.pipe(takeUntil(this.destroy$)).subscribe((event) => {
      this.currentLang = event.lang
    })
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  toggleTheme(): void {
    this.themeService.toggleTheme()
  }

  toggleLanguage(): void {
    const newLang = this.currentLang === "en" ? "vi" : "en"
    this.translate.use(newLang)

    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem("bookshop-language", newLang)
    }
  }

  toggleMobileMenu(): void {
    this.isMenuOpen = !this.isMenuOpen
  }

  toggleUserMenu(): void {
    this.isUserMenuOpen = !this.isUserMenuOpen
  }

  closeUserMenu(): void {
    this.isUserMenuOpen = false
  }

  logout(): void {
    this.authService.logout()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.closeUserMenu(),
        error: (e) => {
          this.authService.clearSession();
          this.closeUserMenu();
          console.error(e);
        }
      });
  }
}
