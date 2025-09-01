import {inject, Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {environment} from '../../../environments/environment';

export type PaymentProviderCode = 0 | 1; // 0=MoMo, 1=VnPay

export interface CreatePaymentReq {
  amount: number;
  provider: PaymentProviderCode;
  orderInfo?: string | null;
  clientIp?: string | null;
}

export interface CreatePaymentRes {
  payUrl: string;
  orderId: string;
}

export interface PaymentStatusRes {
  status?: string;
  isPaid?: boolean;
  message?: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/payment`;

  create(req: CreatePaymentReq): Observable<CreatePaymentRes> {
    return this.http.post<CreatePaymentRes>(this.base + '/pay', {
      Amount: req.amount,
      Provider: req.provider,
      OrderInfo: req.orderInfo ?? null,
      ClientIp: req.clientIp ?? null
    });
  }

  checkStatus(provider: PaymentProviderCode, orderId: string): Observable<PaymentStatusRes> {
    const params = new HttpParams()
      .set('provider', String(provider))
      .set('orderId', orderId);
    return this.http.get<PaymentStatusRes>(this.base + '/status', { params });
  }

  confirm(body: { provider: number; paymentOrderId: string; shopOrderId: string }) {
    return this.http.post<{ isPaid: boolean; orderId: string }>(
      `${environment.apiUrl}/api/payment/confirm`, body, { withCredentials: true }
    );
  }
}
