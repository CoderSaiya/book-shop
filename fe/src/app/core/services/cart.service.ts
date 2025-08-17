import {inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {BehaviorSubject, Observable, forkJoin, of, throwError} from 'rxjs';
import { map, switchMap, tap, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import type { Book } from '../../models/book.model';
import type { CartRes, CartItemDto, AddCartItemReq, CombinedCartItem } from '../../models/cart.model';
import { GlobalResponse } from '../../models/api-response.model';
import { BookService } from './book.service';
import { Router } from '@angular/router';
import {NotifyService} from './notify.service';
import {getCurrentLang} from '../../shared/utils/lang';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly baseUrl = `${environment.apiUrl}/api/cart`;
  private http = inject(HttpClient)
  private bookService = inject(BookService)
  private router = inject(Router)
  private notify = inject(NotifyService)

  // Giữ CartRes từ server
  private cartSubject = new BehaviorSubject<CartRes | null>(null);
  readonly cart$ = this.cartSubject.asObservable();

  // Suy diễn ra tổng tiền (items) theo server (không gồm shipping/tax)
  readonly subtotal$ = this.cart$.pipe(map(c => c?.totalAmount ?? 0));

  // Suy diễn tổng số lượng
  readonly itemCount$ = this.cart$.pipe(
    map(c => c ? c.items.reduce((sum, i) => sum + i.quantity, 0) : 0)
  );

  /** Lấy giỏ active từ server, cập nhật subject */
  loadActive(): Observable<CartRes> {
    return this.http.get<GlobalResponse<CartRes>>(
      `${this.baseUrl}/active`,
      { withCredentials: true }
    ).pipe(
      map(res => res.data!),
      tap(cart => this.cartSubject.next(cart)),
      catchError(err => {
        if (err.status === 401) this.router.navigate(['/auth/login']);
        throw err;
      })
    );
  }

  /** Thêm/cập nhật 1 item (server sẽ cộng/trừ quantity nếu đã tồn tại) */
  addOrUpdateItem(book: Book, quantity = 1): Observable<CartRes> {
    const req: AddCartItemReq = {
      bookId: book.bookId,
      quantity,
      unitPrice: book.currentPrice,
    };

    const isVi = getCurrentLang() === 'vi'
    const action = isVi ? 'đã thêm' : "added"
    const title = isVi ?
      book.title.vi :
      book.title.en
    const subMessage = `${action} ${quantity} x `
    const message = isVi ?
      subMessage + `"${title}" vào giỏ hàng "` :
      subMessage + `"${title}" to cart "`

    const errMes = isVi ?
      "Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại." :
      "The product could not be added to the cart. Please try again."

    return this.http.post<GlobalResponse<CartRes>>(
      `${this.baseUrl}/items`,
      req,
      { withCredentials: true }
    ).pipe(
      map(res => res.data!),
      tap(cart => {
        this.cartSubject.next(cart)
        this.notify.success(message)
      }),
      catchError((err) => {
        this.notify.error(errMes);
        return throwError(() => err);
      })
    );
  }

  /** Cập nhật quantity cho 1 bookId (cần unitPrice theo API). Lấy unitPrice từ cart hiện tại. */
  updateQuantity(bookId: string, quantity: number): Observable<CartRes> {
    const current = this.cartSubject.value;
    const existing = current?.items.find(i => i.bookId === bookId);

    // Nếu chưa có trong cart (hy hữu), không biết unitPrice => bỏ qua
    if (!existing) return of(current!).pipe(map(c => c!));

    const req: AddCartItemReq = {
      bookId,
      quantity,
      unitPrice: existing.unitPrice
    };

    return this.http.post<GlobalResponse<CartRes>>(
      `${this.baseUrl}/items`,
      req,
      { withCredentials: true }
    ).pipe(
      map(res => res.data!),
      tap(cart => this.cartSubject.next(cart))
    );
  }

  /** Xoá 1 item */
  removeItem(bookId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/items/${bookId}`,
      { withCredentials: true }
    ).pipe(
      tap(() => {
        const current = this.cartSubject.value;
        if (!current) return;
        const next: CartRes = {
          ...current,
          items: current.items.filter(i => i.bookId !== bookId),
          totalAmount: current.items
            .filter(i => i.bookId !== bookId)
            .reduce((s, x) => s + x.totalPrice, 0)
        };
        this.cartSubject.next(next);
      })
    );
  }

  /** Clear toàn bộ (theo API) */
  clearCart(): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/clear`,
      {},
      { withCredentials: true }
    ).pipe(
      tap(() => {
        const current = this.cartSubject.value;
        if (!current) return;
        this.cartSubject.next({ ...current, items: [], totalAmount: 0 });
      })
    );
  }

  /** Deactivate cart hiện tại (tuỳ usecase) */
  deactivateCart(): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/deactivate`,
      {},
      { withCredentials: true }
    ).pipe(
      tap(() => this.cartSubject.next(null))
    );
  }

  /** Trả ra list CombinedCartItem để UI dùng như cũ (có Book đầy đủ) */
  getCombinedItems(): Observable<CombinedCartItem[]> {
    return this.cart$.pipe(
      switchMap(cart => {
        if (!cart || cart.items.length === 0) return of<CombinedCartItem[]>([]);
        // Lấy chi tiết book cho từng item
        const calls = cart.items.map(i => this.bookService.getBookById(i.bookId));
        return forkJoin(calls).pipe(
          map((books: Book[]) => {
            const byId = new Map(books.map(b => [b.bookId, b]));
            return cart.items.map<CombinedCartItem>(i => ({
              book: byId.get(i.bookId)!,
              quantity: i.quantity,
              unitPrice: i.unitPrice,
              totalPrice: i.totalPrice
            }));
          })
        );
      })
    );
  }

  /** Tiện ích: load + trả vm items (dùng trong component init) */
  loadAndGetCombinedItems(): Observable<CombinedCartItem[]> {
    return this.loadActive().pipe(
      switchMap(() => this.getCombinedItems())
    );
  }

  /** Tổng tiền (items) hiện tại — sync từ server */
  getCartTotal(): Observable<number> { return this.subtotal$; }

  /** Số lượng item */
  getCartItemCount(): Observable<number> { return this.itemCount$; }
}

// Re-export kiểu cũ nếu nơi khác import từ service:
export type { CombinedCartItem as CartItem } from '../../models/cart.model';
