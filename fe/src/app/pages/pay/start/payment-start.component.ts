import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {PaymentProviderCode, PaymentService} from '../../../core/services/payment.service';

const providerMap: Record<string, PaymentProviderCode> = {
  momo: 0,
  vnpay: 1
};

@Component({
  standalone: true,
  selector: 'app-payment-start',
  imports: [CommonModule, RouterModule],
  template: `
    <main class="container payment-start">
      <h1>Đang chuyển đến cổng thanh toán…</h1>
      <p *ngIf="providerLabel">
        Phương thức: <strong>{{ providerLabel }}</strong>
      </p>
      <p *ngIf="amount">
        Số tiền:
        <strong>{{ amount | number:'1.0-0' }} ₫</strong>
      </p>
      <p class="muted">
        Nếu không tự chuyển trong vài giây,
        <a (click)="open()">
          bấm vào đây
        </a>.
      </p>
      <p class="muted"><a routerLink="/">Quay lại trang chủ</a></p>
    </main>
  `,
  styles: [`
    .payment-start {
      max-width: 640px;
      padding: 32px 16px;
    }

    .muted {
      color:#666
    }

    a {
      cursor:pointer;
      text-decoration: underline;
    }
  `]
})
export class PaymentStartComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private payments = inject(PaymentService);

  providerLabel = '';
  amount = 0;

  private _payUrl: string | null = null;

  ngOnInit(): void {
    const q = this.route.snapshot.queryParamMap;
    const providerStr = (q.get('provider') || '').toLowerCase(); // 'momo' | 'vnpay'
    const shopOrderId = q.get('shopOrderId') || '';
    const amount = Number(q.get('amount') || 0);

    const provider = providerMap[providerStr];
    if ((provider !== 0 && provider !== 1) || !amount) {
      this.router.navigate(['/']); return;
    }

    this.providerLabel = provider === 0 ? 'MoMo' : 'VNPAY';
    this.amount = amount;

    // Gọi backend /pay
    this.payments.create({
      amount,
      provider,
      orderInfo: `Thanh toán đơn hàng ${shopOrderId}`,
      // clientIp: null // để backend tự lấy từ HttpContext nếu cần
    }).subscribe({
      next: (res) => {
        // Lưu session để callback dùng
        sessionStorage.setItem('pay_provider', String(provider));
        sessionStorage.setItem('pay_paymentOrderId', res.orderId); // id do cổng trả
        sessionStorage.setItem('pay_shopOrderId', shopOrderId); // id đơn hàng

        this._payUrl = res.payUrl;
        // Redirect ra cổng
        window.location.href = res.payUrl;
      },
      error: () => {
        this.router.navigate(['/']);
      }
    });
  }

  open() {
    if (this._payUrl) window.location.href = this._payUrl;
  }
}
