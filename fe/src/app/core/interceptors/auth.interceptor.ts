import {inject} from '@angular/core';
import {
  HttpErrorResponse, HttpInterceptorFn, HttpRequest
} from '@angular/common/http';
import { Subject, throwError } from 'rxjs';
import { catchError, filter, switchMap, take, tap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';
import {Router} from '@angular/router';

let refreshInProgress = false;
const refreshDone$ = new Subject<string>(); // phát token mới cho các request đang chờ

function withAuth(req: HttpRequest<any>, token: string | null) {
  if (!token) return req;
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const isApi = req.url.startsWith(environment.apiUrl);

  // Gửi/nhận cookie refresh (HttpOnly) => withCredentials
  let handled = isApi ? req.clone({ withCredentials: true }) : req;
  handled = isApi ? withAuth(handled, auth.accessToken) : handled;

  return next(handled).pipe(
    catchError((err: HttpErrorResponse) => {
      // Không phải request tới API hoặc không phải 401 thì trả lỗi luôn
      if (!isApi || err.status !== 401) return throwError(() => err);

      // Nếu đang refresh: đợi token mới rồi retry
      if (refreshInProgress) {
        return refreshDone$.pipe(
          filter(t => !!t),
          take(1),
          switchMap(() => next(withAuth(handled, auth.accessToken)))
        );
      }

      // Bắt đầu refresh
      refreshInProgress = true;
      return auth.refreshToken().pipe(
        tap((newToken) => {
          refreshInProgress = false;
          refreshDone$.next(newToken);
        }),
        switchMap(() => next(withAuth(handled, auth.accessToken))), // retry request cũ
        catchError(refreshErr => {
          // refresh thất bại -> xóa token, điều hướng login
          refreshInProgress = false;
          auth.clearToken();
          router.navigate(['/auth/login']);
          return throwError(() => refreshErr);
        })
      );
    })
  );
};
