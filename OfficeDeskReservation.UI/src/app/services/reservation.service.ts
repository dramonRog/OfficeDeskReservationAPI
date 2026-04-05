import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ReservationDto {
  deskId: number;
  startTime: string; 
  endTime: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  private apiUrl = 'https://localhost:7115/api/Reservations';

  constructor(private http: HttpClient) { }

  getReservations(pageNumber: number, pageSize: number): Observable<any> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());

    return this.http.get<any>(this.apiUrl, { params });
  }

  createReservation(reservation: ReservationDto): Observable<any> {
    return this.http.post<any>(this.apiUrl, reservation);
  }

  deleteReservation(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
