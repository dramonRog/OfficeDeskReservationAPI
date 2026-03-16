import { Component, OnInit, ChangeDetectorRef } from '@angular/core'; 
import { DeskService, DeskDto } from '../../services/desk.service';

@Component({
  selector: 'app-desk-management',
  templateUrl: './desk-management.html',
  styleUrl: './desk-management.css',
  standalone: false
})
export class DeskManagementComponent implements OnInit {
  public desks: any[] = [];
  public rooms: any[] = [];

  public pageNumber: number = 1;
  public pageSize: number = 8;
  public totalCount: number = 0;
  public totalPages: number = 1;

  public isModalOpen: boolean = false;
  public isEditMode: boolean = false;
  public currentDeskId: number | null = null;

  public isConfirmSaveModalOpen: boolean = false;
  public isDeleteModalOpen: boolean = false;
  public deskToDelete: number | null = null;

  public notification = { show: false, message: '', isError: false };
  public formErrorMessage: string = '';

  public deskForm: DeskDto = {
    deskIdentifier: '',
    roomId: 0
  };

  constructor(private deskService: DeskService, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.loadRooms();
    this.loadDesks();
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
    this.deskService.getRoomsForDropdown().subscribe({
      next: (res: any) => {
        const items = res.items || res.Items || [];
        this.rooms = items.map((r: any) => ({
          id: r.id !== undefined ? r.id : r.Id,
          name: r.name || r.Name
        }));
        this.cdr.detectChanges(); 
      },
      error: (err: any) => this.showNotification('Error loading rooms', true)
    });
  }

  loadDesks(): void {
    this.deskService.getDesks(this.pageNumber, this.pageSize).subscribe({
      next: (res: any) => {
        const items = res.items || res.Items || [];
        this.desks = items.map((d: any) => ({
          id: d.id !== undefined ? d.id : d.Id,
          deskIdentifier: d.deskIdentifier || d.DeskIdentifier,
          roomName: d.roomName || d.RoomName || 'Unassigned',
          roomId: d.roomId !== undefined ? d.roomId : d.RoomId
        }));

        this.totalCount = res.totalCount || res.TotalCount || 0;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize) || 1;

        this.cdr.detectChanges(); 
      },
      error: (err: any) => this.showNotification('Failed to load desks', true)
    });
  }

  changePage(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.pageNumber = newPage;
      this.loadDesks();
    }
  }

  openAddModal(): void {
    this.isEditMode = false;
    this.currentDeskId = null;
    this.formErrorMessage = '';
    const defaultRoomId = this.rooms.length > 0 ? this.rooms[0].id : 0;
    this.deskForm = { deskIdentifier: '', roomId: defaultRoomId };
    this.isModalOpen = true;
  }

  openEditModal(desk: any): void {
    this.isEditMode = true;
    this.currentDeskId = desk.id;
    this.formErrorMessage = '';
    const matchedRoom = this.rooms.find(r => r.name === desk.roomName);
    this.deskForm = { deskIdentifier: desk.deskIdentifier, roomId: matchedRoom ? matchedRoom.id : 0 };
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.formErrorMessage = '';
  }

  attemptSaveDesk(): void {
    this.formErrorMessage = '';
    const identifier = this.deskForm.deskIdentifier?.trim();

    if (!identifier) {
      this.formErrorMessage = "Desk identifier cannot be empty.";
      return;
    }
    if (this.deskForm.roomId <= 0) {
      this.formErrorMessage = "Please select a valid room.";
      return;
    }

    const isDuplicate = this.desks.some(d =>
      d.deskIdentifier.toLowerCase() === identifier.toLowerCase() &&
      d.id !== this.currentDeskId
    );

    if (isDuplicate) {
      this.formErrorMessage = "A desk with this identifier already exists.";
      return;
    }

    this.isConfirmSaveModalOpen = true;
  }

  closeConfirmSaveModal(): void {
    this.isConfirmSaveModalOpen = false;
  }

  confirmSaveDesk(): void {
    if (this.isEditMode && this.currentDeskId) {
      this.deskService.updateDesk(this.currentDeskId, this.deskForm).subscribe({
        next: () => {
          this.loadDesks();
          this.closeConfirmSaveModal();
          this.closeModal();
          this.showNotification("Desk updated successfully!");
        },
        error: (err: any) => {
          this.closeConfirmSaveModal();
          this.formErrorMessage = err.error?.message || err.error || "Error updating desk.";
          this.cdr.detectChanges(); 
        }
      });
    } else {
      this.deskService.createDesk(this.deskForm).subscribe({
        next: () => {
          this.loadDesks();
          this.closeConfirmSaveModal();
          this.closeModal();
          this.showNotification("Desk created successfully!");
        },
        error: (err: any) => {
          this.closeConfirmSaveModal();
          this.formErrorMessage = err.error?.message || err.error || "Error creating desk.";
          this.cdr.detectChanges(); 
        }
      });
    }
  }

  openDeleteModal(id: number): void {
    this.deskToDelete = id;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.deskToDelete = null;
  }

  confirmDeleteDesk(): void {
    if (this.deskToDelete) {
      this.deskService.deleteDesk(this.deskToDelete).subscribe({
        next: () => {
          this.loadDesks();
          this.closeDeleteModal();
          this.showNotification("Desk deleted successfully!");
        },
        error: (err: any) => {
          this.closeDeleteModal();
          this.showNotification(err.error?.message || err.error || "Error deleting desk", true);
        }
      });
    }
  }
}
