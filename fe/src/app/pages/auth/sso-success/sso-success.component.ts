import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="center">
      <div class="spinner"></div>
      <p>Signing you in...</p>
    </div>
  `,
  styles: [`
    .center{min-height:60vh;display:flex;flex-direction:column;align-items:center;justify-content:center;gap:12px}
    .spinner{width:28px;height:28px;border:3px solid var(--border);border-top-color:var(--primary);border-radius:50%;animation:spin 1s linear infinite}
    @keyframes spin{to{transform:rotate(360deg)}}
  `]
})
export class SsoSuccessComponent implements OnInit {
  private auth = inject(AuthService);
  private router = inject(Router);

  async ngOnInit(): Promise<void> {
    const ok = this.auth.storeAccessTokenFromFragment(window.location.hash);

    history.replaceState(null, '', location.pathname + location.search);

    if (!ok) {
      this.router.navigate(['/auth/login'], { queryParams: { error: 'sso_missing_token' } });
      return;
    }

    this.router.navigateByUrl('/');
  }
}
