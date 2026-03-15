import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router'; 
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false
})
export class App implements OnInit {
  public userName: string = '';
  public userRole: string = '';
  public userEmail: string = '';
  public userInitials: string = '';
  public isProfileOpen: boolean = false;

  public navLinks = [
    { label: 'Dashboard', path: '/', active: true },
    { label: 'My Bookings', path: '/bookings', active: false },
    { label: 'Floor Plans', path: '/floor-plans', active: false },
    { label: 'Reports', path: '/reports', active: false }
  ];

  constructor(private router: Router) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.checkUserStatus();
    });
  }

  ngOnInit(): void {
    this.checkUserStatus();
  }

  public isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  private checkUserStatus(): void {
    const token = localStorage.getItem('token');
    if (token) {
      this.decodeToken(token);
    } else {
      this.resetUserData();
    }
  }

  private decodeToken(token: string): void {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.userName = payload['unique_name'] || '';
      this.userRole = payload['role'] || '';
      this.userEmail = payload['email'] || '';

      const parts = this.userName.trim().split(' ');
      if (parts.length > 1 && parts[0] && parts[parts.length - 1]) {
        this.userInitials = (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
      } else {
        this.userInitials = this.userName.substring(0, 2).toUpperCase();
      }
    } catch (e) {
      this.resetUserData();
    }
  }

  private resetUserData(): void {
    this.userName = '';
    this.userRole = '';
    this.userEmail = '';
    this.userInitials = '';
    this.isProfileOpen = false;
  }

  public toggleProfileMenu() {
    this.isProfileOpen = !this.isProfileOpen;
  }

  public logout() {
    localStorage.removeItem('token');
    this.resetUserData();
    this.router.navigate(['/login']);
  }
}
