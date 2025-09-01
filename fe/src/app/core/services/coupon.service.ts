import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GlobalResponse } from '../../models/api-response.model';

export interface EligibleCouponRes {
  id: string;
  code: string;
  type: 'Percentage' | 'Amount' | string;
  value: number;
  maxDiscountAmount?: number | null;
  minSubtotal?: number | null;
  startsAt?: string | null;
  expiresAt?: string | null;
  isUsed: boolean;
  isActive: boolean;
  createdAt: string;
  discount: number;
  message?: string;
}

@Injectable({ providedIn: 'root' })
export class CouponService {
  private base = `${environment.apiUrl}/api/coupon`;
  constructor(private http: HttpClient) {}

  eligible(subtotal: number): Observable<EligibleCouponRes[]> {
    const params = new HttpParams().set('subtotal', String(subtotal));
    return this.http
      .get<GlobalResponse<EligibleCouponRes[]>>(`${this.base}/eligible`, { params, withCredentials: true })
      .pipe(map(r => r.data ?? []));
  }
}
