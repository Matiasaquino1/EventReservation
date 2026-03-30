import { ApplicationConfig, LOCALE_ID } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import localeEsAr from '@angular/common/locales/es-AR';
import { registerLocaleData } from '@angular/common';


registerLocaleData(localeEsAr);
export const appConfig: ApplicationConfig = {
  providers: [
    provideAnimations(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    { provide: LOCALE_ID, useValue: 'es-AR' }
  ]
};


