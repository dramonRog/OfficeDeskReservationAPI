import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DeskDto {
  deskIdentifier: string;
  roomId: number;
}

@Injectable({
  providedIn: 'root'
})
export class DeskService {
  private apiUrl = 'https://localhost:7115/api/Desks';
  private roomsUrl = 'https://localhost:7115/api/Rooms';

  constructor(private http: HttpClient) { }

  getDesks(pageNumber: number, pageSize: number): Observable<any> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());

    return this.http.get<any>(this.apiUrl, { params });
  }

  createDesk(desk: DeskDto): Observable<any> {
    return this.http.post<any>(this.apiUrl, desk);
  }

  updateDesk(id: number, desk: DeskDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, desk);
  }

  deleteDesk(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getRoomsForDropdown(): Observable<any> {
    let params = new HttpParams().set('PageNumber', '1').set('PageSize', '50');
    return this.http.get<any>(this.roomsUrl, { params });
  }
}
