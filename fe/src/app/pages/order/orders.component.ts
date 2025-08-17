import { Component, type OnInit, type OnDestroy, signal, computed } from "@angular/core"
import { CommonModule } from "@angular/common"
import { FormsModule } from "@angular/forms"
import { RouterModule } from "@angular/router"
import { TranslateModule } from "@ngx-translate/core"
import { HeaderComponent } from "../../shared/components/header/header.component"
import { Subject } from "rxjs"

interface OrderItem {
  id: string
  bookTitle: string
  bookAuthor: string
  quantity: number
  price: number
  image: string
}

interface Order {
  id: string
  orderNumber: string
  date: Date
  status: "pending" | "processing" | "shipped" | "delivered" | "cancelled"
  total: number
  items: OrderItem[]
  shippingAddress: {
    name: string
    address: string
    city: string
    postalCode: string
    phone: string
  }
  paymentMethod: string
  trackingNumber?: string
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
            <div class="order-card" [class.expanded]="expandedOrderId() === order.id">
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
                    <span class="amount">{{ order.total.toFixed(2) }}</span>
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
                          <img [src]="item.image" [alt]="item.bookTitle" />
                        </div>
                        <div class="item-info">
                          <h4 class="item-title">{{ item.bookTitle }}</h4>
                          <p class="item-author">{{ 'orders.by' | translate }} {{ item.bookAuthor }}</p>
                          <div class="item-details">
                            <span class="quantity">{{ 'orders.quantity' | translate }}: {{ item.quantity }}</span>
                            <span class="price">{{ item.price.toFixed(2) }}</span>
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

                    <button class="btn btn-outline" (click)="reorderItems(order.id)">
                      {{ 'orders.reorder' | translate }}
                    </button>
                  </div>
                </div>
              }
            </div>
          }
        </div>

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
  orders = signal<Order[]>([])
  selectedStatus = ""
  sortBy = "date-desc"
  expandedOrderId = signal<string | null>(null)

  filteredOrders = computed(() => {
    let filtered = this.orders()

    if (this.selectedStatus) {
      filtered = filtered.filter((order) => order.status === this.selectedStatus)
    }

    return this.sortOrders(filtered)
  })

  private destroy$ = new Subject<void>()

  ngOnInit(): void {
    this.loadOrders()
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  private loadOrders(): void {
    const mockOrders: Order[] = [
      {
        id: "1",
        orderNumber: "ORD-2024-001",
        date: new Date("2024-01-15"),
        status: "delivered",
        total: 89.97,
        trackingNumber: "TRK123456789",
        paymentMethod: "Credit Card (**** 1234)",
        shippingAddress: {
          name: "Nguyễn Văn A",
          address: "123 Đường ABC, Quận 1",
          city: "TP. Hồ Chí Minh",
          postalCode: "70000",
          phone: "+84 901 234 567",
        },
        items: [
          {
            id: "1",
            bookTitle: "Sách Lập Trình Angular",
            bookAuthor: "Tác Giả A",
            quantity: 2,
            price: 29.99,
            image: "/placeholder.svg?height=80&width=60",
          },
          {
            id: "2",
            bookTitle: "TypeScript Nâng Cao",
            bookAuthor: "Tác Giả B",
            quantity: 1,
            price: 29.99,
            image: "/placeholder.svg?height=80&width=60",
          },
        ],
      },
      {
        id: "2",
        orderNumber: "ORD-2024-002",
        date: new Date("2024-01-10"),
        status: "shipped",
        total: 45.98,
        trackingNumber: "TRK987654321",
        paymentMethod: "PayPal",
        shippingAddress: {
          name: "Trần Thị B",
          address: "456 Đường XYZ, Quận 3",
          city: "TP. Hồ Chí Minh",
          postalCode: "70000",
          phone: "+84 902 345 678",
        },
        items: [
          {
            id: "3",
            bookTitle: "React Hooks Thực Hành",
            bookAuthor: "Tác Giả C",
            quantity: 1,
            price: 25.99,
            image: "/placeholder.svg?height=80&width=60",
          },
          {
            id: "4",
            bookTitle: "Node.js Backend",
            bookAuthor: "Tác Giả D",
            quantity: 1,
            price: 19.99,
            image: "/placeholder.svg?height=80&width=60",
          },
        ],
      },
    ]

    this.orders.set(mockOrders)
  }

  onStatusChange(): void {
    // Trigger computed property recalculation
  }

  onSortChange(): void {
    // Trigger computed property recalculation
  }

  private sortOrders(orders: Order[]): Order[] {
    return [...orders].sort((a, b) => {
      switch (this.sortBy) {
        case "date-desc":
          return b.date.getTime() - a.date.getTime()
        case "date-asc":
          return a.date.getTime() - b.date.getTime()
        case "total-desc":
          return b.total - a.total
        case "total-asc":
          return a.total - b.total
        default:
          return 0
      }
    })
  }

  toggleOrderExpansion(orderId: string): void {
    this.expandedOrderId.set(this.expandedOrderId() === orderId ? null : orderId)
  }

  cancelOrder(orderId: string): void {
    const orders = this.orders()
    const updatedOrders = orders.map((order) =>
      order.id === orderId ? { ...order, status: "cancelled" as const } : order,
    )
    this.orders.set(updatedOrders)
  }

  trackOrder(trackingNumber: string): void {
    window.open(`https://tracking.example.com/${trackingNumber}`, "_blank")
  }

  reorderItems(orderId: string): void {
    const order = this.orders().find((o) => o.id === orderId)
    if (order) {
      console.log("Reordering items from order:", orderId)
    }
  }
}
