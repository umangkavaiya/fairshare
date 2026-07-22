// client/src/app/core/services/health.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class HealthService {
  private readonly apiUrl = 'https://localhost:7xxx/api/health'; // match your actual API port

  constructor(private http: HttpClient) {}

  check(): Observable<{ status: string; time: string }> {
    return this.http.get<{ status: string; time: string }>(this.apiUrl);
  }
}
