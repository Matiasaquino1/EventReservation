import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('token');
    if (token) {
      req = req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) });
    }
    return next.handle(req).pipe(
      catchError(err => {
        if (err.status === 401) {
          this.authService.logout();
          alert('Tu sesión expiró. Inicia sesión nuevamente.');
        } else if (err.status === 403) {
          alert('No tienes permisos para esta acción.');
        } else if (err.status === 400) {
          alert(err.error?.message || 'Error en la solicitud.');
        } else if (err.status >= 500) {
          alert('Error del servidor. Intenta nuevamente.');
        }
        return throwError(() => err);
      })
    );
  }
}