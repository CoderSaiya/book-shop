import {Component, type OnInit, type OnDestroy, inject} from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule } from "@angular/router"
import { FormsModule } from "@angular/forms"
import { TranslateModule } from "@ngx-translate/core"
import { HeaderComponent } from "../../shared/components/header/header.component"
import { BookService } from "../../core/services/book.service"
import { CartService } from "../../core/services/cart.service"
import type { Book } from "../../models/book.model"
import {finalize, Subject} from "rxjs"
import {takeUntil, debounceTime, distinctUntilChanged, switchMap} from "rxjs/operators"
import {getCurrentLang, getLangText} from '../../shared/utils/lang';
import {formatPrice} from '../../shared/utils/currency';
import {NotifyService} from '../../core/services/notify.service';

@Component({
  selector: "app-books",
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule, HeaderComponent],
  template: `
    <app-header></app-header>

    <main class="books-page">
      <div class="container">
        <div class="page-header">
          <h1 class="page-title">{{ 'books.title' | translate }}</h1>
          <p class="page-subtitle">{{ 'books.desc' | translate }}</p>
        </div>

        <div class="books-content">
          <!-- Filters Sidebar -->
          <aside class="filters-sidebar">
            <div class="filter-section">
              <h3 class="filter-title">{{ 'books.searchTitle' | translate }}</h3>
              <input
                type="text"
                [(ngModel)]="searchQuery"
                (ngModelChange)="onSearchChange($event)"
                placeholder="{{ 'books.search' | translate }}"
                class="search-input"
              />
            </div>

            <div class="filter-section">
              <h3 class="filter-title">{{ 'books.categories' | translate }}</h3>
              <div class="filter-options">
                <label class="filter-option" *ngFor="let category of categories">
                  <input
                    type="checkbox"
                    [value]="category.slug"
                    (change)="onCategoryChange($event)"
                  />
                  <span>{{ category.name }} ({{ category.bookCount }})</span>
                </label>
              </div>
            </div>

            <div class="filter-section">
              <h3 class="filter-title">{{ 'books.priceRange' | translate }}</h3>
              <div class="price-range">
                <input
                  type="range"
                  min="0"
                  [max]="maxPrice"
                  [(ngModel)]="priceRange.max"
                  (ngModelChange)="onPriceChange()"
                  class="price-slider"
                />
                <div class="price-display">
                  0 - {{ priceRange.max | number:'1.0-0' }}
                </div>
              </div>
            </div>
          </aside>

          <!-- Books Grid -->
          <div class="books-main">
            <div class="books-toolbar">
              <div class="results-count">
                {{ filteredBooks.length }}
                {{ (filteredBooks.length === 1
                ? 'books.found.singular'
                : 'books.found.plural') | translate:{ count: filteredBooks.length } }}
              </div>
              <select [(ngModel)]="sortBy" (ngModelChange)="onSortChange()" class="sort-select">
                <option value="title">{{ 'books.sortBy.title' | translate }}</option>
                <option value="author">{{ 'books.sortBy.author' | translate }}</option>
                <option value="price-low">{{ 'books.sortBy.price.l2h' | translate }}</option>
                <option value="price-high">{{ 'books.sortBy.price.h2l' | translate }}</option>
                <option value="rating">{{ 'books.sortBy.rate' | translate }}</option>
              </select>
            </div>

            <div class="books-grid" *ngIf="!isLoading; else loadingSkeleton">
              <div class="book-card" *ngFor="let book of filteredBooks; trackBy: trackByBookId">
                <div class="book-cover">
                  <img [src]="book.images[0]" [alt]="book.title" />
                  <div class="book-overlay">
                    <button
                      class="overlay-btn"
                      [routerLink]="['/books', book.bookId]"
                    >
                      {{ 'books.viewDetails' | translate }}
                    </button>
                    <button
                      class="btn add-to-cart-btn"
                      (click)="addToCart(book)"
                      [disabled]="book.isSold || isAddingToCart"
                    >
                      <span *ngIf="!isAddingToCart">{{ 'books.addToCart' | translate }}</span>
                      <span *ngIf="isAddingToCart">{{ 'common.loading' | translate }}...</span>
                    </button>
                  </div>
                </div>
                <div class="book-info">
                  <div class="book-meta">
                    <span class="book-category">{{ getLangText(book.category.name) | titlecase }}</span>
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
                      {{ fmtPrice(book.currentPrice, getCurrentLang()) }}
                    </span>
                    <span class="original-price" *ngIf="book.price">
                      {{ fmtPrice(book.price, getCurrentLang()) }}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <ng-template #loadingSkeleton>
              <div class="books-grid">
                <div class="book-card skeleton-card" *ngFor="let item of [1,2,3,4,5,6]">
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
        </div>
      </div>
    </main>
  `,
  styleUrls: ["./books.component.scss"],
})
export class BooksComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>()
  private bookService = inject(BookService)
  private cartService = inject(CartService)
  private notify = inject(NotifyService)

  books: Book[] = []
  filteredBooks: Book[] = []
  categories: Array<{ slug: string; name: string; bookCount: number }> = []

  searchQuery = ""
  private searchSubject = new Subject<string>()

  selectedCategories: string[] = []
  priceRange = { min: 0, max: 100 }
  sortBy = "title"
  isLoading = true
  maxPrice = 0;
  isAddingToCart = false;

  ngOnInit(): void {
    this.fetchInitialBooks()
    this.setupSearch()
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  private fetchInitialBooks(): void {
    this.isLoading = true
    this.bookService
      .searchBooks()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (books) => {
          this.books = books || []
          this.recalcPriceSlider(this.books);
          this.buildCategories(this.books);
          this.applyFilters();
        },
        error: () => {
          this.books = []
          this.categories = []
          this.filteredBooks = []
        },
        complete: () => (this.isLoading = false),
      })
  }

  private buildCategories(books: Book[]): void {
    const map = new Map<string, { slug: string; name: string; bookCount: number }>()
    for (const b of books) {
      const name = getLangText(b.category?.name ?? { vi: "", en: "" })
      const slug =
        (b.category as any)?.slug ||
        name
          .toLowerCase()
          .normalize("NFD")
          .replace(/[\u0300-\u036f]/g, "")
          .replace(/\s+/g, "-")
      const cur = map.get(slug) || { slug, name, bookCount: 0 }
      cur.bookCount++
      map.set(slug, cur)
    }
    this.categories = Array.from(map.values())
  }

  private setupSearch(): void {
    this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((keyword) => {
          this.isLoading = true;
          const k = (keyword || "").trim();
          return this.bookService
            .searchBooks({ keyword: k, page: 1, pageSize: 50 })
            .pipe(finalize(() => (this.isLoading = false)));
        }),
        takeUntil(this.destroy$),
      )
      .subscribe({
        next: (books) => {
          this.books = books || []
          this.recalcPriceSlider(this.books);
          this.buildCategories(this.books)
          this.applyFilters()
        },
        error: () => {
          this.books = []
          this.categories = []
          this.filteredBooks = []
        }
      })
  }

  private recalcPriceSlider(books: Book[]): void {
    const prices = books.map(b => Number((b as any).currentPrice ?? b.price ?? 0)).filter(n => Number.isFinite(n));
    const max = prices.length ? Math.ceil(Math.max(...prices)) : 0;
    this.maxPrice = Math.max(max, 0);
    // set default filter = full range để không chặn dữ liệu ban đầu
    this.priceRange.max = this.maxPrice || 0;
  }

  onSearchChange(query: string): void {
    this.searchSubject.next(query)
  }

  onCategoryChange(event: any): void {
    const category = event.target.value
    if (event.target.checked) {
      this.selectedCategories.push(category)
    } else {
      this.selectedCategories = this.selectedCategories.filter((c) => c !== category)
    }
    this.applyFilters()
  }

  onPriceChange(): void {
    this.applyFilters()
  }

  onSortChange(): void {
    this.applyFilters()
  }

  addToCart(book: Book): void {
    if (!book || book.isSold) return

    this.isAddingToCart = true

    setTimeout(() => {
      this.cartService.addOrUpdateItem(book!, 1).subscribe();
      this.isAddingToCart = false
    }, 500)
  }

  trackByBookId(index: number, book: Book): string {
    return book.bookId
  }

  private applyFilters(): void {
    let filtered = [...this.books];

    // Search (client phụ)
    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase();
      filtered = filtered.filter(
        (book) =>
          getLangText(book.title).toLowerCase().includes(query) ||
          (book.authorName || "").toLowerCase().includes(query),
      );
    }

    // Category
    if (this.selectedCategories.length > 0) {
      filtered = filtered.filter((book) => this.selectedCategories.includes(this.getCategorySlug(book)));
    }

    // Price: chỉ lọc khi có maxPrice > 0
    if (this.maxPrice > 0) {
      filtered = filtered.filter((book) => {
        const price = Number((book as any).currentPrice ?? book.price ?? 0);
        return price <= this.priceRange.max;
      });
    }

    // Sort
    filtered.sort((a, b) => {
      const pa = Number((a as any).currentPrice ?? a.price ?? 0);
      const pb = Number((b as any).currentPrice ?? b.price ?? 0);
      switch (this.sortBy) {
        case "title":
          return getLangText(a.title).localeCompare(getLangText(b.title));
        case "author":
          return (a.authorName || "").localeCompare(b.authorName || "");
        case "price-low":
          return pa - pb;
        case "price-high":
          return pb - pa;
        default:
          return 0;
      }
    });

    this.filteredBooks = filtered;
  }

  private getCategorySlug(book: Book): string {
    const name = getLangText(book.category?.name ?? { vi: "", en: "" })
    return (
      (book.category as any)?.slug ||
      name
        .toLowerCase()
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .replace(/\s+/g, "-")
    )
  }

  protected readonly getLangText = getLangText;
  protected readonly fmtPrice = formatPrice;
  protected readonly getCurrentLang = getCurrentLang;
}
