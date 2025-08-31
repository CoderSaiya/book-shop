import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, map } from 'rxjs';
import { GlobalResponse } from '../../models/api-response.model';
import {CreateOrderReq, OrderDetailRes} from '../../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private base = `${environment.apiUrl}/api/order`;

  constructor(private http: HttpClient) {}

  createOrder(req: CreateOrderReq): Observable<OrderDetailRes> {
    return this.http.post<GlobalResponse<OrderDetailRes>>(this.base, req, { withCredentials: true })
      .pipe(map(r => r.data!));
  }

  getOrderById(id: string): Observable<GlobalResponse<OrderDetailRes>> {
    return this.http.get<GlobalResponse<OrderDetailRes>>(this.base + `/${id}`);
  }

  getMyOrders(page = 1, pageSize = 20): Observable<OrderDetailRes[]> {
    return this.http
      .get<OrderDetailRes[] | GlobalResponse<OrderDetailRes[]>>(
        `${this.base}/my`,
        {
          withCredentials: true,
          params: { page: String(page), pageSize: String(pageSize) }
        }
      )
      .pipe(map((r: any) => (Array.isArray(r) ? r : (r?.data ?? [])) as OrderDetailRes[]));
  }
}
