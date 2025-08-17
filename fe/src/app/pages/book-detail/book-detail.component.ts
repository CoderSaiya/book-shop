import { Component, type OnInit, type OnDestroy } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule, ActivatedRoute, Router } from "@angular/router"
import { TranslateModule } from "@ngx-translate/core"
import { HeaderComponent } from "../../shared/components/header/header.component"
import { BookReviewsComponent } from "../../shared/components/book-reviews/book-reviews.component"
import { RelatedBooksComponent } from "../../shared/components/related-books/related-books.component"
import { BookService } from "../../core/services/book.service"
import { CartService } from "../../core/services/cart.service"
import type { Book } from "../../models/book.model"
import { Subject } from "rxjs"
import { takeUntil, switchMap } from "rxjs/operators"
import {getCurrentLang, getLangText} from '../../shared/utils/lang';
import {discountPercent, formatPrice} from '../../shared/utils/currency';
import {NotifyService} from '../../core/services/notify.service';

@Component({
  selector: "app-book-detail",
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, HeaderComponent, BookReviewsComponent, RelatedBooksComponent],
  template: `
    <app-header></app-header>

    <main class="book-detail-page">
      <div class="container">
        <nav class="breadcrumb">
          <a routerLink="/">{{ 'nav.home' | translate }}</a>
          <span class="separator">›</span>
          <a routerLink="/books">{{ 'nav.books' | translate }}</a>
          <span class="separator">›</span>
          <span class="current">{{ book === undefined ? ('common.loading' | translate) : getLangText(book.title) }}</span>
        </nav>

        <div class="book-detail-content" *ngIf="!isLoading && book; else loadingSkeleton">
          <div class="book-main">
            <div class="book-image">
              <img [src]="book.images" [alt]="book.title"/>
              <div class="book-badges" *ngIf="book.bestseller">
                <span class="badge bestseller" *ngIf="book.bestseller">Bestseller</span>
                <!--                <span class="badge new-release" *ngIf="book.newRelease">New Release</span>-->
              </div>
            </div>

            <div class="book-info">
              <div class="book-category">{{ getLangText(book.category.name) | titlecase }}</div>
              <h1 class="book-title">{{ getLangText(book.title) }}</h1>
              <p class="book-author">{{ 'bookDetail.author' | translate }}: {{ book.authorName }}</p>

              <div class="book-rating">
                <div class="rating-stars">
                  <span class="stars">★★★★★</span>
                  <span class="rating-value">{{ book.rating }}</span>
                </div>
                <span class="rating-count">({{ book.reviewCount }} {{ 'bookDetail.reviews' | translate }})</span>
              </div>

              <div class="book-price">
                <span class="current-price">
                  {{ fmtPrice(book?.currentPrice, getCurrentLang()) }}
                </span>
                <span class="original-price" *ngIf="book.price">
                  {{ fmtPrice(book.price, getCurrentLang()) }}
                </span>
                <span class="discount" *ngIf="book.price">
                  {{ getCurrentLang() === 'vi' ? 'Giảm giá' : '' }}
                  {{ pctOff(book.price, book.currentPrice) }}%
                  {{ getCurrentLang() === 'en' ? ' off' : '' }}
                </span>
              </div>

              <div class="book-stock" [class.out-of-stock]="book.isSold">
                <span *ngIf="!book.isSold" class="in-stock">
                  ✓ {{ 'bookDetail.inStock' | translate }}
                </span>
                <span *ngIf="book.isSold" class="out-of-stock">
                  ✗ {{ 'bookDetail.outOfStock' | translate }}
                </span>
              </div>

              <div class="book-actions">
                <button
                  class="btn btn-primary add-to-cart-btn"
                  (click)="addToCart()"
                  [disabled]="book.isSold || isAddingToCart"
                >
                  <span *ngIf="!isAddingToCart">{{ 'bookDetail.addToCart' | translate }}</span>
                  <span *ngIf="isAddingToCart">{{ 'common.loading' | translate }}...</span>
                </button>
                <button class="btn btn-accent buy-now-btn" [disabled]="book.isSold">
                  {{ 'bookDetail.buyNow' | translate }}
                </button>
              </div>

              <div class="book-details">
                <h3 class="details-title">{{ 'bookDetail.title' | translate }}</h3>
                <div class="details-grid">
                  <div class="detail-item">
                    <span class="detail-label">{{ 'bookDetail.publisher' | translate }}:</span>
                    <span class="detail-value">{{ book.publisherName }}</span>
                  </div>
                  <div class="detail-item">
                    <span class="detail-label">{{ 'bookDetail.publishedDate' | translate }}:</span>
                    <span class="detail-value">{{ book.publishedDate | date:'mediumDate' }}</span>
                  </div>
                  <!--                  <div class="detail-item">-->
                  <!--                    <span class="detail-label">{{ 'bookDetail.pages' | translate }}:</span>-->
                  <!--                    <span class="detail-value">{{ book.pages }}</span>-->
                  <!--                  </div>-->
                  <!--                  <div class="detail-item">-->
                  <!--                    <span class="detail-label">{{ 'bookDetail.language' | translate }}:</span>-->
                  <!--                    <span class="detail-value">{{ book.language }}</span>-->
                  <!--                  </div>-->
                  <!--                  <div class="detail-item">-->
                  <!--                    <span class="detail-label">{{ 'bookDetail.isbn' | translate }}:</span>-->
                  <!--                    <span class="detail-value">{{ book.isbn }}</span>-->
                  <!--                  </div>-->
                </div>
              </div>
            </div>
          </div>

          <div class="book-description">
            <h2 class="section-title">{{ 'bookDetail.description' | translate }}</h2>
            <p class="description-text">{{ getLangText(book.description) }}</p>
          </div>

          <app-book-reviews [bookId]="book.bookId"></app-book-reviews>

          <app-related-books [bookId]="book.bookId" [category]="getLangText(book.category.name)"></app-related-books>
        </div>

        <ng-template #loadingSkeleton>
          <div class="book-detail-skeleton">
            <div class="book-main">
              <div class="skeleton book-image-skeleton"></div>
              <div class="book-info">
                <div class="skeleton skeleton-category"></div>
                <div class="skeleton skeleton-title"></div>
                <div class="skeleton skeleton-author"></div>
                <div class="skeleton skeleton-rating"></div>
                <div class="skeleton skeleton-price"></div>
                <div class="skeleton skeleton-stock"></div>
                <div class="skeleton-actions">
                  <div class="skeleton skeleton-button"></div>
                  <div class="skeleton skeleton-button"></div>
                </div>
              </div>
            </div>
            <div class="skeleton skeleton-description"></div>
          </div>
        </ng-template>

        <div class="error-state" *ngIf="!isLoading && !book">
          <h2>{{ 'common.error' | translate }}</h2>
          <p>Book not found</p>
          <button class="btn btn-primary" routerLink="/books">
            {{ 'common.back' | translate }} to Books
          </button>
        </div>
      </div>
    </main>
  `,
  styleUrls: ["./book-detail.component.scss"],
})
export class BookDetailComponent implements OnInit, OnDestroy {
  book: Book | undefined
  isLoading = true
  isAddingToCart = false
  private destroy$ = new Subject<void>()

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bookService: BookService,
    private cartService: CartService,
    private notify: NotifyService,
  ) {}

  ngOnInit(): void {
    this.route.params
      .pipe(
        takeUntil(this.destroy$),
        switchMap((params) => {
          this.isLoading = true
          return this.bookService.getBookById(params["id"])
        }),
      )
      .subscribe({
        next: (book) => {
          this.book = book
          this.isLoading = false
          if (!book) {
            // Book not found, could redirect to 404 or books page
            console.warn("Book not found")
          }
        },
        error: (error) => {
          console.error("Error loading book:", error)
          this.isLoading = false
        },
      })
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  addToCart(): void {
    if (!this.book || this.book.isSold) return

    this.isAddingToCart = true

    setTimeout(() => {
      this.cartService.addOrUpdateItem(this.book!, 1).subscribe();
      this.isAddingToCart = false
    }, 500)
  }

  getDiscountPercentage(): number {
    if (!this.book?.price) return 0
    return Math.round(((this.book.price - this.book.currentPrice) / this.book.price) * 100)
  }

  protected readonly getCurrentLang = getCurrentLang;
  protected readonly getLangText = getLangText;
  protected readonly fmtPrice = formatPrice;
  protected readonly pctOff = discountPercent;
}
