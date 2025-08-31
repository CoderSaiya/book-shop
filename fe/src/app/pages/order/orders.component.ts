import { Component, type OnInit, type OnDestroy, signal, computed, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import {Router, RouterModule} from "@angular/router";
import { TranslateModule } from "@ngx-translate/core";
import { HeaderComponent } from "../../shared/components/header/header.component";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import { OrdersService } from "../../core/services/order.service";
import {formatPrice} from '../../shared/utils/currency';

interface OrderItem {
  id: string;
  bookTitle: string;
  bookAuthor: string;
  quantity: number;
  price: number;
  coverImage: string;
}

interface Order {
  id: string;
  orderNumber: string;
  date: Date;
  status: "pending" | "processing" | "shipped" | "delivered" | "cancelled";
  total: number;
  items: OrderItem[];
  shippingAddress: {
    name: string;
    address: string;
    city: string;
    postalCode: string;
    phone: string;
  };
  paymentMethod: string;
  trackingNumber?: string;
}

@Component({
  selector: "app-orders",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, TranslateModule, HeaderComponent],
  template: `
    <app-header></app-header>

    <main class="orders-page">
      <div class="container">
        <div class="orders-header">
          <h1 class="page-title">{{ 'orders.title' | translate }}</h1>
          <p class="page-subtitle">{{ 'orders.subtitle' | translate }}</p>
        </div>

        <div class="orders-filters">
          <div class="filter-group">
            <label for="status-filter" class="filter-label">{{ 'orders.filterByStatus' | translate }}</label>
            <select
              id="status-filter"
              [(ngModel)]="selectedStatus"
              (ngModelChange)="onStatusChange()"
              class="filter-select"
            >
              <option value="">{{ 'orders.allOrders' | translate }}</option>
              <option value="pending">{{ 'orders.status.pending' | translate }}</option>
              <option value="processing">{{ 'orders.status.processing' | translate }}</option>
              <option value="shipped">{{ 'orders.status.shipped' | translate }}</option>
              <option value="delivered">{{ 'orders.status.delivered' | translate }}</option>
              <option value="cancelled">{{ 'orders.status.cancelled' | translate }}</option>
            </select>
          </div>

          <div class="filter-group">
            <label for="sort-filter" class="filter-label">{{ 'orders.sortBy' | translate }}</label>
            <select
              id="sort-filter"
              [(ngModel)]="sortBy"
              (ngModelChange)="onSortChange()"
              class="filter-select"
            >
              <option value="date-desc">{{ 'orders.sort.dateDesc' | translate }}</option>
              <option value="date-asc">{{ 'orders.sort.dateAsc' | translate }}</option>
              <option value="total-desc">{{ 'orders.sort.totalDesc' | translate }}</option>
              <option value="total-asc">{{ 'orders.sort.totalAsc' | translate }}</option>
            </select>
          </div>
        </div>

        <div class="orders-list" *ngIf="filteredOrders().length > 0; else noOrders">
          @for (order of filteredOrders(); track order.id) {
            <div class="order-card" [ngClass]="['status-' + order.status, expandedOrderId() === order.id ? 'expanded' : '']">
              <div class="order-header" (click)="toggleOrderExpansion(order.id)">
                <div class="order-info">
                  <div class="order-number">
                    <span class="label">{{ 'orders.orderNumber' | translate }}:</span>
                    <span class="value">#{{ order.orderNumber }}</span>
                  </div>
                  <div class="order-date">
                    <span class="label">{{ 'orders.orderDate' | translate }}:</span>
                    <span class="value">{{ order.date | date:'medium' }}</span>
                  </div>
                </div>

                <div class="order-summary">
                  <div class="order-total">
                    <span class="amount">{{ fmtPrice(order.total) }}</span>
                  </div>
                  <div class="order-status">
                    <span class="status-badge" [class]="'status-' + order.status">
                      {{ ('orders.status.' + order.status) | translate }}
                    </span>
                  </div>
                </div>

                <div class="expand-icon">
                  <span [class.rotated]="expandedOrderId() === order.id">▼</span>
                </div>
              </div>

              @if (expandedOrderId() === order.id) {
                <div class="order-details">
                  <div class="order-items">
                    <h3 class="section-title">{{ 'orders.items' | translate }}</h3>
                    @for (item of order.items; track item.id) {
                      <div class="order-item">
                        <div class="item-image">
                          <img [src]="item.coverImage" [alt]="item.bookTitle" />
                        </div>
                        <div class="item-info">
                          <h4 class="item-title">{{ item.bookTitle }}</h4>
<!--                          <p class="item-author">{{ 'orders.by' | translate }} {{ item.bookAuthor }}</p>-->
                          <div class="item-details">
                            <span class="quantity">{{ 'orders.quantity' | translate }}: {{ item.quantity }}</span>
                            <span class="price">{{ fmtPrice(item.price) }}</span>
                          </div>
                        </div>
                      </div>
                    }
                  </div>

                  <div class="order-info-grid">
                    <div class="shipping-info">
                      <h3 class="section-title">{{ 'orders.shippingAddress' | translate }}</h3>
                      <div class="address-details">
                        <p class="recipient-name">{{ order.shippingAddress.name }}</p>
                        <p class="address-line">{{ order.shippingAddress.address }}</p>
                        <p class="city-postal">{{ order.shippingAddress.city }}, {{ order.shippingAddress.postalCode }}</p>
                        <p class="phone">{{ 'orders.phone' | translate }}: {{ order.shippingAddress.phone }}</p>
                      </div>
                    </div>

                    <div class="payment-info">
                      <h3 class="section-title">{{ 'orders.paymentMethod' | translate }}</h3>
                      <p class="payment-method">{{ order.paymentMethod }}</p>

                      @if (order.trackingNumber) {
                        <div class="tracking-info">
                          <h4 class="tracking-title">{{ 'orders.trackingNumber' | translate }}</h4>
                          <p class="tracking-number">{{ order.trackingNumber }}</p>
                        </div>
                      }
                    </div>
                  </div>

                  <div class="order-actions">
                    @if (order.status === 'pending' || order.status === 'processing') {
                      <button class="btn btn-outline" (click)="cancelOrder(order.id)">
                        {{ 'orders.cancelOrder' | translate }}
                      </button>
                    }

                    @if (order.trackingNumber) {
                      <button class="btn btn-primary" (click)="trackOrder(order.trackingNumber!)">
                        {{ 'orders.trackOrder' | translate }}
                      </button>
                    }

                    @if (canPayOnline(order)) {
                      <button class="btn btn-primary" (click)="payOrder(order)">
                        Thanh toán ngay
                      </button>
                    }

                    <button class="btn btn-outline" (click)="reorderItems(order.id)">
                      {{ 'orders.reorder' | translate }}
                    </button>
                  </div>
                </div>
              }
            </div>
          }
        </div>

        <nav class="pagination-bar" *ngIf="filteredOrders().length > 0">
          <div class="left">
            <button class="btn btn-outline" (click)="goPrev()" [disabled]="currentPage === 1 || loading">« Trước</button>
            <span class="page-indicator">Trang {{ currentPage }}</span>
            <button class="btn btn-outline" (click)="goNext()" [disabled]="lastPageReached || loading">Sau »</button>
          </div>
          <div class="right">
            <label>Hiển thị</label>
            <select class="page-size" [ngModel]="pageSize" (ngModelChange)="changePageSize($event)">
              <option [ngValue]="5">5</option>
              <option [ngValue]="10">10</option>
              <option [ngValue]="20">20</option>
              <option [ngValue]="50">50</option>
            </select>
            <span>mục / trang</span>
          </div>
        </nav>

        <ng-template #noOrders>
          <div class="no-orders">
            <div class="no-orders-icon">📦</div>
            <h2 class="no-orders-title">{{ 'orders.noOrders' | translate }}</h2>
            <p class="no-orders-message">{{ 'orders.noOrdersMessage' | translate }}</p>
            <a routerLink="/books" class="btn btn-primary">
              {{ 'orders.startShopping' | translate }}
            </a>
          </div>
        </ng-template>
      </div>
    </main>
  `,
  styleUrls: ["./orders.component.scss"],
})

