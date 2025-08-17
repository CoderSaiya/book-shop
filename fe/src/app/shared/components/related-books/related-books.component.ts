import { Component, Input, type OnInit, type OnDestroy } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule } from "@angular/router"
import { TranslateModule } from "@ngx-translate/core"
import { BookService } from "../../../core/services/book.service"
import { CartService } from "../../../core/services/cart.service"
import type { Book } from "../../../models/book.model"
import { Subject } from "rxjs"
import { takeUntil } from "rxjs/operators"
import {getLangText} from '../../utils/lang';

@Component({
  selector: "app-related-books",
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  template: `
    <section class="related-books" *ngIf="relatedBooks.length > 0 || isLoading">
      <h2 class="section-title">{{ 'bookDetail.relatedBooks' | translate }}</h2>

      <div class="related-books-grid" *ngIf="!isLoading; else loadingSkeleton">
        <div class="book-card" *ngFor="let book of relatedBooks; trackBy: trackByBookId">
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
                class="overlay-btn secondary"
                (click)="addToCart(book)"
              >
                {{ 'books.addToCart' | translate }}
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
              <a [routerLink]="['/books', book.bookId]">{{ book.title }}</a>
            </h3>
            <p class="book-author">by {{ book.authorName }}</p>
            <div class="book-price">
              <span class="current-price">\${{ book.currentPrice }}</span>
              <span class="original-price" *ngIf="book.price">\${{ book.price }}</span>
            </div>
          </div>
        </div>
      </div>

      <ng-template #loadingSkeleton>
        <div class="related-books-grid">
          <div class="book-card skeleton-card" *ngFor="let item of [1,2,3,4]">
            <div class="skeleton book-cover-skeleton"></div>
            <div class="book-info">
              <div class="skeleton skeleton-meta"></div>
              <div class="skeleton skeleton-title"></div>
              <div class="skeleton skeleton-author"></div>
              <div class="skeleton skeleton-price"></div>
            </div>
          </div>
        </div>
      </ng-template>
    </section>
  `,
  styleUrls: ["./related-books.component.scss"],
})
export class RelatedBooksComponent implements OnInit, OnDestroy {
  @Input() bookId!: string
  @Input() category!: string

  relatedBooks: Book[] = []
  isLoading = true
  private destroy$ = new Subject<void>()

  constructor(
    private bookService: BookService,
    private cartService: CartService,
  ) {}

  ngOnInit(): void {
    if (this.bookId && this.category) {
      // this.loadRelatedBooks()
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  // private loadRelatedBooks(): void {
  //   this.bookService
  //     .getRelatedBooks(this.bookId, this.category)
  //     .pipe(takeUntil(this.destroy$))
  //     .subscribe({
  //       next: (books) => {
  //         this.relatedBooks = books
  //         this.isLoading = false
  //       },
  //       error: (error) => {
  //         console.error("Error loading related books:", error)
  //         this.isLoading = false
  //       },
  //     })
  // }

  addToCart(book: Book): void {
    this.cartService.addOrUpdateItem(book, 1).subscribe();
  }

  trackByBookId(index: number, book: Book): string {
    return book.bookId
  }

  protected readonly getLangText = getLangText;
}
