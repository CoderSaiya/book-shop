import {Component, inject, type OnDestroy} from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule, Router } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { TranslateModule } from "@ngx-translate/core";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import { HeaderComponent } from "../../../shared/components/header/header.component";
import { AuthService } from "../../../core/services/auth.service";
import type {LoginCredentials} from "../../../models/auth.model"

@Component({
  selector: "app-login",
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule, HeaderComponent],
  template: `
    <app-header></app-header>

    <main class="auth-page">
      <div class="container">
        <div class="auth-container">
          <div class="auth-card">
            <div class="auth-header">
              <h1 class="auth-title">{{ 'auth.login' | translate }}</h1>
              <p class="auth-subtitle">Welcome back to BookShop</p>
            </div>

            <form class="auth-form" (ngSubmit)="onSubmit()" #loginForm="ngForm">
              <div class="form-group">
                <label for="email" class="form-label">{{ 'auth.email' | translate }}</label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  [(ngModel)]="credentials.email"
                  required
                  email
                  class="form-input"
                  [class.error]="emailError"
                  placeholder="Enter your email"
                />
                <div class="form-error" *ngIf="emailError">
                  Please enter a valid email address
                </div>
              </div>

              <div class="form-group">
                <label for="password" class="form-label">{{ 'auth.password' | translate }}</label>
                <input
                  type="password"
                  id="password"
                  name="password"
                  [(ngModel)]="credentials.password"
                  required
                  minlength="6"
                  class="form-input"
                  [class.error]="passwordError"
                  placeholder="Enter your password"
                />
                <div class="form-error" *ngIf="passwordError">
                  Password must be at least 6 characters
                </div>
              </div>

              <div class="form-error" *ngIf="loginError">
                {{ loginError }}
              </div>

              <button
                type="submit"
                class="btn btn-primary btn-full"
                [disabled]="isLoading || !loginForm.valid"
              >
                <span *ngIf="!isLoading">{{ 'auth.login' | translate }}</span>
                <span *ngIf="isLoading">{{ 'common.loading' | translate }}...</span>
              </button>

              <div class="auth-divider">
                <span>or</span>
              </div>

              <div class="oauth-buttons">
                <button type="button" class="btn btn-oauth google" (click)="loginWithGoogle()" [disabled]="isLoading">
                  <img src="/assets/icons/google.svg" alt="Google" /> Continue with Google
                </button>
                <button type="button" class="btn btn-oauth github" (click)="loginWithGitHub()" [disabled]="isLoading">
                  <img src="/assets/icons/github.svg" alt="GitHub" /> Continue with GitHub
                </button>
              </div>
            </form>

            <div class="auth-footer">
              <p>
                Don't have an account?
                <a routerLink="/auth/register" class="auth-link">{{ 'auth.register' | translate }}</a>
              </p>
              <a href="#" class="auth-link">{{ 'auth.forgotPassword' | translate }}</a>
            </div>

            <div class="demo-info">
              <p><strong>Demo Account:</strong></p>
              <p>Email: client1&#64;gmail.com</p>
              <p>Password: 1234567890</p>
            </div>
          </div>
        </div>
      </div>
    </main>
  `,
  styleUrls: ["../auth.component.scss"],
})
export class LoginComponent implements OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);

  credentials: LoginCredentials = { email: "", password: "" };
  isLoading = false;
  loginError = "";
  emailError = false;
  passwordError = false;
  private destroy$ = new Subject<void>();

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit(): void {
    this.validateForm();
    if (this.emailError || this.passwordError) return;

    this.isLoading = true;
    this.loginError = "";

    this.authService
      .login(this.credentials)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (ok) => {
          this.isLoading = false;
          if (ok) {
            this.router.navigate(["/"]);
          } else {
            this.loginError = "Invalid email or password";
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.loginError = "An error occurred. Please try again.";
          console.error("Login error:", error);
        },
      });
  }

  loginWithGoogle() {
    this.authService.loginWithProvider('google');
  }
  loginWithGitHub() {
    this.authService.loginWithProvider('github');
  }

  private validateForm(): void {
    this.emailError = !this.credentials.email || !this.isValidEmail(this.credentials.email);
    this.passwordError = !this.credentials.password || this.credentials.password.length < 6;
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }
}
