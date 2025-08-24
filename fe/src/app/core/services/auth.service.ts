import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {BehaviorSubject, Observable, of} from 'rxjs';
import {map, switchMap, tap} from 'rxjs/operators';
import {environment} from '../../../environments/environment';
import {GlobalResponse} from '../../models/api-response.model';
import {LoginCredentials, RegisterData} from '../../models/auth.model';
import {User} from '../../models/book.model';
import {formatDate} from '@angular/common';

interface AuthDto {
  accessToken: string;
  refreshToken?: string;
  accessTokenExpiresAtUtc: string; // trùng với JSON bạn gửi
  user?: User;
}

export interface UpdateProfileForm {
  firstName?: string;
  lastName?: string;
  phone?: string;
  dateOfBirth?: Date | null;
  provinceName?: string;
  districtName?: string;
  wardName?: string;
  street?: string;
  avatarFile?: File | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private accessTokenSubject = new BehaviorSubject<string | null>(null);
  private currentUserSubject = new BehaviorSubject<User | null>(null);

  private readonly authUrl = `${environment.apiUrl}/api/auth`;
  private readonly userUrl = `${environment.apiUrl}/api/user`;

  readonly accessToken$ = this.accessTokenSubject.asObservable();
  readonly currentUser$ = this.currentUserSubject.asObservable();

  get accessToken(): string | null { return this.accessTokenSubject.value; }
  get currentUser(): User | null { return this.currentUserSubject.value; }

  constructor(private http: HttpClient) {
    const savedToken = localStorage.getItem('access-token');
    const savedUser = localStorage.getItem('current-user');
    if (savedToken) this.accessTokenSubject.next(savedToken);
    if (savedUser) this.currentUserSubject.next(JSON.parse(savedUser) as User);
  }

  private setAccessToken(token: string | null) {
    this.accessTokenSubject.next(token);
    if (token) localStorage.setItem('access-token', token);
    else localStorage.removeItem('access-token');
  }

  private setCurrentUser(user: User | null) {
    this.currentUserSubject.next(user);
    if (user) localStorage.setItem('current-user', JSON.stringify(user));
    else localStorage.removeItem('current-user');
  }

  register(data: RegisterData): Observable<boolean> {
    return this.http.post<GlobalResponse<string>>(
      `${this.authUrl}/register`,
      data,
      { withCredentials: true }
    )
      // Nếu backend trả "success" thì dùng res.success; còn "isSuccess" thì giữ như hiện tại.
      .pipe(map(res => (res as any).success ?? (res as any).isSuccess));
  }

  login(credentials: LoginCredentials): Observable<boolean> {
    const body = new HttpParams()
      .set('email', credentials.email)
      .set('password', credentials.password);

    return this.http.post<GlobalResponse<AuthDto>>(
      `${this.authUrl}/login`,
      body.toString(), // serialize form body
      {
        withCredentials: true,
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
      }
    ).pipe(
      tap(res => {
        const dto = res.data;
        if (dto?.accessToken) this.setAccessToken(dto.accessToken);
        if (dto?.user) this.setCurrentUser(dto.user);
      }),
      switchMap(res => {
        const hasUser = !!res.data?.user;
        const ok = (res as any).success ?? (res as any).isSuccess;
        if (!ok) return of(false);
        if (hasUser) return of(true);
        return this.fetchMe().pipe(map(() => true));
      })
    );
  }

  refreshToken(): Observable<string> {
    return this.http.post<GlobalResponse<AuthDto>>(
      `${this.authUrl}/refresh-token`,
      {}, // server đọc refresh token từ cookie HttpOnly "rt"
      { withCredentials: true }
    ).pipe(
      tap(res => {
        if (!res.data?.accessToken) throw new Error('No access token in refresh response');
        this.setAccessToken(res.data.accessToken);
        if (res.data.user) this.setCurrentUser(res.data.user);
      }),
      map(res => res.data!.accessToken)
    );
  }

  fetchMe(): Observable<User> {
    return this.http.get<GlobalResponse<User>>(
      `${this.authUrl}/me`,
      { withCredentials: true }
    ).pipe(
      map(res => res.data as User),
      tap(user => this.setCurrentUser(user))
    );
  }

  logout(): Observable<void> {
    return this.http.post<GlobalResponse<string>>(
      `${this.authUrl}/logout`,
      {},
      { withCredentials: true }
    ).pipe(
      tap(() => {
        this.setAccessToken(null);
        this.setCurrentUser(null);
      }),
      map(() => void 0)
    );
  }

  loginWithProvider(provider: 'google' | 'github', returnPath = '/auth/sso/success') {
    const returnUrl = `${window.location.origin}${returnPath}`;
    window.location.href = `${this.authUrl}/external/${provider}/start?returnUrl=${encodeURIComponent(returnUrl)}`;
  }

  storeAccessTokenFromFragment(hash: string) {
    const params = new URLSearchParams(hash.replace(/^#/, ''));
    const access = params.get('access_token');
    if (access) this.setAccessToken(access);
  }

  clearToken() { this.setAccessToken(null); }

  public clearSession() {
    this.setAccessToken(null);
    this.setCurrentUser(null);
  }

  updateProfile(userId: string, form: UpdateProfileForm) {
    const fd = new FormData();
    if (form.firstName) fd.append('FirstName', form.firstName);
    if (form.lastName) fd.append('LastName', form.lastName);
    if (form.phone) fd.append('PhoneNumber', form.phone);
    if (form.dateOfBirth) fd.append('DateOfBirth', formatDate(form.dateOfBirth, 'yyyy-MM-dd', 'en-US')!);
    if (form.street) fd.append('Address.Street', form.street);
    if (form.wardName) fd.append('Address.Ward', form.wardName);
    if (form.districtName) fd.append('Address.District', form.districtName);
    if (form.provinceName) fd.append('Address.CityOrProvince', form.provinceName);
    if (form.avatarFile) fd.append('Avatar', form.avatarFile, form.avatarFile.name);

    return this.http.put(`${this.userUrl}/profile/${userId}`, fd, {withCredentials: true})
      .pipe(
        switchMap(() => this.fetchMe()),
        map(() => void 0)
      );
  }
}
