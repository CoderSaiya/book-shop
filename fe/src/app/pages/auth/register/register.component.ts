import { Component, type OnDestroy } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule, Router } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { TranslateModule } from "@ngx-translate/core";
import { HeaderComponent } from "../../../shared/components/header/header.component";
import { AuthService } from "../../../core/services/auth.service";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import type {RegisterData} from "../../../models/auth.model"

@Component({
  selector: "app-register",
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule, HeaderComponent],
  template: `
    <app-header></app-header>

    <main class="auth-page">
      <div class="container">
        <div class="auth-container">
          <div class="auth-card">
            <div class="auth-header">
              <h1 class="auth-title">{{ 'auth.register' | translate }}</h1>
              <p class="auth-subtitle">Create your BookShop account</p>
            </div>

            <form class="auth-form" (ngSubmit)="onSubmit()" #registerForm="ngForm">
              <div class="form-group">
                <label for="email" class="form-label">{{ 'auth.email' | translate }}</label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  [(ngModel)]="registerData.email"
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
                  [(ngModel)]="registerData.password"
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

              <div class="form-group">
                <label for="confirmPassword" class="form-label">{{ 'auth.confirmPassword' | translate }}</label>
                <input
                  type="password"
                  id="confirmPassword"
                  name="confirmPassword"
                  [(ngModel)]="confirmPassword"
                  required
                  class="form-input"
                  [class.error]="confirmPasswordError"
                  placeholder="Confirm your password"
                />
                <div class="form-error" *ngIf="confirmPasswordError">
                  Passwords do not match
                </div>
              </div>

              <div class="form-error" *ngIf="registerError">
                {{ registerError }}
              </div>

              <button
                type="submit"
                class="btn btn-primary btn-full"
                [disabled]="isLoading || !registerForm.valid || confirmPasswordError"
              >
                <span *ngIf="!isLoading">{{ 'auth.register' | translate }}</span>
                <span *ngIf="isLoading">{{ 'common.loading' | translate }}...</span>
              </button>
            </form>

            <div class="auth-footer">
              <p>
                Already have an account?
                <a routerLink="/auth/login" class="auth-link">{{ 'auth.login' | translate }}</a>
              </p>
            </div>
          </div>
        </div>
      </div>
    </main>
  `,
  styleUrls: ["../auth.component.scss"],
})
export class RegisterComponent implements OnDestroy {
  // ⚠️ Nhớ cập nhật type RegisterData trong service chỉ còn { email: string; password: string }
  registerData: RegisterData = { email: "", password: "" };
  confirmPassword = "";

  isLoading = false;
  registerError = "";
  emailError = false;
  passwordError = false;
  confirmPasswordError = false;

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit(): void {
    this.validateForm();
    if (this.emailError || this.passwordError || this.confirmPasswordError) return;

    this.isLoading = true;
    this.registerError = "";

    this.authService
      .register(this.registerData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (ok) => {
          this.isLoading = false;
          if (ok) {
            this.router.navigate(["/"]);
          } else {
            this.registerError = "Registration failed";
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.registerError = "An error occurred. Please try again.";
          console.error("Register error:", error);
        },
      });
  }

  private validateForm(): void {
    this.emailError = !this.registerData.email || !this.isValidEmail(this.registerData.email);
    this.passwordError = !this.registerData.password || this.registerData.password.length < 6;
    this.confirmPasswordError = this.registerData.password !== this.confirmPassword;
  }

  private isValidEmail(email: string): boolean {
    const emailRegex =/^\S+@\S+\.\S+$/;
    return emailRegex.test(email);
  }
}
