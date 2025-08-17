import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { HeaderComponent } from '../../shared/components/header/header.component';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import type { User } from '../../models/book.model';
import { LocationService, Province, District, Ward } from '../../core/services/location.service';
import {looseEqualsName, normalizeName, parseVietnamAddress} from '../../shared/utils/address';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, HeaderComponent],
  template: `
    <app-header></app-header>

    <main class="profile-page">
      <div class="container">
        <div class="profile-header">
          <h1 class="page-title">{{ 'profile.title' | translate }}</h1>
          <p class="page-subtitle">{{ 'profile.subtitle' | translate }}</p>
        </div>

        <div class="profile-content" *ngIf="user; else mustLogin">
          <div class="profile-card">
            <div class="profile-avatar">
              <div class="avatar-circle">
                <img *ngIf="avatarPreview; else userIcon"
                     [src]="avatarPreview"
                     [alt]="user?.firstName || user?.email"
                     class="user-avatar"/>
                <ng-template #userIcon>
                  <span class="user-icon">👤</span>
                </ng-template>
              </div>
              <label class="btn btn-secondary mt-2">
                {{ 'profile.changeAvatar' | translate }}
                <input type="file" accept="image/*" (change)="onAvatarChange($event)" hidden>
              </label>
            </div>

            <form class="profile-form" (ngSubmit)="onSubmit()" #profileForm="ngForm">
              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">{{ 'profile.name.firstName' | translate }}</label>
                  <input type="text" name="firstName" class="form-input" [(ngModel)]="form.firstName" required>
                </div>
                <div class="form-group">
                  <label class="form-label">{{ 'profile.name.lastName' | translate }}</label>
                  <input type="text" name="lastName" class="form-input" [(ngModel)]="form.lastName" required>
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">{{ 'profile.email' | translate }}</label>
                <input type="email" class="form-input" [value]="user?.email" disabled>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">{{ 'profile.phone' | translate }}</label>
                  <input type="tel" name="phone" class="form-input" [(ngModel)]="form.phone">
                </div>
                <div class="form-group">
                  <label class="form-label">{{ 'profile.dob' | translate }}</label>
                  <input type="date" name="dob" class="form-input" [(ngModel)]="form.dob">
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">{{ 'profile.address.title' | translate }}</label>
                <div class="grid-3">
                  <select class="form-input"
                          [(ngModel)]="selectedProvinceCode"
                          name="province"
                          (change)="onProvinceChange()"
                          [disabled]="loadingProvinces">
                    <option [ngValue]="null">-- {{ 'profile.address.province' | translate }} --</option>
                    <option *ngFor="let p of provinces" [ngValue]="p.code">{{ p.name }}</option>
                  </select>

                  <select class="form-input"
                          [(ngModel)]="selectedDistrictCode"
                          name="district"
                          (change)="onDistrictChange()"
                          [disabled]="!selectedProvinceCode || loadingDistricts">
                    <option [ngValue]="null">-- {{ 'profile.address.district' | translate }} --</option>
                    <option *ngFor="let d of districts" [ngValue]="d.code">{{ d.name }}</option>
                  </select>

                  <select class="form-input"
                          [(ngModel)]="selectedWardCode"
                          name="ward"
                          [disabled]="!selectedDistrictCode || loadingWards">
                    <option [ngValue]="null">-- {{ 'profile.address.ward' | translate }} --</option>
                    <option *ngFor="let w of wards" [ngValue]="w.code">{{ w.name }}</option>
                  </select>
                </div>

                <input type="text" class="form-textarea mt-2" placeholder="{{ 'profile.address.street' | translate }}"
                       [(ngModel)]="form.street" name="street">
              </div>

              <div class="form-actions">
                <button type="submit" class="btn btn-primary" [disabled]="isSaving || !profileForm.form.valid">
                  <span *ngIf="!isSaving">{{ 'profile.saveChanges' | translate }}</span>
                  <span *ngIf="isSaving">{{ 'common.saving' | translate }}...</span>
                </button>
              </div>
            </form>
          </div>
        </div>

        <ng-template #mustLogin>
          <div class="empty-cart">
            <h2>{{ 'auth.loginRequired' | translate }}</h2>
            <a class="btn btn-primary" routerLink="/auth/login">{{ 'nav.login' | translate }}</a>
          </div>
        </ng-template>
      </div>
    </main>
  `,
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit, OnDestroy {
  user: User | null = null;

  // form state
  form = {
    firstName: '',
    lastName: '',
    phone: '',
    dob: '', // yyyy-MM-dd
    street: ''
  };

  // avatar
  avatarFile: File | null = null;
  avatarPreview = '';

  // address lists
  provinces: Province[] = [];
  districts: District[] = [];
  wards: Ward[] = [];
  selectedProvinceCode: number | null = null;
  selectedDistrictCode: number | null = null;
  selectedWardCode: number | null = null;
  loadingProvinces = false;
  loadingDistricts = false;
  loadingWards = false;

  isSaving = false;
  private destroy$ = new Subject<void>();

  constructor(
    private auth: AuthService,
    private loc: LocationService
  ) {}

  ngOnInit(): void {
    // Load user (đã có guard, nhưng vẫn an toàn)
    this.auth.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(u => {
        this.user = u;
        if (u) this.fillFormFromUser(u);
      });

    if (!this.auth.currentUser && this.auth.accessToken) {
      this.auth.fetchMe().pipe(takeUntil(this.destroy$)).subscribe();
    }

    // Load provinces
    this.loadingProvinces = true;
    this.loc.getProvinces()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ps => {
          this.provinces = ps;
          if (this.user) this.fillFormFromUser(this.user);
          if (this.pendingParsedAddr) {
            const { provinceName, districtName, wardName } = this.pendingParsedAddr;
            this.preselectAddressByNames(provinceName, districtName, wardName);
            this.pendingParsedAddr = null;
          }
        },
        complete: () => this.loadingProvinces = false
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private findByNameLoose<T extends { name: string }>(arr: T[], name?: string): T | undefined {
    if (!name) return undefined;
    return arr.find(x => looseEqualsName(x.name, name));
  }

  private fillFormFromUser(u: User): void {
    const fullName = (u as any).name as string | undefined;
    if (fullName) {
      const parts = fullName.trim().split(/\s+/);
      this.form.lastName = parts.pop() ?? '';
      this.form.firstName = parts.join(' ');
    } else {
      this.form.firstName = (u as any).firstName || '';
      this.form.lastName  = (u as any).lastName || '';
    }

    this.form.phone = (u as any).phone || '';
    this.form.street = ''; // sẽ điền khi có address chi tiết

    // Avatar
    const av = (u as any).avatar as string | undefined;
    if (av && (av.startsWith('http') || av.startsWith('data:'))) {
      this.avatarPreview = av;
    } else if (av) {
      this.avatarPreview = `data:image/png;base64,${av}`;
    }

    const addrStr =
      (u as any).address
      || (u as any).addressString
      || (u as any).address_full
      || '';

    const { street, provinceName, districtName, wardName } = parseVietnamAddress(addrStr);
    console.log(street, provinceName, districtName, wardName);
    if (street) this.form.street = street;

    // Nếu đã có provinces thì preselect ngay; nếu chưa, lưu pending
    if (this.provinces.length) {
      this.preselectAddressByNames(provinceName, districtName, wardName);
    } else {
      this.pendingParsedAddr = { provinceName, districtName, wardName, street };
    }
  }

  private preselectAddressByNames(cityName?: string, districtName?: string, wardName?: string) {
    if (!cityName || !this.provinces.length) return;

    const p = this.findByNameLoose(this.provinces, cityName);
    if (!p) return;

    this.selectedProvinceCode = p.code;

    this.loadingDistricts = true;
    this.loc.getDistricts(p.code)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (ds) => {
          this.districts = ds;
          const d = this.findByNameLoose(ds, districtName);

          if (!d) return; // không ép lấy ward khi không match district
          this.selectedDistrictCode = d.code;

          this.loadingWards = true;
          this.loc.getWards(d.code)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: (ws) => {
                this.wards = ws;
                const w = this.findByNameLoose(ws, wardName);
                if (w) this.selectedWardCode = w.code;
              },
              complete: () => (this.loadingWards = false),
            });
        },
        complete: () => (this.loadingDistricts = false),
      });
  }

  private pendingParsedAddr:
    | { provinceName?: string; districtName?: string; wardName?: string; street?: string }
    | null = null;

  private findByName<T extends { name: string }>(arr: T[], name?: string): T | undefined {
    if (!name) return undefined;
    const target = normalizeName(name);
    return arr.find(x => normalizeName(x.name) === target);
  }

  onAvatarChange(ev: Event) {
    const input = ev.target as HTMLInputElement;
    const file = input.files?.[0] || null;
    this.avatarFile = file;
    if (file) {
      const reader = new FileReader();
      reader.onload = () => this.avatarPreview = String(reader.result);
      reader.readAsDataURL(file);
    }
  }

  onProvinceChange() {
    this.districts = [];
    this.wards = [];
    this.selectedDistrictCode = null;
    this.selectedWardCode = null;

    if (!this.selectedProvinceCode) return;

    this.loadingDistricts = true;
    this.loc.getDistricts(this.selectedProvinceCode)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ds => this.districts = ds,
        error:  () => {},
        complete: () => this.loadingDistricts = false
      });
  }

  onDistrictChange() {
    this.wards = [];
    this.selectedWardCode = null;

    if (!this.selectedDistrictCode) return;

    this.loadingWards = true;
    this.loc.getWards(this.selectedDistrictCode)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ws => this.wards = ws,
        error:  () => {},
        complete: () => this.loadingWards = false
      });
  }

  onSubmit(): void {
    if (!this.user) return;
    this.isSaving = true;

    const provinceName = this.provinces.find(p => p.code === this.selectedProvinceCode)?.name;
    const districtName = this.districts.find(d => d.code === this.selectedDistrictCode)?.name;
    const wardName = this.wards.find(w => w.code === this.selectedWardCode)?.name;

    const payload = {
      firstName: this.form.firstName,
      lastName: this.form.lastName,
      phone: this.form.phone,
      dateOfBirth: this.form.dob ? new Date(this.form.dob) : null,
      provinceName: provinceName || undefined,
      districtName: districtName || undefined,
      wardName: wardName || undefined,
      street: this.form.street || undefined,
      avatarFile: this.avatarFile || undefined
    };

    this.auth.updateProfile(this.user.userId || (this.user as any).id, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.isSaving = false; },
        error: (e) => { this.isSaving = false; console.error('Update profile error:', e); }
      });
  }
}
