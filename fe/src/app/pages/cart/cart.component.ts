import {Component, type OnInit, type OnDestroy, computed} from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { TranslateModule } from "@ngx-translate/core";
import { HeaderComponent } from "../../shared/components/header/header.component";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import type { CombinedCartItem as CartItem } from "../../models/cart.model";
import { CartService } from "../../core/services/cart.service";
import {getLangText, getCurrentLang} from '../../shared/utils/lang';
import {formatPrice} from '../../shared/utils/currency';

@Component({
  selector: "app-cart",
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, HeaderComponent],
  template: `
    <app-header></app-header>

    <main class="cart-page">
      <div class="container">
        <div class="cart-header">
          <h1 class="page-title">{{ 'cart.title' | translate }}</h1>
          <p class="cart-count">{{ cartItems.length }} {{ 'cart.items' | translate }}</p>
        </div>

        <div class="cart-content" *ngIf="cartItems.length > 0; else emptyCart">
          <div class="cart-items">
            <div class="cart-item" *ngFor="let item of cartItems; trackBy: trackByItemId">
              <div class="item-image">
                <img [src]="item.book.images[0]" [alt]="item.book.title" />
              </div>

              <div class="item-details">
                <h3 class="item-title">{{ getLangText(item.book.title) }}</h3>
                <p class="item-author">{{ 'book.by' | translate }} {{ item.book.authorName }}</p>
                <p class="item-price">{{ fmtPrice(item.unitPrice, getCurrentLang()) }}</p>
              </div>

              <div class="item-quantity">
                <button class="qty-btn" (click)="updateQuantity(item.book.bookId, -1)" [disabled]="item.quantity <= 1">-</button>
                <span class="qty-value">{{ item.quantity }}</span>
                <button class="qty-btn" (click)="updateQuantity(item.book.bookId, 1)">+</button>
              </div>

              <div class="item-total">
                <p class="total-price">{{ fmtPrice(item.totalPrice, getCurrentLang()) }}</p>
                <button class="remove-btn" (click)="removeItem(item.book.bookId)" [attr.aria-label]="'cart.removeItem' | translate">
                  {{ 'cart.remove' | translate }}
                </button>
              </div>
            </div>
          </div>

          <div class="cart-summary">
            <div class="summary-card">
              <h3 class="summary-title">{{ 'cart.orderSummary' | translate }}</h3>

              <div class="summary-row">
                <span>{{ 'cart.subtotal' | translate }}</span>
                <span>{{ fmtPrice(subtotal, getCurrentLang()) }}</span>
              </div>

              <div class="summary-row">
                <span>{{ 'cart.shipping' | translate }}</span>
                <span>{{ fmtPrice(shipping, getCurrentLang()) }}</span>
              </div>

              <div class="summary-row">
                <span>{{ 'cart.tax' | translate }}</span>
                <span>{{ fmtPrice(tax, getCurrentLang()) }}</span>
              </div>

              <div class="summary-divider"></div>

              <div class="summary-row total">
                <span>{{ 'cart.total' | translate }}</span>
                <span>{{ fmtPrice(total, getCurrentLang()) }}</span>
              </div>

              <button class="btn btn-primary btn-full checkout-btn" routerLink="/checkout">
                {{ 'cart.proceedToCheckout' | translate }}
              </button>
            </div>
          </div>
        </div>

        <ng-template #emptyCart>
          <div class="empty-cart">
            <div class="empty-icon">📚</div>
            <h2>{{ 'cart.empty' | translate }}</h2>
            <p>{{ 'cart.emptyMessage' | translate }}</p>
            <button class="btn btn-primary" routerLink="/books">
              {{ 'cart.continueShopping' | translate }}
            </button>
          </div>
        </ng-template>
      </div>
    </main>
  `,
  styleUrls: ["./cart.component.scss"],
})
export class CartComponent implements OnInit, OnDestroy {
  cartItems: CartItem[] = [];
  subtotal = 0;
  shipping = 25000;
  tax = 0;
  total = 0;
  private destroy$ = new Subject<void>();

  constructor(private cartService: CartService) {}

  ngOnInit(): void {
    // Load cart từ server và bind ra VM items
    this.cartService.loadAndGetCombinedItems()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: items => {
          this.cartItems = items;
        },
        error: e => console.error('Load cart error:', e)
      });

    // Subtotal theo server
    this.cartService.getCartTotal()
      .pipe(takeUntil(this.destroy$))
      .subscribe(sub => {
        this.subtotal = sub;
        this.recalcTotal();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  trackByItemId(index: number, item: CartItem): string {
    return item.book.bookId;
  }

  updateQuantity(bookId: string, newQuantity: number): void {
    // if (newQuantity <= 0) return;
    this.cartService.updateQuantity(bookId, newQuantity)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.cartService.getCombinedItems()
            .pipe(takeUntil(this.destroy$))
            .subscribe(items => this.cartItems = items);
        },
        error: e => console.error('Update quantity error:', e)
      });
  }

  removeItem(bookId: string): void {
    this.cartService.removeItem(bookId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.cartItems = this.cartItems.filter(i => i.book.bookId !== bookId);
          this.recalcTotal();
        },
        error: e => console.error('Remove item error:', e)
      });
  }

  private recalcTotal(): void {
    this.tax = Math.round(this.subtotal * 0.08); // 8%
    this.total = this.subtotal + this.shipping + this.tax;
    this.shipping = this.subtotal > 300000 ? 0 : 25000;
  }

  protected readonly getLangText = getLangText;
  protected readonly fmtPrice = formatPrice;
  protected readonly getCurrentLang = getCurrentLang;
}