export class OrdersComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private ordersSvc = inject(OrdersService);
  private router = inject(Router);

  // paging states
  currentPage = 1;
  pageSize = 10;
  lastPageReached = false;
  loading = false;

  orders = signal<Order[]>([]);
  selectedStatus = "";
  sortBy = "date-desc";
  expandedOrderId = signal<string | null>(null);

  filteredOrders = computed(() => {
    let filtered = this.orders();
    if (this.selectedStatus) {
      filtered = filtered.filter((order) => order.status === this.selectedStatus);
    }
    return this.sortOrders(filtered);
  });

  ngOnInit(): void {
    this.loadOrders();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // load data
  private loadOrders(): void {
    this.loading = true;
    this.ordersSvc.getMyOrders(this.currentPage, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (dtos) => {
          console.table(dtos?.map(o => ({
            id: o.id, orderNumber: o.orderNumber, total: o.totalAmount, status: o.status, paymentMethod: o.paymentMethod, paymentStatus: o.paymentStatus
          })));
          const mapped = (dtos ?? []).map(this.mapOrderDto);
          this.orders.set(mapped);
          this.lastPageReached = (dtos?.length ?? 0) < this.pageSize;
        },
        error: (e) => console.error('getMyOrders failed:', e),
        complete: () => this.loading = false
      });
  }

  goPrev(): void {
    if (this.currentPage > 1 && !this.loading) {
      this.currentPage--; this.loadOrders();
    }
  }
  goNext(): void {
    if (!this.lastPageReached && !this.loading) {
      this.currentPage++; this.loadOrders();
    }
  }
  changePageSize(size: number | string): void {
    this.pageSize = Number(size) || 10;
    this.currentPage = 1;
    this.loadOrders();
  }

  // Map OrderDetailRes -> Order (UI)
  private mapOrderDto = (dto: any): Order => {
    const createdAt = this.safeDate(dto?.createdAt);
    const items = this.mapItems(dto?.items ?? []);

    return {
      id: String(dto?.id ?? dto?.orderId ?? cryptoRandom()),
      orderNumber: String(dto?.orderNumber ?? dto?.code ?? dto?.id ?? "—"),
      date: createdAt ?? new Date(),
      status: this.mapStatus(dto?.status, dto?.paymentStatus),
      total: this.toNumber(dto?.totalAmount ?? dto?.total ?? 0),
      items,
      shippingAddress: {
        name: this.pickFirst<string>([
          dto?.recipientName,
          dto?.receiverName,
          dto?.fullName,
          dto?.userName,
          "—",
        ]),
        address: String(dto?.shippingAddress ?? dto?.address ?? "—"),
        city: String(dto?.shippingCity ?? dto?.city ?? "—"),
        postalCode: String(dto?.shippingPostalCode ?? dto?.postalCode ?? "—"),
        phone: this.pickFirst<string>([
          dto?.shippingPhone,
          dto?.phone,
          dto?.contactPhone,
          "—",
        ])
      },
      paymentMethod: this.pickPaymentMethod(dto),
      trackingNumber: dto?.trackingNumber ?? dto?.trackingCode ?? undefined
    };
  };

  private mapItems = (items: any[]): OrderItem[] => {
    return (items ?? []).map((it: any, idx: number) => {
      const qty = this.toNumber(it?.quantity ?? it?.qty ?? 1);
      const unit = this.toNumber(
        it?.unitPrice ?? it?.price ?? (this.toNumber(it?.totalPrice) / (qty || 1))
      );

      // book info (thử bắt nhiều key phổ biến)
      const title = this.pickFirst<string>([
        it?.bookTitle,
        it?.title,
        it?.book?.titleVi,
        it?.book?.titleEn,
        it?.book?.title,
        "Unknown"
      ]);

      const author = this.pickFirst<string>([
        it?.bookAuthor,
        it?.author,
        it?.book?.authorName,
        it?.book?.author,
        "—"
      ]);

      return {
        id: String(it?.id ?? it?.orderItemId ?? it?.bookId ?? idx),
        bookTitle: String(title),
        bookAuthor: String(author),
        quantity: qty,
        price: unit,
        coverImage: it.coverImage
      };
    });
  };

  private normalizeProvider(pm?: string | null): 'momo' | 'vnpay' | null {
    if (!pm) return null;
    const s = pm.toLowerCase().replace(/\s+/g, '');
    if (s.includes('momo')) return 'momo';
    if (s.includes('vnpay') || s.includes('vn-pay') || s.includes('vnppay')) return 'vnpay';
    return null;
  }

  private normalizePaymentMethod(pm?: string | number | null): 'momo' | 'vnpay' | 'cod' | null {
    if (pm == null) return null;
    const s = String(pm).toLowerCase().replace(/\s+/g, '');

    if (s.includes('momo')) return 'momo';
    if (s.includes('vnpay') || s.includes('vn-pay')) return 'vnpay';
    if (s.includes('cashondelivery') || s === 'cod') return 'cod';
    return null;
  }

  private isPaymentPending(ps?: string | number | null): boolean {
    if (ps == null) return false;
    if (typeof ps === 'number') return ps === 0;
    const s = String(ps).toLowerCase();
    return s === '0' || s === 'pending';
  }

  canPayOnline(order: Order): boolean {
    const method = this.normalizePaymentMethod(order.paymentMethod);
    if (method !== 'momo' && method !== 'vnpay') return false;     // chỉ MoMo/VNPAY
    if (!this.isPaymentPending(order.status)) return false; // chỉ khi chưa thanh toán
    return true;
  }

  shouldShowPayNow(order: Order): boolean {
    // chỉ cho thanh toán khi đơn vẫn pending
    if (order.status.toLowerCase() !== 'pending') return false;
    return this.normalizeProvider(order.paymentMethod) !== null;
  }

  providerLabel(order: Order): string {
    const p = this.normalizeProvider(order.paymentMethod);
    return p === 'momo' ? 'MoMo' : p === 'vnpay' ? 'VNPAY' : '';
  }

  payOrder(order: Order) {
    const p = this.normalizeProvider(order.paymentMethod);
    if (!p) return;
    this.router.navigate(
      ['/payment/start'],
      { queryParams: { provider: p, amount: order.total, shopOrderId: order.id } }
    );
  }

  payOrderWith(order: Order, provider: 'momo' | 'vnpay') {
    this.router.navigate(
      ['/payment/start'],
      { queryParams: { provider, amount: order.total, shopOrderId: order.id } }
    );
  }

  // ====== HELPERS ======
  private toNumber(v: any): number {
    const n = Number(v);
    return Number.isFinite(n) ? n : 0;
    // nếu cần format tiền tệ VND, dùng pipe ở template thay vì .toFixed(2)
  }

  private safeDate(v: any): Date | null {
    const d = v ? new Date(v) : null;
    return d && !isNaN(d.getTime()) ? d : null;
  }

  private mapStatus(status?: string, paymentStatus?: string): Order["status"] {
    const s = String(status || "").toLowerCase();
    const p = String(paymentStatus || "").toLowerCase();

    // các nhánh phổ biến
    if (["pending", "created", "awaitingpayment"].includes(s)) return "pending";
    if (["processing", "confirmed", "paid"].includes(s)) return "processing";
    if (["shipped", "intransit"].includes(s)) return "shipped";
    if (["delivered", "completed"].includes(s)) return "delivered";
    if (["cancelled", "canceled", "failed"].includes(s)) return "cancelled";

    // fallback theo paymentStatus
    if (["unpaid", "pending"].includes(p)) return "pending";
    if (["paid", "authorized"].includes(p)) return "processing";
    if (["refunded", "void", "failed"].includes(p)) return "cancelled";

    return "processing";
  }

  private pickPaymentMethod(dto: any): string {
    // nếu BE có field: dto.paymentMethod => dùng luôn
    if (dto?.paymentMethod) return String(dto.paymentMethod);

    // thử suy đoán theo paymentStatus / gateway / flags
    const gw = dto?.paymentGateway ?? dto?.provider ?? dto?.channel ?? "";
    if (gw) return String(gw);

    // cuối cùng: hiển thị paymentStatus cho người dùng hiểu
    return String(dto?.paymentStatus ?? "—");
  }

  private pickFirst<T = any>(candidates: any[]): T {
    for (const v of candidates) {
      if (v !== undefined && v !== null && String(v).trim() !== "") return v as T;
    }
    return candidates[candidates.length - 1] as T;
  }

  // ====== UI handlers ======
  onStatusChange(): void { /* no-op: chỉ để trigger computed */ }
  onSortChange(): void { /* no-op */ }

  private sortOrders(orders: Order[]): Order[] {
    return [...orders].sort((a, b) => {
      switch (this.sortBy) {
        case "date-desc": return b.date.getTime() - a.date.getTime();
        case "date-asc":  return a.date.getTime() - b.date.getTime();
        case "total-desc": return b.total - a.total;
        case "total-asc":  return a.total - b.total;
        default: return 0;
      }
    });
  }

  toggleOrderExpansion(orderId: string): void {
    this.expandedOrderId.set(this.expandedOrderId() === orderId ? null : orderId);
  }

  cancelOrder(orderId: string): void {
    const updated = this.orders().map((o) => o.id === orderId ? { ...o, status: "cancelled" as const } : o);
    this.orders.set(updated);
  }

  trackOrder(trackingNumber: string): void {
    window.open(`https://tracking.example.com/${trackingNumber}`, "_blank");
  }

  reorderItems(orderId: string): void {
    const order = this.orders().find((o) => o.id === orderId);
    if (order) {
      console.log("Reordering items from order:", orderId);
    }
  }

  protected readonly fmtPrice = formatPrice;
}

// tạo id tạm nếu thiếu
function cryptoRandom(): string {
  try { return crypto.randomUUID(); } catch { return Math.random().toString(36).slice(2); }
}
