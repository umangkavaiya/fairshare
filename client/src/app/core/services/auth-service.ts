import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private accessToken: string | null = null;
  readonly currentUser = signal<User | null>(null);

  constructor(private http: HttpClient) {}

  register(email: string, password: string, displayName: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(
        `${environment.apiUrl}/auth/register`,
        { email, password, displayName },
        { withCredentials: true },
      )
      .pipe(tap((res) => this.setSession(res)));
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(
        `${environment.apiUrl}/auth/login`,
        { email, password },
        { withCredentials: true },
      )
      .pipe(tap((res) => this.setSession(res)));
  }

  refresh(): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/refresh`, {}, { withCredentials: true })
      .pipe(tap((res) => this.setSession(res)));
  }

  logout(): Observable<void> {
    return this.http
      .post<void>(`${environment.apiUrl}/auth/logout`, {}, { withCredentials: true })
      .pipe(
        tap(() => {
          this.accessToken = null;
          this.currentUser.set(null);
        }),
      );
  }

  // Called once at app startup. Silently attempts to re-establish a session
  // from the httpOnly refresh cookie, if one still exists and is valid.
  // Never throws — a failed attempt just means "not logged in," which is a normal state.
  initializeSession(): Observable<AuthResponse | null> {
    return this.refresh().pipe(
      catchError(() => {
        this.accessToken = null;
        this.currentUser.set(null);
        return of(null);
      }),
    );
  }

  getAccessToken(): string | null {
    return this.accessToken;
  }

  isAuthenticated(): boolean {
    return !!this.accessToken;
  }

  private setSession(res: AuthResponse) {
    this.accessToken = res.accessToken;
    this.currentUser.set(res.user);
  }
}
