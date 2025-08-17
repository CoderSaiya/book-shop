import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotifyService } from '../../../core/services/notify.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  styles: [`
  .toast-wrap{position:fixed;top:16px;right:16px;display:flex;flex-direction:column;gap:8px;z-index:2000}
  .toast{padding:10px 14px;border-radius:8px;box-shadow:0 6px 20px rgba(0,0,0,.15);color:#fff;min-width:240px}
  .success{background:#10b981}.error{background:#ef4444}.info{background:#3b82f6}.warn{background:#f59e0b}
  .toast button{background:transparent;border:0;color:#fff;margin-left:8px;cursor:pointer}
  `],
  template: `
    <div class="toast-wrap">
      <div *ngFor="let t of notify.toasts()"
           class="toast" [ngClass]="t.type">
        <span>{{ t.message }}</span>
        <button (click)="notify.dismiss(t.id)">✕</button>
      </div>
    </div>
  `
})
export class ToastContainerComponent {
  notify = inject(NotifyService);
}
