import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ReservationService, ReservationDto } from '../../services/reservation.service';
import { RoomService } from '../../services/room.service';

@Component({
  selector: 'app-reservations-management',
  templateUrl: './reservations-management.html',
  styleUrl: './reservations-management.css',
  standalone: false
})
export class ReservationsComponent implements OnInit {
  public myReservations: any[] = [];
  public otherReservations: any[] = [];

  public currentUserId: number = 0;
  public rooms: any[] = [];
  public availableDesks: any[] = [];
  public selectedRoomId: number | null = null;
  public isModalOpen: boolean = false;
  public formErrorMessage: string = '';
  public notification = { show: false, message: '', isError: false };

  public resForm: ReservationDto = { deskId: 0, startTime: '', endTime: '' };
  public pageNumber: number = 1;
  public pageSize: number = 20;

  constructor(
    private reservationService: ReservationService,
    private roomService: RoomService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.extractUserIdFromToken();
    this.loadRooms(); 
    this.loadReservations();
  }

  private extractUserIdFromToken(): void {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.currentUserId = Number(payload['nameid'] || payload['sub']);
      } catch (e) {
        console.error('Failed to parse token', e);
      }
    }
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
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.cdr.detectChanges();
  }

  private extractErrorMessage(err: any): string {
    if (!err || !err.error) return "An error occurred.";
    return err.error.detail || err.error.Detail || "An unexpected error occurred.";
  }

  confirmSaveReservation(): void {
    this.reservationService.createReservation(this.resForm).subscribe({
      next: () => { this.loadReservations(); this.closeModal(); this.showNotification("Success!"); },
      error: (err) => { this.formErrorMessage = this.extractErrorMessage(err); this.cdr.detectChanges(); }
    });
  }

  deleteReservation(id: number): void {
    if (confirm("Cancel this reservation?")) {
      this.reservationService.deleteReservation(id).subscribe({
        next: () => { this.loadReservations(); this.showNotification("Canceled."); },
        error: (err) => this.showNotification(this.extractErrorMessage(err), true)
      });
    }
  }
}
