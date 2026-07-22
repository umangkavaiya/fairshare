import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class HealthService {
  private readonly apiUrl = 'http://localhost:5159/api/health'; // your actual port from Check 1

  constructor(private http: HttpClient) {}

  check(): Observable<{ status: string; time: string }> {
    return this.http.get<{ status: string; time: string }>(this.apiUrl);
  }
}
