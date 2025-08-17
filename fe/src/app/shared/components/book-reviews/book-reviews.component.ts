import { Component, Input, type OnInit, type OnDestroy } from "@angular/core"
import { CommonModule } from "@angular/common"
import { TranslateModule } from "@ngx-translate/core"
import { BookService } from "../../../core/services/book.service"
import { Subject } from "rxjs"
import { takeUntil } from "rxjs/operators"

@Component({
  selector: "app-book-reviews",
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <section class="book-reviews">
<!--      <div class="reviews-header">-->
<!--        <h2 class="section-title">{{ 'bookDetail.reviews' | translate }}</h2>-->
<!--        <button class="btn btn-secondary write-review-btn">-->
<!--          Write a Review-->
<!--        </button>-->
<!--      </div>-->

<!--      <div class="reviews-content" *ngIf="!isLoading; else loadingSkeleton">-->
<!--        <div class="reviews-list" *ngIf="reviews.length > 0; else noReviews">-->
<!--          <div class="review-item" *ngFor="let review of reviews; trackBy: trackByReviewId">-->
<!--            <div class="review-header">-->
<!--              <div class="reviewer-info">-->
<!--                <img-->
<!--                  *ngIf="review.userAvatar; else defaultAvatar"-->
<!--                  [src]="review.userAvatar"-->
<!--                  [alt]="review.userName"-->
<!--                  class="reviewer-avatar"-->
<!--                />-->
<!--                <ng-template #defaultAvatar>-->
<!--                  <div class="reviewer-avatar default">{{ getInitials(review.userName) }}</div>-->
<!--                </ng-template>-->
<!--                <div class="reviewer-details">-->
<!--                  <h4 class="reviewer-name">{{ review.userName }}</h4>-->
<!--                  <div class="review-meta">-->
<!--                    <div class="review-rating">-->
<!--                      <span class="stars">{{ getStars(review.rating) }}</span>-->
<!--                      <span class="rating-value">{{ review.rating }}/5</span>-->
<!--                    </div>-->
<!--                    <span class="review-date">{{ review.date | date:'mediumDate' }}</span>-->
<!--                  </div>-->
<!--                </div>-->
<!--              </div>-->
<!--            </div>-->

<!--            <div class="review-content">-->
<!--              <h5 class="review-title">{{ review.title }}</h5>-->
<!--              <p class="review-comment">{{ review.comment }}</p>-->

<!--              <div class="review-actions">-->
<!--                <button class="helpful-btn" [class.active]="false">-->
<!--                  👍 Helpful ({{ review.helpful }})-->
<!--                </button>-->
<!--                <button class="report-btn">Report</button>-->
<!--              </div>-->
<!--            </div>-->
<!--          </div>-->
<!--        </div>-->

<!--        <ng-template #noReviews>-->
<!--          <div class="no-reviews">-->
<!--            <p>No reviews yet. Be the first to review this book!</p>-->
<!--          </div>-->
<!--        </ng-template>-->
<!--      </div>-->

<!--      <ng-template #loadingSkeleton>-->
<!--        <div class="reviews-skeleton">-->
<!--          <div class="review-skeleton" *ngFor="let item of [1,2,3]">-->
<!--            <div class="skeleton reviewer-avatar-skeleton"></div>-->
<!--            <div class="review-content-skeleton">-->
<!--              <div class="skeleton skeleton-name"></div>-->
<!--              <div class="skeleton skeleton-rating"></div>-->
<!--              <div class="skeleton skeleton-title"></div>-->
<!--              <div class="skeleton skeleton-comment"></div>-->
<!--            </div>-->
<!--          </div>-->
<!--        </div>-->
<!--      </ng-template>-->
    </section>
  `,
  styleUrls: ["./book-reviews.component.scss"],
})
export class BookReviewsComponent implements OnInit, OnDestroy {
  @Input() bookId!: string

  // reviews: BookReview[] = []
  // isLoading = true
  // private destroy$ = new Subject<void>()

  constructor(private bookService: BookService) {}

  ngOnInit(): void {
    if (this.bookId) {
      // this.loadReviews()
    }
  }

  ngOnDestroy(): void {
    // this.destroy$.next()
    // this.destroy$.complete()
  }

  // private loadReviews(): void {
  //   this.bookService
  //     .getBookReviews(this.bookId)
  //     .pipe(takeUntil(this.destroy$))
  //     .subscribe({
  //       next: (reviews) => {
  //         this.reviews = reviews
  //         this.isLoading = false
  //       },
  //       error: (error) => {
  //         console.error("Error loading reviews:", error)
  //         this.isLoading = false
  //       },
  //     })
  // }
  //
  // getStars(rating: number): string {
  //   const fullStars = Math.floor(rating)
  //   const emptyStars = 5 - fullStars
  //   return "★".repeat(fullStars) + "☆".repeat(emptyStars)
  // }
  //
  // getInitials(name: string): string {
  //   return name
  //     .split(" ")
  //     .map((n) => n[0])
  //     .join("")
  //     .toUpperCase()
  //     .slice(0, 2)
  // }
  //
  // trackByReviewId(index: number, review: BookReview): string {
  //   return review.id
  // }
}
