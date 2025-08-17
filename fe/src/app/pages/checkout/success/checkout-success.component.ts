import {Component, inject, type OnInit, signal} from "@angular/core"
import { CommonModule } from "@angular/common"
import {RouterModule, ActivatedRoute, Router} from "@angular/router"
import { TranslateModule } from "@ngx-translate/core"
import { HeaderComponent } from "../../../shared/components/header/header.component"
import {OrdersService} from '../../../core/services/order.service';
import {AuthService} from '../../../core/services/auth.service';
import {OrderDetailRes} from '../../../models/order.model';
import {getCurrentLang} from '../../../shared/utils/lang';
import {formatPrice} from '../../../shared/utils/currency';

interface OrderInfo {
  orderId: string
  orderDate: Date
  estimatedDelivery: Date
  customerEmail: string
  shippingAddress: string
  paymentMethod: string
  total: number
}

@Component({
  selector: "app-checkout-success",
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, HeaderComponent],
  template: `
    <app-header></app-header>

    <main class="success-page">
      <div class="container">
        <div class="success-content">
          <!-- Success Icon & Message -->
          <div class="success-header">
            <div class="success-icon">
              <svg width="80" height="80" viewBox="0 0 80 80" fill="none">
                <circle cx="40" cy="40" r="40" fill="#10B981"/>
                <path d="M25 40L35 50L55 30" stroke="white" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </div>
            <h1 class="success-title">{{ 'checkout.success.orderPlaced' | translate }}</h1>
            <p class="success-subtitle">{{ 'checkout.success.thankYou' | translate }}</p>
          </div>

          <!-- Order Details Card -->
          <div class="order-details-card">
            <div class="card-header">
              <h2>{{ 'checkout.success.orderDetails' | translate }}</h2>
              <span class="order-id">#{{ orderInfo().orderId }}</span>
            </div>

            <div class="order-info-grid">
              <div class="info-item">
                <span class="label">{{ 'checkout.success.orderDate' | translate }}</span>
                <span class="value">{{ orderInfo().orderDate | date:'medium' }}</span>
              </div>

              <div class="info-item">
                <span class="label">{{ 'checkout.success.estimatedDelivery' | translate }}</span>
                <span class="value">{{ orderInfo().estimatedDelivery | date:'mediumDate' }}</span>
              </div>

              <div class="info-item">
                <span class="label">{{ 'checkout.success.email' | translate }}</span>
                <span class="value">{{ orderInfo().customerEmail }}</span>
              </div>

              <div class="info-item">
                <span class="label">{{ 'checkout.success.paymentMethod' | translate }}</span>
                <span class="value">{{ getPaymentMethodText(orderInfo().paymentMethod) | translate }}</span>
              </div>

              <div class="info-item full-width">
                <span class="label">{{ 'checkout.success.shippingAddress' | translate }}</span>
                <span class="value">{{ orderInfo().shippingAddress }}</span>
              </div>

              <div class="info-item total">
                <span class="label">{{ 'checkout.success.total' | translate }}</span>
                <span class="value">
                  {{ fmtPrice(orderInfo().total, getCurrentLang()) }}
                </span>
              </div>
            </div>
          </div>

          <!-- Next Steps -->
          <div class="next-steps-card">
            <h3>{{ 'checkout.success.whatNext' | translate }}</h3>
            <div class="steps-list">
              <div class="step-item">
                <div class="step-icon">📧</div>
                <div class="step-content">
                  <h4>{{ 'checkout.success.confirmationEmail' | translate }}</h4>
                  <p>{{ 'checkout.success.confirmationEmailDesc' | translate }}</p>
                </div>
              </div>

              <div class="step-item">
                <div class="step-icon">📦</div>
                <div class="step-content">
                  <h4>{{ 'checkout.success.processing' | translate }}</h4>
                  <p>{{ 'checkout.success.processingDesc' | translate }}</p>
                </div>
              </div>

              <div class="step-item">
                <div class="step-icon">🚚</div>
                <div class="step-content">
                  <h4>{{ 'checkout.success.shipping' | translate }}</h4>
                  <p>{{ 'checkout.success.shippingDesc' | translate }}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Action Buttons -->
          <div class="action-buttons">
            <button class="btn btn-primary" routerLink="/profile">
              {{ 'checkout.success.trackOrder' | translate }}
            </button>
            <button class="btn btn-secondary" routerLink="/books">
              {{ 'checkout.success.continueShopping' | translate }}
            </button>
          </div>

          <!-- Support Info -->
          <div class="support-info">
            <p>{{ 'checkout.success.needHelp' | translate }}</p>
            <div class="support-contacts">
              <span>📞 0935-234-074</span>
              <span>✉️ support&#64;bookstore.com</span>
            </div>
          </div>
        </div>
      </div>
    </main>
  `,
  styleUrls: ["./checkout-success.component.scss"],
})
export class CheckoutSuccessComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private orders = inject(OrdersService);
  private auth = inject(AuthService);

  orderInfo = signal<OrderInfo>({
    orderId: "",
    orderDate: new Date(),
    estimatedDelivery: new Date(),
    customerEmail: "",
    shippingAddress: "",
    paymentMethod: "cod",
    total: 0,
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')
    console.log(id);

    if (!id) {
      this.router.navigate(['/books']);
      return;
    }

    this.orders.getOrderById(id).subscribe({
      next: (res) => {
        const o = res?.data as OrderDetailRes;
        if (!o) return;

        const eta = this.buildEstimatedDelivery(o);

        this.orderInfo.set({
          orderId: o.orderNumber || o.id,
          orderDate: new Date(o.createdAt),
          estimatedDelivery: eta,
          customerEmail: this.auth.currentUser?.email ?? "", // backend không trả email
          shippingAddress: `${o.shippingAddress}, ${o.shippingCity}`,
          // phương thức không có trong response -> lấy từ state
          paymentMethod: window.history.state?.method ?? 'cod',
          total: Number(o.totalAmount),
        });
      },
      error: () => {
        // lỗi thì đưa user về trang sách hoặc hiển thị thông báo
        this.router.navigate(['/books']);
      }
    });
  }

  getPaymentMethodText(method: string): string {
    const methodMap: { [key: string]: string } = {
      "credit-card": "checkout.creditCard",
      paypal: "PayPal",
      "bank-transfer": "checkout.bankTransfer",
      cod: "checkout.cashOnDelivery",
    }
    return methodMap[method] || method
  }

  private buildEstimatedDelivery(o: OrderDetailRes): Date {
    const d = new Date(o.createdAt);
    d.setDate(d.getDate() + 7);
    return d;
  }

  protected readonly getCurrentLang = getCurrentLang;
  protected readonly fmtPrice = formatPrice;
}
