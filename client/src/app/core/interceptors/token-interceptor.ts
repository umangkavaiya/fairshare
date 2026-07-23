import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth-service';

export const tokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` }, withCredentials: true })
    : req.clone({ withCredentials: true });

  return next(authReq).pipe(
    catchError((error) => {
      // 401 on an authenticated request likely means the access token expired mid-session
      if (error.status === 401 && token) {
        return authService.refresh().pipe(
          switchMap(() => {
            const retryReq = req.clone({
              setHeaders: { Authorization: `Bearer ${authService.getAccessToken()}` },
              withCredentials: true,
            });
            return next(retryReq);
          }),
          catchError((refreshError) => throwError(() => refreshError)),
        );
      }
      return throwError(() => error);
    }),
  );
};
