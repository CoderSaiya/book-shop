import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, firstValueFrom } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { HeaderComponent } from '../../shared/components/header/header.component';
import { CartService } from '../../core/services/cart.service';
import type {Book, User} from '../../models/book.model';
import { getCurrentLang, getLangText } from '../../shared/utils/lang';
import { AuthService } from '../../core/services/auth.service';
import { LocationService, Province, District, Ward } from '../../core/services/location.service';
import { parseVietnamAddress, looseEqualsName, joinVietnamAddress } from '../../shared/utils/address';
import {CreateOrderReq} from "../../models/order.model";
import {OrdersService} from "../../core/services/order.service";

interface CartItem {
  book: any;
  quantity: number;
}

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule, HeaderComponent],
  template: `
    <app-header></app-header>

    <main class="checkout-page">
      <div class="container">
        <div class="checkout-header">
          <h1 class="page-title">{{ 'checkout.title' | translate }}</h1>
          <div class="breadcrumb">
            <span routerLink="/cart">{{ 'checkout.cart' | translate }}</span>
            <span class="separator">></span>
            <span class="current">{{ 'checkout.title' | translate }}</span>
          </div>
        </div>

        <div class="checkout-content" *ngIf="cartItems().length > 0; else emptyCheckout">
          <div class="checkout-form">
            <form #f="ngForm" (ngSubmit)="onSubmit(f)">
              <!-- Shipping Information -->
              <div class="form-section">
                <h2 class="section-title">{{ 'checkout.shippingInfo' | translate }}</h2>

                <div class="form-grid">
                  <div class="form-group">
                    <label for="firstName">{{ 'checkout.firstName' | translate }} *</label>
                    <input
                      id="firstName"
                      type="text"
                      name="firstName"
                      class="form-input"
                      required
                      minlength="2"
                      [(ngModel)]="form.firstName"
                      #firstName="ngModel"
                      [class.error]="firstName.invalid && (firstName.dirty || firstName.touched)"
                    />
                    <div class="error-message" *ngIf="firstName.invalid && (firstName.dirty || firstName.touched)">
                      {{ 'checkout.firstNameRequired' | translate }}
                    </div>
                  </div>

                  <div class="form-group">
                    <label for="lastName">{{ 'checkout.lastName' | translate }} *</label>
                    <input
                      id="lastName"
                      type="text"
                      name="lastName"
                      class="form-input"
                      required
                      minlength="2"
                      [(ngModel)]="form.lastName"
                      #lastName="ngModel"
                      [class.error]="lastName.invalid && (lastName.dirty || lastName.touched)"
                    />
                    <div class="error-message" *ngIf="lastName.invalid && (lastName.dirty || lastName.touched)">
                      {{ 'checkout.lastNameRequired' | translate }}
                    </div>
                  </div>
                </div>

                <div class="form-group">
                  <label for="email">{{ 'checkout.email' | translate }} *</label>
                  <input
                    id="email"
                    type="email"
                    name="email"
                    class="form-input"
                    required
                    [(ngModel)]="form.email"
                    #email="ngModel"
                    [class.error]="email.invalid && (email.dirty || email.touched)"
                  />
                  <div class="error-message" *ngIf="email.invalid && (email.dirty || email.touched)">
                    {{ 'checkout.emailRequired' | translate }}
                  </div>
                </div>

                <div class="form-group">
                  <label for="phone">{{ 'checkout.phone' | translate }} *</label>
                  <input
                    id="phone"
                    type="tel"
                    name="phone"
                    class="form-input"
                    required
                    pattern="^\\+?[\\d\\s\\-()]+$"
                    [(ngModel)]="form.phone"
                    #phone="ngModel"
                    [class.error]="phone.invalid && (phone.dirty || phone.touched)"
                  />
                  <div class="error-message" *ngIf="phone.invalid && (phone.dirty || phone.touched)">
                    {{ 'checkout.phoneRequired' | translate }}
                  </div>
                </div>

                <!-- Address (street + province/district/ward) -->
                <div class="form-group">
                  <label class="form-label">{{ 'profile.address.title' | translate }}</label>

                  <div class="grid-3">
                    <select class="form-input"
                            name="province"
                            required
                            [(ngModel)]="selectedProvinceCode"
                            #province="ngModel"
                            (change)="onProvinceChange()"
                            [disabled]="loadingProvinces">
                      <option [ngValue]="null">-- {{ 'profile.address.province' | translate }} --</option>
                      <option *ngFor="let p of provinces" [ngValue]="p.code">{{ p.name }}</option>
                    </select>

                    <select class="form-input"
                            name="district"
                            required
                            [(ngModel)]="selectedDistrictCode"
                            #district="ngModel"
                            (change)="onDistrictChange()"
                            [disabled]="!selectedProvinceCode || loadingDistricts">
                      <option [ngValue]="null">-- {{ 'profile.address.district' | translate }} --</option>
                      <option *ngFor="let d of districts" [ngValue]="d.code">{{ d.name }}</option>
                    </select>

                    <select class="form-input"
                            name="ward"
                            required
                            [(ngModel)]="selectedWardCode"
                            #ward="ngModel"
                            [disabled]="!selectedDistrictCode || loadingWards">
                      <option [ngValue]="null">-- {{ 'profile.address.ward' | translate }} --</option>
                      <option *ngFor="let w of wards" [ngValue]="w.code">{{ w.name }}</option>
                    </select>
                  </div>

                  <input type="text"
                         name="street"
                         required
                         minlength="5"
                         class="form-input mt-2"
                         placeholder="{{ 'profile.address.street' | translate }}"
                         [(ngModel)]="form.street"
                         #street="ngModel"
                         [class.error]="street.invalid && (street.dirty || street.touched)">
                </div>

                <div class="form-grid">
                  <div class="form-group">
                    <label for="postalCode">{{ 'checkout.postalCode' | translate }} *</label>
                    <input
                      id="postalCode"
                      type="text"
                      name="postalCode"
                      class="form-input"
                      required
                      pattern="^[0-9A-Za-z\\s\\-]{3,10}$"
                      [(ngModel)]="form.postalCode"
                      #postalCode="ngModel"
                      [class.error]="postalCode.invalid && (postalCode.dirty || postalCode.touched)"
                    />
                    <div class="error-message" *ngIf="postalCode.invalid && (postalCode.dirty || postalCode.touched)">
                      {{ 'checkout.postalCodeRequired' | translate }}
                    </div>
                  </div>
                </div>
              </div>

              <!-- Payment Method -->
              <div class="form-section">
                <h2 class="section-title">{{ 'checkout.paymentMethod' | translate }}</h2>

                <div class="payment-methods">
                  <label class="payment-option">
                    <input type="radio" name="paymentMethod" [(ngModel)]="form.paymentMethod" value="credit-card"/>
                    <div class="payment-content">
                      <span class="payment-icon">💳</span>
                      <span>{{ 'checkout.creditCard' | translate }}</span>
                    </div>
                  </label>

                  <label class="payment-option">
                    <input type="radio" name="paymentMethod" [(ngModel)]="form.paymentMethod" value="paypal"/>
                    <div class="payment-content">
                      <span class="payment-icon">🅿️</span>
                      <span>PayPal</span>
                    </div>
                  </label>

                  <label class="payment-option">
                    <input type="radio" name="paymentMethod" [(ngModel)]="form.paymentMethod" value="bank-transfer"/>
                    <div class="payment-content">
                      <span class="payment-icon">🏦</span>
                      <span>{{ 'checkout.bankTransfer' | translate }}</span>
                    </div>
                  </label>

                  <label class="payment-option">
                    <input type="radio" name="paymentMethod" [(ngModel)]="form.paymentMethod" value="cod"/>
                    <div class="payment-content">
                      <span class="payment-icon">💵</span>
                      <span>{{ 'checkout.cashOnDelivery' | translate }}</span>
                    </div>
                  </label>
                </div>
                <p class="text-muted" *ngIf="form.paymentMethod !== 'cod'">
                  (Hiện chỉ hỗ trợ COD. Các phương thức khác sẽ được xử lý sau.)
                </p>
              </div>

              <!-- Order Notes -->
              <div class="form-section">
                <h2 class="section-title">{{ 'checkout.orderNotes' | translate }}</h2>
                <div class="form-group">
                  <label for="notes">{{ 'checkout.notesPlaceholder' | translate }}</label>
                  <textarea
                    id="notes"
                    name="notes"
                    rows="4"
                    class="form-textarea"
                    [(ngModel)]="form.notes"
                    [placeholder]="'checkout.notesPlaceholder' | translate"
                  ></textarea>
                </div>
              </div>

              <!-- Nút đặt hàng đặt TRONG form để có thể (ngSubmit) -->
              <button
                type="submit"
                class="btn btn-primary btn-full place-order-btn"
                [disabled]="!isFormValid() || isProcessing() || form.paymentMethod !== 'cod'">
                <span *ngIf="isProcessing()" class="loading-spinner"></span>
                {{ isProcessing() ? ('checkout.processing' | translate) : ('checkout.placeOrder' | translate) }}
              </button>
              <p class="secure-checkout">🔒 {{ 'checkout.secureCheckout' | translate }}</p>
            </form>
          </div>

          <!-- Order Summary -->
          <div class="order-summary">
            <div class="summary-card">
              <h3 class="summary-title">{{ 'checkout.orderSummary' | translate }}</h3>

              <div class="order-items">
                <div *ngFor="let item of cartItems(); trackBy: trackByBookId" class="order-item">
                  <div class="item-image">
                    <img [src]="imageSrc(item.book)" [alt]="getLangText(item.book) ?? 'Unknown title'"/>
                  </div>
                  <div class="item-details">
                    <h4>{{ getLangText(item.book) ?? 'Unknown title' }}</h4>
                    <p>{{ 'checkout.quantity' | translate }}: {{ item.quantity }}</p>
                  </div>
                  <div class="item-price">
                    {{ getCurrentLang() === 'en' ? '$' : '' }}
                    {{ linePrice(item) | number:'1.0-0' }}
                    {{ getCurrentLang() === 'vi' ? ' ₫' : '' }}
                  </div>
                </div>
              </div>

              <div class="summary-calculations">
                <div class="summary-row">
                  <span>{{ 'checkout.subtotal' | translate }}</span>
                  <span>
                    {{ getCurrentLang() === 'en' ? '$' : '' }}
                    {{ subtotal() | number:'1.0-0' }}
                    {{ getCurrentLang() === 'vi' ? ' ₫' : '' }}
                  </span>
                </div>

                <div class="summary-row">
                  <span>{{ 'checkout.shipping' | translate }}</span>
                  <span>
                    {{ getCurrentLang() === 'en' ? '$' : '' }}
                    {{ shipping() | number:'1.0-0' }}
                    {{ getCurrentLang() === 'vi' ? ' ₫' : '' }}
                  </span>
                </div>

                <div class="summary-row">
                  <span>{{ 'checkout.tax' | translate }}</span>
                  <span>
                    {{ getCurrentLang() === 'en' ? '$' : '' }}
                    {{ tax() | number:'1.0-0' }}
                    {{ getCurrentLang() === 'vi' ? ' ₫' : '' }}
                  </span>
                </div>

                <div class="summary-row total">
                  <span>{{ 'checkout.total' | translate }}</span>
                  <span>
                    {{ getCurrentLang() === 'en' ? '$' : '' }}
                    {{ total() | number:'1.0-0' }}
                    {{ getCurrentLang() === 'vi' ? ' ₫' : '' }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <ng-template #emptyCheckout>
          <div class="empty-checkout">
            <div class="empty-icon">🛒</div>
            <h2>{{ 'checkout.emptyCart' | translate }}</h2>
            <p>{{ 'checkout.emptyCartMessage' | translate }}</p>
            <button class="btn btn-primary" routerLink="/books">
              {{ 'checkout.continueShopping' | translate }}
            </button>
          </div>
        </ng-template>
      </div>
    </main>
  `,
  styleUrls: ['./checkout.component.scss'],
})
export class CheckoutComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private router = inject(Router);
  private cartService = inject(CartService);
  private auth = inject(AuthService);
  private orders = inject(OrdersService);
  private loc = inject(LocationService);

  user: User | null = null;

  cartItems = signal<CartItem[]>([]);
  isProcessing = signal(false);

  // address lists
  provinces: Province[] = [];
  districts: District[] = [];
  wards: Ward[] = [];
  selectedProvinceCode: number | null = null;
  selectedDistrictCode: number | null = null;
  selectedWardCode: number | null = null;
  loadingProvinces = false;
  loadingDistricts = false;
  loadingWards = false;

  // form model (template-driven)
  form = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    street: '',
    postalCode: '',
    paymentMethod: 'cod', // mặc định COD
    notes: '' as string | null
  };

  subtotal = computed(() =>
    this.cartItems().reduce((sum, item) => sum + this.linePrice(item), 0)
  );
  // ví dụ: free ship > 500k (VND)
  shipping = computed(() => (this.subtotal() > 500_000 ? 0 : 25_000));
  tax = computed(() => Math.round(this.subtotal() * 0.08));
  total = computed(() => this.subtotal() + this.shipping() + this.tax());

  ngOnInit(): void {
    // Cart
    this.cartService.getCombinedItems()
      .pipe(takeUntil(this.destroy$))
      .subscribe(items => {
        this.cartItems.set(items);
        if (!items.length) this.router.navigate(['/cart']);
      });

    // User & prefill
    this.auth.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(u => {
        this.user = u;
        if (u) this.fillFormFromUser(u);
      });

    if (!this.auth.currentUser && this.auth.accessToken) {
      this.auth.fetchMe().pipe(takeUntil(this.destroy$)).subscribe();
    }

    // Provinces
    this.loadingProvinces = true;
    this.loc.getProvinces()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ps => {
          this.provinces = ps;
          if (this.user) this.fillFormFromUser(this.user);
          if (this.pendingParsedAddr) {
            const { provinceName, districtName, wardName } = this.pendingParsedAddr;
            this.preselectAddressByNames(provinceName, districtName, wardName);
            this.pendingParsedAddr = null;
          }
        },
        complete: () => this.loadingProvinces = false
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next(); this.destroy$.complete();
  }

  private fillFormFromUser(u: User): void {
    const fullName = (u as any).name as string | undefined;
    if (fullName) {
      const parts = fullName.trim().split(/\s+/);
      this.form.lastName = parts.pop() ?? '';
      this.form.firstName = parts.join(' ');
    } else {
      this.form.firstName = (u as any).firstName || '';
      this.form.lastName  = (u as any).lastName || '';
    }

    this.form.email = (u as any).email || '';
    this.form.phone = (u as any).phone || '';
    this.form.street = '';

    const addrStr =
      (u as any).address
      || (u as any).addressString
      || (u as any).address_full
      || '';

    const { street, provinceName, districtName, wardName } = parseVietnamAddress(addrStr);
    if (street) this.form.street = street;

    if (this.provinces.length) {
      this.preselectAddressByNames(provinceName, districtName, wardName);
    } else {
      this.pendingParsedAddr = { provinceName, districtName, wardName, street };
    }
  }

  private preselectAddressByNames(cityName?: string, districtName?: string, wardName?: string) {
    if (!cityName || !this.provinces.length) return;
    const p = this.findByNameLoose(this.provinces, cityName);
    if (!p) return;

    this.selectedProvinceCode = p.code;

    this.loadingDistricts = true;
    this.loc.getDistricts(p.code)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (ds) => {
          this.districts = ds;
          const d = this.findByNameLoose(ds, districtName);
          if (!d) return;

          this.selectedDistrictCode = d.code;

          this.loadingWards = true;
          this.loc.getWards(d.code)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: (ws) => {
                this.wards = ws;
                const w = this.findByNameLoose(ws, wardName);
                if (w) this.selectedWardCode = w.code;
              },
              complete: () => (this.loadingWards = false),
            });
        },
        complete: () => (this.loadingDistricts = false),
      });
  }

  private pendingParsedAddr:
    | { provinceName?: string; districtName?: string; wardName?: string; street?: string }
    | null = null;

  private findByNameLoose<T extends { name: string }>(arr: T[], name?: string): T | undefined {
    if (!name) return undefined;
    return arr.find(x => looseEqualsName(x.name, name));
  }

  onProvinceChange() {
    this.districts = [];
    this.wards = [];
    this.selectedDistrictCode = null;
    this.selectedWardCode = null;

    if (!this.selectedProvinceCode) return;

    this.loadingDistricts = true;
    this.loc.getDistricts(this.selectedProvinceCode)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ds => this.districts = ds,
        complete: () => this.loadingDistricts = false
      });
  }

  onDistrictChange() {
    this.wards = [];
    this.selectedWardCode = null;

    if (!this.selectedDistrictCode) return;

    this.loadingWards = true;
    this.loc.getWards(this.selectedDistrictCode)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ws => this.wards = ws,
        complete: () => this.loadingWards = false
      });
  }

  // UI helpers
  imageSrc(book: Book): string {
    const img = (book as any)?.images?.[0] ?? null;
    if (!img) return '';
    return img.startsWith('data:') ? img : `data:image/jpeg;base64,${img}`;
  }

  linePrice(item: CartItem): number {
    const p = Number(item.book?.currentPrice ?? item.book?.price ?? 0);
    return p * item.quantity;
  }

  trackByBookId(index: number, item: CartItem): string | number {
    return item?.book?.bookId ?? index;
  }

  getCurrentLang = getCurrentLang;
  getLangText = getLangText;

  // form validity
  isFormValid(): boolean {
    const emailOk = !!this.form.email && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.form.email);
    const phoneOk = !!this.form.phone && /^\+?[\d\s\-()]+$/.test(this.form.phone);
    return !!(
      this.form.firstName?.trim() &&
      this.form.lastName?.trim() &&
      emailOk &&
      phoneOk &&
      this.form.street?.trim() &&
      this.form.postalCode?.trim() &&
      this.selectedProvinceCode &&
      this.selectedDistrictCode &&
      this.selectedWardCode
    );
  }

  async onSubmit(f: NgForm): Promise<void> {
    if (!this.isFormValid()) {
      f.form.markAllAsTouched();
      return;
    }
    if (!this.auth.currentUser) {
      this.router.navigate(['/auth/login']);
      return;
    }

    const method = this.form.paymentMethod;
    if (method !== 'cod') {
      alert('Hiện tại chỉ hỗ trợ thanh toán COD.');
      return;
    }

    // Assemble address
    const street = String(this.form.street || '').trim();
    const provinceName = this.provinces.find(p => p.code === this.selectedProvinceCode)?.name || '';
    const districtName = this.districts.find(d => d.code === this.selectedDistrictCode)?.name || '';
    const wardName     = this.wards.find(w => w.code === this.selectedWardCode)?.name || '';

    if (!street || !provinceName || !districtName || !wardName) {
      alert('Vui lòng điền đủ Tỉnh/TP, Quận/Huyện, Phường/Xã và Địa chỉ.');
      return;
    }

    const shippingAddress = joinVietnamAddress(street, wardName, districtName); // "street, ward, district"
    const shippingCity = provinceName; // province

    const currentAddrStr: string =
      (this.auth.currentUser as any)?.address || (this.auth.currentUser as any)?.addressString || '';

    const normalizedNow = (currentAddrStr || '').trim().toLowerCase();
    const normalizedNew = `${shippingAddress}, ${shippingCity}`.trim().toLowerCase();

    this.isProcessing.set(true);
    try {
      // Update profile only if changed
      if (normalizedNew && normalizedNew !== normalizedNow) {
        await firstValueFrom(this.auth.updateProfile(
          (this.auth.currentUser as any).userId || (this.auth.currentUser as any).id,
          {
            firstName: this.form.firstName,
            lastName:  this.form.lastName,
            phone:     this.form.phone,
            street,
            wardName,
            districtName,
            provinceName
          }
        ));
      }

      // Build order items
      const items = this.cartItems().map(i => ({
        bookId: i.book.bookId, // Guid
        quantity: i.quantity,
        unitPrice: Number(i.book?.currentPrice ?? i.book?.price ?? 0)
      }));

      const req: CreateOrderReq = {
        shippingAddress,  // "street, ward, district"
        shippingCity, // province
        shippingPostalCode: this.form.postalCode,
        shippingPhone: this.form.phone,
        notes: this.form.notes ?? null,
        paymentMethod: method,
        items
      };

      // Create order
      const order = await firstValueFrom(this.orders.createOrder(req));

      // // Clear cart FE + success
      // this.cartService.clearCart();
      this.router.navigate(
        ['/checkout/success', order.id],
        { queryParams: { method } }
      );
    } catch (e) {
      console.error('Order failed:', e);

    } finally {
      this.isProcessing.set(false);
    }
  }
}
