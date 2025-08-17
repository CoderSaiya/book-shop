import {Component, type OnInit, type OnDestroy, inject} from "@angular/core"
import { CommonModule } from "@angular/common"
import {Router, RouterModule} from "@angular/router"
import { TranslateModule } from "@ngx-translate/core"
import { HeaderComponent } from "../../shared/components/header/header.component"
import { BookService } from "../../core/services/book.service"
import type { Book } from "../../models/book.model"
import { Subject } from "rxjs"
import { takeUntil } from "rxjs/operators"
import type { Category } from "../../models/api-response.model"
import type { AppState } from "../../store/app.state"
import {
  selectAllCategories,
  selectCategoryLoading,
  selectCategoryError,
} from "../../store/category/category.selectors"
import { loadCategories } from "../../store/category/category.actions"
import {Store} from '@ngrx/store';
import {LanguageService} from '../../core/services/language.service';
import {getCurrentLang, getLangText} from '../../shared/utils/lang';
import { formatPrice} from '../../shared/utils/currency';

@Component({
  selector: "app-home",
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, HeaderComponent],
  template: `
    <app-header></app-header>

    <main class="home">
      <!-- Hero Section -->
      <section class="hero">
        <div class="container">
          <div class="hero-grid">
            <div class="hero-content">
              <div class="hero-badge">
                <span>{{ 'home.featured' | translate }}</span>
              </div>
              <h1 class="hero-title">
                {{ 'home.hero.title_part1' | translate }}
                <span class="title-accent">{{ 'home.hero.title_accent' | translate }}</span>
                {{ 'home.hero.title_part2' | translate }}
              </h1>
              <p class="hero-description">
                {{ 'home.subtitle' | translate }}
              </p>
              <div class="hero-actions">
                <a routerLink="/books" class="btn btn-primary">
                  {{ 'home.cta.explore' | translate }}
                </a>
                <button class="btn btn-secondary">
                  {{ 'home.cta.viewCategories' | translate }}
                </button>
              </div>
              <div class="hero-stats">
                <div class="stat">
                  <span class="stat-number">10K+</span>
                  <span class="stat-label">{{ 'home.stats.books' | translate }}</span>
                </div>
                <div class="stat">
                  <span class="stat-number">50K+</span>
                  <span class="stat-label">{{ 'home.stats.readers' | translate }}</span>
                </div>
                <div class="stat">
                  <span class="stat-number">4.9</span>
                  <span class="stat-label">{{ 'home.stats.rating' | translate }}</span>
                </div>
              </div>
            </div>

            <div class="hero-visual">
              <div class="book-stack">
                @for (book of featuredBooks.slice(0, 3); track book.bookId) {
                  <div class="book book-{{ $index + 1 }}">
                    <img [src]="book.images" [alt]="book.title" />
                  </div>
                }
              </div>
              <div class="floating-elements">
                <div class="element element-1">📚</div>
                <div class="element element-2">✨</div>
                <div class="element element-3">💡</div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Featured Books -->
      <section class="featured-section">
        <div class="container">
          <div class="section-header">
            <div class="section-title-group">
              <h2 class="section-title">{{ 'home.featuredSection.title' | translate }}</h2>
              <p class="section-subtitle">{{ 'home.featuredSection.subtitle' | translate }}</p>
            </div>
            <a routerLink="/books" class="view-all-link">
              {{ 'home.viewAll' | translate }} →
            </a>
          </div>

          <div class="featured-grid" *ngIf="!isLoading; else loadingSkeleton">
            <div class="featured-book" *ngFor="let book of featuredBooks; trackBy: trackByBookId">
              <div class="book-cover">
                <img [src]="book.images[0]" [alt]="book.title" />
                <div class="book-overlay">
                  <button class="quick-view-btn" (click)="navigateToBook(book.bookId)">{{ 'home.quickView' | translate }}</button>
                </div>
              </div>
              <div class="book-info">
                <div class="book-meta">
                  <span class="book-category">{{ getLangText(book.category.name)}}</span>
                  <div class="book-rating">
                    <span class="rating-stars">★★★★★</span>
                    <span class="rating-count">({{ book.reviewCount }})</span>
                  </div>
                </div>
                <h3 class="book-title">
                  <a [routerLink]="['/books', book.bookId]">{{ getLangText(book.title) }}</a>
                </h3>
                <p class="book-author">by {{ book.authorName }}</p>
                <div class="book-price">
                <span class="current-price">
                  {{ fmtPrice(book?.currentPrice, getCurrentLang()) }}
                </span>
                  <span class="original-price" *ngIf="book.price">
                  {{ fmtPrice(book.price, getCurrentLang()) }}
                </span>
                </div>
              </div>
            </div>
          </div>

          <ng-template #loadingSkeleton>
            <div class="featured-grid">
              <div class="featured-book skeleton-book" *ngFor="let item of [1,2,3]">
                <div class="skeleton book-cover-skeleton"></div>
                <div class="book-info">
                  <div class="skeleton skeleton-text"></div>
                  <div class="skeleton skeleton-title"></div>
                  <div class="skeleton skeleton-author"></div>
                  <div class="skeleton skeleton-price"></div>
                </div>
              </div>
            </div>
          </ng-template>
        </div>
      </section>

      <!-- Categories -->
      <section class="categories-section">
        <div class="container">
          <div class="categories-content">
            <div class="categories-text">
              <h2 class="section-title">{{ 'home.categories.title' | translate }}</h2>
              <p class="section-description">
                {{ 'home.categories.description' | translate }}
              </p>
              <a routerLink="/books" class="btn btn-accent">
                {{ 'home.categories.browseAll' | translate }}
              </a>
            </div>

            <div class="categories-grid" *ngIf="!isCategoriesLoading; else categoriesLoadingSkeleton">
              <div class="category-card" *ngFor="let category of categories; trackBy: trackByCategoryId">
                <div class="category-icon">{{ category.icon }}</div>
                <h3 class="category-name">{{ getLangText(category.name) }}</h3>
                <p class="category-count">
                  {{ category.bookCount }} {{ 'home.categories.books' | translate }}
                </p>
              </div>
            </div>

            <ng-template #categoriesLoadingSkeleton>
              <div class="categories-grid">
                <div class="category-card skeleton-category" *ngFor="let item of [1,2,3,4,5,6]">
                  <div class="skeleton category-icon-skeleton"></div>
                  <div class="skeleton skeleton-text"></div>
                  <div class="skeleton skeleton-count"></div>
                </div>
              </div>
            </ng-template>

            <div class="error-message" *ngIf="categoryError">
              <p>{{ 'errors.loadCategories' | translate }}</p>
              <button class="btn btn-secondary" (click)="retryLoadCategories()">
                {{ 'common.retry' | translate }}
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- Newsletter -->
      <section class="newsletter-section">
        <div class="container">
          <div class="newsletter-content">
            <div class="newsletter-text">
              <h2 class="newsletter-title">{{ 'home.newsletter.title' | translate }}</h2>
              <p class="newsletter-description">{{ 'home.newsletter.description' | translate }}</p>
            </div>
            <form class="newsletter-form">
              <input
                type="email"
                [attr.placeholder]="'home.newsletter.placeholder' | translate"
                class="newsletter-input"
              />
              <button type="submit" class="btn btn-primary">
                {{ 'home.newsletter.subscribe' | translate }}
              </button>
            </form>
          </div>
        </div>
      </section>
    </main>
  `,
  styleUrls: ["home.component.scss"],
})
export class HomeComponent implements OnInit, OnDestroy {
  trendingBooks: Book[] = []
  featuredBooks: Book[] = []
  categories: Category[] = []
  isLoading = true
  isCategoriesLoading = false
  categoryError: string | null = null
  private destroy$ = new Subject<void>()
  private languageService = inject(LanguageService)

