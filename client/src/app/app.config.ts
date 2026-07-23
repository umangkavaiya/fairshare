// client/src/app/app.config.ts
import { ApplicationConfig, inject } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAppInitializer } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { routes } from './app.routes';
import { tokenInterceptor } from './core/interceptors/token-interceptor';
import { AuthService } from './core/services/auth-service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([tokenInterceptor])),
    provideAppInitializer(() => {
      const authService = inject(AuthService);
      return firstValueFrom(authService.initializeSession());
    }),
  ],
};
