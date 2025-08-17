import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info' | 'warn';
export interface Toast { id: number; message: string; type: ToastType; timeout: number; }

@Injectable({ providedIn: 'root' })
export class NotifyService {
  private _toasts = signal<Toast[]>([]);
  toasts = this._toasts.asReadonly();

  show(message: string, type: ToastType = 'info', timeout = 3000) {
    const id = Date.now() + Math.random();
    this._toasts.update(list => [...list, { id, message, type, timeout }]);
    if (timeout > 0) setTimeout(() => this.dismiss(id), timeout);
  }
  success(m: string, t = 2500) { this.show(m, 'success', t); }
  error(m: string, t = 4000) { this.show(m, 'error', t); }
  info(m: string, t = 3000) { this.show(m, 'info', t); }
  warn(m: string, t = 3000) { this.show(m, 'warn', t); }

  dismiss(id: number) {
    this._toasts.update(list => list.filter(x => x.id !== id));
  }
}
