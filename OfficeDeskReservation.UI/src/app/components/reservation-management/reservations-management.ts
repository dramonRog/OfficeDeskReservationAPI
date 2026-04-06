import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ReservationService, ReservationDto } from '../../services/reservation.service';
import { RoomService } from '../../services/room.service';
import { UserService } from '../../services/user.service';
import { ActivatedRoute } from '@angular/router';
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
  public isEditMode: boolean = false; 
  public currentReservationId: number | null = null; 

  public isConfirmDeleteModalOpen: boolean = false;
  public reservationToDelete: number | null = null;

  public isSaving: boolean = false;
  public isDeleting: boolean = false;

  public formErrorMessage: string = '';
  public notification = { show: false, message: '', isError: false };

  public resForm: ReservationDto = { deskId: 0, startTime: '', endTime: '' };
  
  public pageNumber: number = 1;
  public pageSize: number = 1000; 

  public myPage: number = 1;
  public otherPage: number = 1;
  public tablePageSize: number = 5; 

  constructor(
    private reservationService: ReservationService,
    private roomService: RoomService,
    private userService: UserService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.extractUserDataFromToken();
    this.loadRooms();
    this.loadUsers();
    this.loadReservations();

    this.route.queryParams.subscribe(params => {
      if (params['action'] === 'new') {
        setTimeout(() => {
          this.openAddModal();
        }, 100);
      }
    });
  }

  get totalMyPages(): number {
    return Math.ceil(this.myReservations.length / this.tablePageSize) || 1;
  }

  get totalOtherPages(): number {
    return Math.ceil(this.otherReservations.length / this.tablePageSize) || 1;
  }

  get displayedMyReservations(): any[] {
    const start = (this.myPage - 1) * this.tablePageSize;
    return this.myReservations.slice(start, start + this.tablePageSize);
  }

  get displayedOtherReservations(): any[] {
    const start = (this.otherPage - 1) * this.tablePageSize;
    return this.otherReservations.slice(start, start + this.tablePageSize);
  }

  nextMyPage(): void { if (this.myPage < this.totalMyPages) this.myPage++; }
  prevMyPage(): void { if (this.myPage > 1) this.myPage--; }

  nextOtherPage(): void { if (this.otherPage < this.totalOtherPages) this.otherPage++; }
  prevOtherPage(): void { if (this.otherPage > 1) this.otherPage--; }

  public get isAdminOrManager(): boolean {
    return this.currentUserRole === 'Admin' || this.currentUserRole === 'Manager';
  }

  public getUserEmail(userId: number): string {
    const user = this.allUsers.find(u => (u.id || u.Id) === userId);
    return user ? (user.email || user.Email || 'No Email') : 'Loading...';
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

        this.myPage = 1;
        this.otherPage = 1;

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
    this.isEditMode = false;
    this.currentReservationId = null;
    this.resForm = { deskId: 0, startTime: '', endTime: '' };
    this.selectedRoomId = null;
    this.availableDesks = [];
    this.formErrorMessage = '';
    this.isSaving = false;
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  openEditModal(reservation: any): void {
    this.isEditMode = true;
    this.currentReservationId = reservation.id;
    this.formErrorMessage = '';
    this.isSaving = false;

    const room = this.rooms.find(r => r.name === reservation.roomName);
    if (room) {
      this.selectedRoomId = room.id;
      this.availableDesks = room.desks || room.Desks || [];
      const desk = this.availableDesks.find(d => (d.deskIdentifier || d.DeskIdentifier) === reservation.deskName);
      
      this.resForm = {
        deskId: desk ? desk.id : 0,
        startTime: this.formatDateForInput(reservation.startTime),
        endTime: this.formatDateForInput(reservation.endTime)
      };
    }
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  private formatDateForInput(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    return date.toISOString().slice(0, 16);
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.cdr.detectChanges();
  }

  private extractErrorMessage(err: any): string {
    if (!err || !err.error) return "An unexpected error occurred.";
    const error = err.error;
    const validationErrors = error.errors || error.Errors;
    if (validationErrors && Object.keys(validationErrors).length > 0) {
      return validationErrors[Object.keys(validationErrors)[0]][0];
    }
    return error.detail || error.Detail || error.message || error.Message || "An unexpected error occurred.";
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

    const request = this.isEditMode && this.currentReservationId
      ? this.reservationService.updateReservation(this.currentReservationId, this.resForm)
      : this.reservationService.createReservation(this.resForm);

    request.subscribe({
      next: () => {
        this.isSaving = false;
        this.loadReservations();
        this.closeModal();
        this.showNotification(this.isEditMode ? "Reservation updated!" : "Reservation created!");
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
          this.showNotification(err.status === 403 ? "Permission denied." : this.extractErrorMessage(err), true);
        }
      });
    }
  }
}
