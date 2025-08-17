import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';

export interface Province {
  code: number;
  name: string;
}

export interface District {
  code: number;
  name: string;
  province_code: number;
}

export interface Ward {
  code: number;
  name: string;
  district_code: number;
}

@Injectable({ providedIn: 'root' })
export class LocationService {
  private base = 'https://provinces.open-api.vn/api/v1';

  constructor(private http: HttpClient) {}

  getProvinces(): Observable<Province[]> {
    return this.http.get<any>(`${this.base}/`).pipe(
      map(res => Array.isArray(res) ? res : (res?.data ?? []))
    );
  }

  getDistricts(provinceCode: number): Observable<District[]> {
    return this.http.get<any>(`${this.base}/p/${provinceCode}?depth=2`).pipe(
      map(res => res?.districts ?? res?.data?.districts ?? [])
    );
  }

  getWards(districtCode: number): Observable<Ward[]> {
    return this.http.get<any>(`${this.base}/d/${districtCode}?depth=2`).pipe(
      map(res => res?.wards ?? res?.data?.wards ?? [])
    );
  }
}
