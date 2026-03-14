import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  styleUrl: './home.css',
  standalone: false
})
export class HomeComponent implements OnInit {
  public userName: string = '';
  public userRole: string = '';
  public userEmail: string = '';
  public userInitials: string = '';
  public isProfileOpen: boolean = false;

  public navLinks = [
    { label: 'Dashboard', active: true },
    { label: 'My Bookings', active: false },
    { label: 'Floor Plans', active: false },
    { label: 'Reports', active: false }
  ];

  constructor(private router: Router) { }

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      this.logout();
      return;
    }
    this.decodeToken(token);
  }

  private decodeToken(token: string): void {
    const payload = JSON.parse(atob(token.split('.')[1]));

    const fullName = payload['unique_name'];
    this.userName = fullName;

    this.userRole = payload['role'];

    this.userEmail = payload['email'];

    const parts = this.userName.trim().split(' ');
    if (parts.length > 1 && parts[0] && parts[parts.length - 1]) {
      this.userInitials = (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    } else {
      this.userInitials = this.userName.substring(0, 2).toUpperCase();
    }
  }

  public toggleProfileMenu() {
    this.isProfileOpen = !this.isProfileOpen;
  }

  public logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}
