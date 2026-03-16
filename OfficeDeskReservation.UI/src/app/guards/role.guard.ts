import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const roleGuard: CanActivateFn = (route, state) => {
  const router: Router = inject(Router);
  const token: string | null = localStorage.getItem('token');

  if (!token) {
    router.navigate(['/login']);
    return false;
  }

  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const userRole = payload['role'];

    const expectedRoles = route.data['roles'] as Array<string>;

    if (expectedRoles && expectedRoles.includes(userRole)) {
      return true;
    } else {
      alert("Access Denied: You don't have permission to view this page.");
      router.navigate(['/']);
      return false;
    }
  } catch (e) {
    router.navigate(['/login']);
    return false;
  }
};
