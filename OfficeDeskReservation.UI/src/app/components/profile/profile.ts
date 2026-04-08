import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.html',
  styleUrls: ['./profile.css'],
  standalone: false
})
export class ProfileComponent implements OnInit {

  public currentUserId: number  = 0;

  public userForm = {
    firstName: '',
    lastName: '',
    email: ''
  };

  public passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  public originalData = { ...this.userForm };

  public isEditMode: boolean = false;
  public isDeleteModalOpen: boolean = false;
  public userInitials: string = 'U';

  public profileError: string = '';
  public passwordError: string = '';
  public notification = { show: false, message: '', isError: false };

  constructor(
    private cdr: ChangeDetectorRef,
    private router: Router,
    private userService: UserService
  ) { }

  ngOnInit(): void {
    this.loadUserData();
  }

  private loadUserData(): void {
    const cachedUserJson = localStorage.getItem('currentUser');
    if (cachedUserJson) {
      try {
        const cachedUser = JSON.parse(cachedUserJson);
        this.currentUserId = cachedUser.id || cachedUser.Id || 0;
        this.userForm.firstName = cachedUser.firstName || cachedUser.FirstName || 'Unknown';
        this.userForm.lastName = cachedUser.lastName || cachedUser.LastName || 'User';
        this.userForm.email = cachedUser.email || cachedUser.Email || '';

        this.originalData = { ...this.userForm };
        this.updateInitials();
      } catch (e) {
        console.error('Błąd odczytu z pamięci podręcznej', e);
      }
    }

    this.userService.getMyProfile().subscribe({
      next: (user) => {
        this.currentUserId = user.id || user.Id;
        this.userForm.firstName = user.firstName || user.FirstName || 'Unknown';
        this.userForm.lastName = user.lastName || user.LastName || 'User';
        this.userForm.email = user.email || user.Email || '';

        this.originalData = { ...this.userForm };
        this.updateInitials();

        localStorage.setItem('currentUser', JSON.stringify(user));
      },
      error: (err) => {
        console.error('Nie udało się zsynchronizować profilu z bazą danych', err);
        if (!this.currentUserId) {
          this.setFallbackUser();
        }
      }
    });
  }

  private setFallbackUser(): void {
    this.currentUserId = 0;
    this.userForm.firstName = 'Unknown';
    this.userForm.lastName = 'User';
    this.userForm.email = '';
    this.originalData = { ...this.userForm };
    this.updateInitials();
  }

  private updateInitials(): void {
    const first = this.userForm.firstName.charAt(0).toUpperCase();
    const last = this.userForm.lastName.charAt(0).toUpperCase();
    this.userInitials = `${first}${last}` || 'U';
  }

  public toggleEditMode(): void {
    this.isEditMode = true;
    this.profileError = '';
  }

  public cancelEdit(): void {
    this.userForm = { ...this.originalData };
    this.isEditMode = false;
    this.profileError = '';
  }

  public saveProfile(): void {
    this.profileError = '';

    if (!this.currentUserId) {
      this.profileError = 'Błąd: Nie znaleziono ID użytkownika.';
      return;
    }

    if (!this.userForm.firstName.trim() || !this.userForm.lastName.trim()) {
      this.profileError = 'First name and Last name cannot be empty.';
      return;
    }

    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!emailRegex.test(this.userForm.email)) {
      this.profileError = 'Please enter a valid email address.';
      return;
    }

    this.userService.updateUser(this.currentUserId, this.userForm).subscribe({
      next: () => {
        this.originalData = { ...this.userForm };
        this.updateInitials();
        this.isEditMode = false;
        this.showNotification('Profile updated successfully!');

        const currentUser = JSON.parse(localStorage.getItem('currentUser') || '{}');
        const updatedUser = {
          id: this.currentUserId,
          firstName: this.userForm.firstName,
          lastName: this.userForm.lastName,
          email: this.userForm.email
        };

        this.userService.updateCurrentUserState(updatedUser);
      },
      error: (err) => {
        console.error('Update failed', err);
        this.profileError = err.error?.message || err.error || 'Failed to update profile. Please try again.';
      }
    });
  }

  public updatePassword(): void {
    this.passwordError = '';

    if (!this.passwordForm.currentPassword || !this.passwordForm.newPassword || !this.passwordForm.confirmPassword) {
      this.passwordError = 'All password fields are required.';
      return;
    }

    const pwd = this.passwordForm.newPassword;

    if (pwd.length < 6) {
      this.passwordError = 'Passwords must be at least 6 characters.';
      return;
    }
    if (!/[a-z]/.test(pwd)) {
      this.passwordError = 'Passwords must have at least one lowercase (\'a\'-\'z\').';
      return;
    }
    if (!/[A-Z]/.test(pwd)) {
      this.passwordError = 'Passwords must have at least one uppercase (\'A\'-\'Z\').';
      return;
    }
    if (!/[0-9]/.test(pwd)) {
      this.passwordError = 'Passwords must have at least one digit (\'0\'-\'9\').';
      return;
    }
    if (!/[^a-zA-Z0-9]/.test(pwd)) {
      this.passwordError = 'Passwords must have at least one non alphanumeric character.';
      return;
    }

    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      this.passwordError = 'The new password and confirmation password do not match.';
      return;
    }

    this.userService.changePassword(this.passwordForm).subscribe({
      next: () => {
        this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
        this.showNotification('Password changed successfully!');
      },
      error: (err) => {
        console.error('Password change failed', err);
        this.passwordError = err.error?.message || err.error || 'Failed to change password. Check your current password.';
      }
    });
  }

  public openDeleteModal(): void {
    this.isDeleteModalOpen = true;
  }

  public closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
  }

  public confirmDeleteAccount(): void {
    
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
