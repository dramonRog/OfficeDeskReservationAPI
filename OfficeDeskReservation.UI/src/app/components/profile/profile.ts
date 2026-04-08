import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.html',
  styleUrls: ['./profile.css'],
  standalone: false
})
export class ProfileComponent implements OnInit {

  // Model danych podstawowych
  public userForm = {
    firstName: '',
    lastName: '',
    email: ''
  };

  // Model dla zmiany hasła
  public passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  public originalData = { ...this.userForm };

  public isEditMode: boolean = false;
  public isDeleteModalOpen: boolean = false;
  public userInitials: string = 'U';

  public passwordError: string = '';
  public notification = { show: false, message: '', isError: false };

  constructor(
    private cdr: ChangeDetectorRef,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadUserData();
  }

  private loadUserData(): void {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        const payload = JSON.parse(jsonPayload);

        this.userForm.email = payload.email || '';

        let firstName = 'Unknown';
        let lastName = 'User';

        if (payload.unique_name) {
          const parts = payload.unique_name.toString().trim().split(' ');
          if (parts.length > 1) {
            firstName = parts[0];
            lastName = parts.slice(1).join(' ');
          } else {
            firstName = payload.unique_name;
            lastName = '';
          }
        }

        this.userForm.firstName = firstName || 'Unknown';
        this.userForm.lastName = lastName || 'User';

        this.originalData = { ...this.userForm };
        this.updateInitials();

      } catch (e) {
        console.error('Błąd parsowania tokena:', e);
        this.userForm.firstName = 'Unknown';
        this.userForm.lastName = 'User';
      }
    }
  }

  private updateInitials(): void {
    const first = this.userForm.firstName.charAt(0).toUpperCase();
    const last = this.userForm.lastName.charAt(0).toUpperCase();
    this.userInitials = `${first}${last}` || 'U';
  }

  public toggleEditMode(): void {
    this.isEditMode = true;
  }

  public cancelEdit(): void {
    this.userForm = { ...this.originalData };
    this.isEditMode = false;
  }

  public saveProfile(): void {
    // TODO: Tutaj API Call: this.userService.updateProfile(this.userForm)...

    this.originalData = { ...this.userForm };
    this.updateInitials();
    this.isEditMode = false;
    this.showNotification('Profile updated successfully!');
  }

  public updatePassword(): void {
    this.passwordError = '';

    if (!this.passwordForm.currentPassword || !this.passwordForm.newPassword || !this.passwordForm.confirmPassword) {
      this.passwordError = 'All password fields are required.';
      return;
    }

    if (this.passwordForm.newPassword.length < 6) {
      this.passwordError = 'New password must be at least 6 characters long.';
      return;
    }

    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      this.passwordError = 'New passwords do not match.';
      return;
    }

    // TODO: Tutaj API Call: this.userService.changePassword(this.passwordForm)...

    this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
    this.showNotification('Password changed successfully!');
  }

  public openDeleteModal(): void {
    this.isDeleteModalOpen = true;
  }

  public closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
  }

  public confirmDeleteAccount(): void {
    // TODO: Tutaj API Call usuwający konto
    this.closeDeleteModal();
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  public showNotification(message: string, isError: boolean = false): void {
    this.notification = { show: true, message, isError };
    this.cdr.detectChanges();
    setTimeout(() => {
      this.notification.show = false;
      this.cdr.detectChanges();
    }, 4000);
  }
}
