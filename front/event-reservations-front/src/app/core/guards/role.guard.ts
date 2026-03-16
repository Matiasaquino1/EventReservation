import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const RoleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  // Obtenemos los roles permitidos desde la data de la ruta
  const expectedRoles = route.data['roles'] as Array<string>;
  const userRole = authService.getUserRole(); // Debes implementar este método en tu AuthService

  if (!authService.isAuthenticated() || !expectedRoles.includes(userRole)) {
    console.log('Bloqueado por Rol');
    alert('No tienes permisos para acceder a esta sección');
    router.navigate(['/']);
    return false;
  }

  return true;
};