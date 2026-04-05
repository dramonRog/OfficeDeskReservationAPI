import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ReservationService, ReservationDto } from '../../services/reservation.service';
import { RoomService } from '../../services/room.service';
import { UserService } from '../../services/user.service'; 

@Component({
  selector: 'app-reservations-management',
  templateUrl: './reservations-management.html',
  styleUrl: './reservations-management.css',
  standalone: false
})
export class ReservationsComponent implements OnInit {
  public myReservations: any[] = [];
  public otherReservations: any[] = [];
  public allUsers: any[] = []; 

  public currentUserId: number = 0;
  public currentUserRole: string = '';

  public rooms: any[] = [];
  public availableDesks: any[] = [];
  public selectedRoomId: number | null = null;

  public isModalOpen: boolean = false;
  public isConfirmDeleteModalOpen: boolean = false;
  public reservationToDelete: number | null = null;

  public isSaving: boolean = false;
  public isDeleting: boolean = false;

  public formErrorMessage: string = '';
  public notification = { show: false, message: '', isError: false };

  public resForm: ReservationDto = { deskId: 0, startTime: '', endTime: '' };
  public pageNumber: number = 1;
  public pageSize: number = 20;

  constructor(
    private reservationService: ReservationService,
    private roomService: RoomService,
    private userService: UserService, 
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.extractUserDataFromToken();
    this.loadRooms();
    this.loadUsers(); 
    this.loadReservations();
  }

  public get isAdminOrManager(): boolean {
    return this.currentUserRole === 'Admin' || this.currentUserRole === 'Manager';
  }

  public getUserEmail(userId: number): string {
    const user = this.allUsers.find(u => (u.id || u.Id) === userId);
    if (user) {
      return user.email || user.Email || 'No Email';
    }
    return 'Loading...';
  }

  private extractUserDataFromToken(): void {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.currentUserId = Number(payload['nameid'] || payload['sub']);
        this.currentUserRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
          || payload['role']
          || 'User';
      } catch (e) {
        console.error('Failed to parse token', e);
      }
    }
  }

  loadUsers(): void {
    this.userService.getUsers(1, 1000).subscribe({
      next: (res: any) => {
        this.allUsers = res.items || res.Items || [];
        this.cdr.detectChanges();
      }
    });
  }

  loadReservations(): void {
    this.reservationService.getReservations(this.pageNumber, this.pageSize).subscribe({
      next: (res: any) => {
        const rawItems = res.items || res.Items || [];

        const allItems = rawItems.map((r: any) => ({
          id: r.id !== undefined ? r.id : r.Id,
          userId: r.userId !== undefined ? r.userId : r.UserId,
          deskName: r.deskName || r.DeskName || 'Unknown Desk',
          roomName: r.roomName || r.RoomName || 'Unknown Room',
          userName: r.userName || r.UserName || 'Unknown User',
          startTime: r.startTime || r.StartTime,
          endTime: r.endTime || r.EndTime
        }));

        this.myReservations = allItems.filter((r: any) => r.userId === this.currentUserId);
        this.otherReservations = allItems.filter((r: any) => r.userId !== this.currentUserId);

        this.cdr.detectChanges();
      },
      error: () => this.showNotification('Failed to load reservations', true)
    });
  }

  showNotification(message: string, isError: boolean = false): void {
    this.notification = { show: true, message, isError };
    this.cdr.detectChanges();
    setTimeout(() => { this.notification.show = false; this.cdr.detectChanges(); }, 4000);
  }

  loadRooms(): void {
    this.roomService.getRooms(1, 100).subscribe({ next: (res: any) => this.rooms = res.items || res.Items || [] });
  }

  onRoomChange(): void {
    const selectedRoom = this.rooms.find(r => r.id == this.selectedRoomId);
    this.availableDesks = selectedRoom ? (selectedRoom.desks || selectedRoom.Desks || []) : [];
    this.resForm.deskId = 0;
    this.cdr.detectChanges();
  }

  openAddModal(): void {
    this.resForm = { deskId: 0, startTime: '', endTime: '' };
    this.selectedRoomId = null;
    this.availableDesks = [];
    this.formErrorMessage = '';
    this.isSaving = false;
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.isSaving = false;
    this.cdr.detectChanges();
  }

  private extractErrorMessage(err: any): string {
    if (!err || !err.error) {
      return "An unexpected error occurred.";
    }

    const error = err.error;

    const validationErrors = error.errors || error.Errors;
    if (validationErrors && Object.keys(validationErrors).length > 0) {
      const firstKey = Object.keys(validationErrors)[0];
      return validationErrors[firstKey][0];
    }

    const detail = error.detail || error.Detail;
    if (detail) {
      return detail;
    }

    const message = error.message || error.Message;
    if (message) {
      return message;
    }

    if (typeof error === 'string') {
      return error;
    }

    return "An unexpected error occurred.";
  }

  confirmSaveReservation(): void {
    this.formErrorMessage = '';

    if (!this.resForm.deskId || !this.resForm.startTime || !this.resForm.endTime) {
      this.formErrorMessage = "Please fill in all fields.";
      return;
    }

    if (new Date(this.resForm.startTime) >= new Date(this.resForm.endTime)) {
      this.formErrorMessage = "End time must be after the start time.";
      return;
    }

    this.isSaving = true;
    this.cdr.detectChanges();

    this.reservationService.createReservation(this.resForm).subscribe({
      next: () => {
        this.isSaving = false;
        this.loadReservations();
        this.closeModal();
        this.showNotification("Reservation created successfully!");
      },
      error: (err) => {
        this.isSaving = false;
        this.formErrorMessage = this.extractErrorMessage(err);
        this.cdr.detectChanges();
      }
    });
  }

  openDeleteModal(id: number): void {
    this.reservationToDelete = id;
    this.isDeleting = false;
    this.isConfirmDeleteModalOpen = true;
    this.cdr.detectChanges();
  }

  closeDeleteModal(): void {
    this.isConfirmDeleteModalOpen = false;
    this.reservationToDelete = null;
    this.isDeleting = false;
    this.cdr.detectChanges();
  }

  confirmDeleteReservation(): void {
    if (this.reservationToDelete) {
      this.isDeleting = true;
      this.cdr.detectChanges();

      this.reservationService.deleteReservation(this.reservationToDelete).subscribe({
        next: () => {
          this.isDeleting = false;
          this.loadReservations();
          this.closeDeleteModal();
          this.showNotification("Reservation canceled.");
        },
        error: (err) => {
          this.isDeleting = false;
          this.closeDeleteModal();
          if (err.status === 403) {
            this.showNotification("You don't have permission to delete this reservation.", true);
          } else {
            this.showNotification(this.extractErrorMessage(err), true);
          }
        }
      });
    }
  }
}
