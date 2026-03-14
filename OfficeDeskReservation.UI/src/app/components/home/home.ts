import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  styleUrl: './home.css',
  standalone: false
})
export class HomeComponent implements OnInit {
  public rooms: any[] = [];
  public errorMessage: string = '';
  public isLoading: boolean = true;

  public selectedDate: string = new Date().toISOString().split('T')[0];

  constructor(private http: HttpClient, private router: Router, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.loadRooms();
  }

  public loadRooms(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const token = localStorage.getItem('token');
    if (!token) {
      this.logout();
      return;
    }

    const headers = new HttpHeaders({ 'Authorization': `Bearer ${token}` });

    this.http.get('https://localhost:7115/api/Rooms', { headers }).subscribe({
      next: (response: any) => {
        this.rooms = response.items || response;
        this.isLoading = false;

        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = 'Failed to load rooms. ' + err.status;
        this.isLoading = false;
        if (err.status === 401) {
          this.logout();
        }
      }
    });
  }

  public viewRoom(roomId: number): void {
    console.log(`Navigating to room ${roomId} on date ${this.selectedDate}`);
  }

  public logout(): void {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}
