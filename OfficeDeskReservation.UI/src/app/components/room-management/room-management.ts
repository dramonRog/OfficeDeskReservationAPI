import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RoomService, RoomDto, RoomResponseDto } from '../../services/room.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-room-management',
  templateUrl: './room-management.html',
  styleUrl: './room-management.css',
  standalone: false
})
export class RoomManagementComponent implements OnInit {
  public rooms: RoomResponseDto[] = [];

  public currentUserRole: string = '';

  public pageNumber: number = 1;
  public pageSize: number = 8;
  public totalCount: number = 0;
  public totalPages: number = 1;

  public isModalOpen: boolean = false;
  public isEditMode: boolean = false;
  public currentRoomId: number | null = null;

  public isConfirmSaveModalOpen: boolean = false;
  public isDeleteModalOpen: boolean = false;
  public roomToDelete: number | null = null;

  public notification = { show: false, message: '', isError: false };
  public formErrorMessage: string = '';

  public roomForm: RoomDto = { name: '' };

  constructor(private roomService: RoomService, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.extractUserDataFromToken();
    this.loadRooms();
  }

  public get isAdminOrManager(): boolean {
    return this.currentUserRole === 'Admin' || this.currentUserRole === 'Manager';
  }

  public get hasNameError(): boolean {
    return this.formErrorMessage.includes('already exists') ||
      this.formErrorMessage.includes('Room name');
  }

  showNotification(message: string, isError: boolean = false): void {
    this.notification = { show: true, message, isError };
    this.cdr.detectChanges();
    setTimeout(() => {
      this.notification.show = false;
      this.cdr.detectChanges();
    }, 4000);
  }

  loadRooms(): void {
    this.roomService.getRooms(this.pageNumber, this.pageSize).subscribe({
      next: (res: any) => {
        const items = res.items || res.Items || [];
        this.rooms = items.map((r: any) => ({
          id: r.id !== undefined ? r.id : r.Id,
          name: r.name || r.Name,
          desks: (r.desks || r.Desks || []).map((d: any) => ({
            id: d.id !== undefined ? d.id : d.Id,
            deskIdentifier: d.deskIdentifier || d.DeskIdentifier
          }))
        }));

        this.totalCount = res.totalCount || res.TotalCount || 0;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize) || 1;

        this.cdr.detectChanges();
      },
      error: (err: any) => this.showNotification('Failed to load rooms', true)
    });
  }

  changePage(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.pageNumber = newPage;
      this.loadRooms();
    }
  }

  openAddModal(): void {
    this.isEditMode = false;
    this.currentRoomId = null;
    this.roomForm = { name: '' };
    this.isModalOpen = true;
  }

  openEditModal(room: RoomResponseDto): void {
    this.isEditMode = true;
    this.currentRoomId = room.id;
    this.roomForm = { name: room.name };
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.formErrorMessage = '';
  }

  attemptSaveRoom(): void {
    this.formErrorMessage = '';
    const roomName = this.roomForm.name?.trim();

    if (!roomName) {
      this.formErrorMessage = "Room name cannot be empty.";
      return;
    }

    if (roomName.length < 3 || roomName.length > 30) {
      this.formErrorMessage = "Room name must have at least 3 characters and no more than 30!";
      return;
    }

    this.roomService.getRooms(1, 1000, roomName).subscribe({
      next: (res: any) => {
        const items = res.items || res.Items || [];
        const duplicate = items.find((r: any) =>
          (r.name || r.Name).toLowerCase() === roomName.toLowerCase() &&
          (r.id !== this.currentRoomId)
        );

        if (duplicate) {
          this.formErrorMessage = "A room with this name already exists.";
          this.cdr.detectChanges();
        } else {
          this.isConfirmSaveModalOpen = true;
          this.cdr.detectChanges();
        }
      },
      error: () => {
        this.formErrorMessage = "Could not verify room name uniqueness. Try again.";
        this.cdr.detectChanges();
      }
    })
  }

  closeConfirmSaveModal(): void {
    this.isConfirmSaveModalOpen = false;
  }

  private extractErrorMessage(err: any): string {
    if (err.error && err.error.errors) {
      const firstKey = Object.keys(err.error.errors)[0];
      return err.error.errors[firstKey][0];
    }

    if (err.error && err.error.detail) {
      return err.error.detail;
    }

    if (err.error && err.error.message) {
      return err.error.message;
    }

    if (typeof err.error === 'string') {
      return err.error;
    }

    return "An unexpected error occurred.";
  }

  private extractUserDataFromToken(): void {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.currentUserRole = payload['role'];
      } catch (e) {
        console.error('Failed to parse token', e);
      }
    }
  }

  confirmSaveRoom(): void {
    const request$: Observable<any> = (this.isEditMode && this.currentRoomId)
      ? this.roomService.updateRoom(this.currentRoomId, this.roomForm)
      : this.roomService.createRoom(this.roomForm);

    const successMsg = this.isEditMode ? "Room updated successfully!" : "Room created successfully!";

    request$.subscribe({
      next: () => {
        this.loadRooms();
        this.closeConfirmSaveModal();
        this.closeModal();
        this.showNotification(successMsg);
      },
      error: (err: any) => {
        this.closeConfirmSaveModal();
        this.formErrorMessage = this.extractErrorMessage(err);
        this.cdr.detectChanges();
      }
    });
  }

  openDeleteModal(id: number): void {
    this.roomToDelete = id;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.roomToDelete = null;
  }

  confirmDeleteRoom(): void {
    if (this.roomToDelete) {
      this.roomService.deleteRoom(this.roomToDelete).subscribe({
        next: () => {
          this.loadRooms();
          this.closeDeleteModal();
          this.showNotification("Room deleted successfully!");
        },
        error: (err: any) => {
          this.closeDeleteModal();
          const errorMsg = this.extractErrorMessage(err);
          this.showNotification(errorMsg, true);
        }
      });
    }
  }
}