  constructor(
    private bookService: BookService,
    private store: Store<AppState>,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadFeaturedBooks()
    this.loadCategories()
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  private loadFeaturedBooks(): void {
    this.bookService
      .getFeaturedBooks()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (books) => {
          this.featuredBooks = books
          this.isLoading = false
        },
        error: (error) => {
          console.error("Error loading featured books:", error)
          this.isLoading = false
        },
      })
  }

  private loadCategories(): void {
    this.store.dispatch(loadCategories())

    this.store
      .select(selectAllCategories)
      .pipe(takeUntil(this.destroy$))
      .subscribe((categories) => {
        this.categories = categories
      })

    this.store
      .select(selectCategoryLoading)
      .pipe(takeUntil(this.destroy$))
      .subscribe((loading) => {
        this.isCategoriesLoading = loading
      })

    this.store
      .select(selectCategoryError)
      .pipe(takeUntil(this.destroy$))
      .subscribe((error) => {
        this.categoryError = error?.message || null
      })
  }

  retryLoadCategories(): void {
    this.store.dispatch(loadCategories())
  }

  trackByBookId(index: number, book: Book): string {
    return book.bookId
  }

  trackByCategoryId(index: number, category: Category): string {
    return category.id
  }

  navigateToBook(id: string): void {
    this.router.navigate([`/books/${id}`])
  }

  protected readonly getLangText = getLangText;
  protected readonly getCurrentLang = getCurrentLang;
  protected readonly fmtPrice = formatPrice;
}
