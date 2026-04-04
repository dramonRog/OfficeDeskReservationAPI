import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.html',
  styleUrl: './user-management.css',
  standalone: false
})
export class UserManagementComponent implements OnInit {
  public users: any[] = [];
  public isLoading: boolean = true;
  public currentPage: number = 1;
  public totalPages: number = 1;
  public searchTerm: string = '';
  public currentUserId: number = 0;

  public isEditModalOpen: boolean = false;
  public isSaving: boolean = false;
  public editObj: any = { id: 0, firstName: '', lastName: '', email: '', role: 0 };

  public isDeleteModalOpen: boolean = false;
  public userToDelete: any = null;

  constructor(private userService: UserService, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.loadUsers();

    const token = localStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));

        const rawId = payload['nameid'];

        this.currentUserId = Number(rawId);
      } catch (e) {
        console.error('Failed to parse token payload', e);
      }
    }
  }

  public loadUsers(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.userService.getUsers(this.currentPage, 10, this.searchTerm).subscribe({
      next: (response: any) => {
        this.users = response.items || response;
        if (response.totalCount) this.totalPages = Math.ceil(response.totalCount / 10);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('Failed to load users', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  public onSearch(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  public nextPage(): void {
    if (this.currentPage < this.totalPages)
      { this.currentPage++; this.loadUsers(); }
  }

  public prevPage(): void {
    if (this.currentPage > 1)
      { this.currentPage--; this.loadUsers(); }
  }

  public onEdit(user: any): void {
    this.editObj = { ...user };
    if (this.editObj.role === 'Admin') this.editObj.role = 2;
    else if (this.editObj.role === 'Manager') this.editObj.role = 1;
    else if (this.editObj.role === 'User') this.editObj.role = 0;

    this.isEditModalOpen = true;
    this.cdr.detectChanges();
  }

  public closeEditModal(): void {
    this.isEditModalOpen = false;
    this.cdr.detectChanges();
  }

  public onSaveUser(): void {
    this.isSaving = true;
    this.cdr.detectChanges();

    const profileData = {
      firstName: this.editObj.firstName,
      lastName: this.editObj.lastName,
      email: this.editObj.email
    };

    this.userService.updateUser(this.editObj.id, profileData).subscribe({
      next: () => {
        this.userService.changeUserRole(this.editObj.id, Number(this.editObj.role)).subscribe({
          next: () => {
            this.isSaving = false;
            this.isEditModalOpen = false;
            this.loadUsers(); 
          },
          error: (roleErr) => {
            this.isSaving = false;
            alert("Data updated, but failed to change role.");
            this.cdr.detectChanges();
          }
        });
      },
      error: (profileErr) => {
        this.isSaving = false;
        const msg = profileErr.error?.detail || "Failed to update profile data.";
        alert(`Error: ${msg}`);
        this.cdr.detectChanges();
      }
    });
  }

  public openDeleteModal(user: any): void {
    this.userToDelete = user;
    this.isDeleteModalOpen = true;
    this.cdr.detectChanges();
  }

  public closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.userToDelete = null;
    this.cdr.detectChanges();
  }

  public confirmDelete(): void {
    if (!this.userToDelete) return;

    this.isSaving = true;
    this.cdr.detectChanges();

    this.userService.deleteUser(this.userToDelete.id).subscribe({
      next: () => {
        this.isSaving = false;
        this.isDeleteModalOpen = false;
        this.userToDelete = null;
        this.loadUsers();
      },
      error: (err: any) => {
        this.isSaving = false;
        alert("Failed to delete user. Make sure you have Admin rights!");
        this.cdr.detectChanges();
      }
    });

  }
}
