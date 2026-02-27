import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const AuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true; // Deja pasar
  }

  // Si no está logueado, lo mandamos al login
  // Opcional: guardamos la URL para redirigirlo después de loguearse
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};