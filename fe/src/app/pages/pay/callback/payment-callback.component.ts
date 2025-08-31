import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { PaymentProviderCode, PaymentService } from '../../../core/services/payment.service';

@Component({
  standalone: true,
  selector: 'app-payment-callback',
  imports: [CommonModule, RouterModule],
  template: `
    <main class="container payment-callback">
      <h1>Đang xác nhận thanh toán…</h1>
      <p class="muted">Vui lòng đợi trong giây lát.</p>
    </main>
  `,
  styles: [`.payment-callback{max-width:640px;padding:32px 16px}.muted{color:#666}`]
})
export class PaymentCallbackComponent implements OnInit {
  private router = inject(Router);
  private payments = inject(PaymentService);

  ngOnInit(): void {
    const provider = Number(sessionStorage.getItem('pay_provider')) as PaymentProviderCode;
    const paymentOrderId = sessionStorage.getItem('pay_paymentOrderId') || '';
    const shopOrderId = sessionStorage.getItem('pay_shopOrderId') || '';

    if ((provider !== 0 && provider !== 1) || !paymentOrderId) {
      this.router.navigate(['/']);
      return;
    }

    this.payments.checkStatus(provider, paymentOrderId).subscribe({
      next: (res) => {
        const ok = res?.isPaid === true || (res?.status || '').toLowerCase() === 'success';

        if (!ok) {
          // dọn session + failed
          sessionStorage.removeItem('pay_provider');
          sessionStorage.removeItem('pay_paymentOrderId');
          this.router.navigate(['/checkout/failed'], { queryParams: { reason: res?.message || 'failed' }});
          return;
        }

        // Gọi confirm để BE đánh dấu đơn Paid
        this.payments.confirm({ provider, paymentOrderId, shopOrderId }).subscribe({
          next: () => {
            sessionStorage.removeItem('pay_provider');
            sessionStorage.removeItem('pay_paymentOrderId');
            this.router.navigate(['/checkout/success', shopOrderId || paymentOrderId], {
              queryParams: { method: provider === 0 ? 'momo' : 'vnpay' }
            });
          },
          error: () => {
            this.router.navigate(['/checkout/failed'], { queryParams: { reason: 'confirm-error' }});
          }
        });
      },
      error: () => {
        this.router.navigate(['/checkout/failed'], { queryParams: { reason: 'error' } });
      }
    });
  }
}
