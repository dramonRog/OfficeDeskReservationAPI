import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface RoomDto {
  name: string;
}

export interface RoomResponseDto {
  id: number;
  name: string;
  desks?: any[];
}

@Injectable({
  providedIn: 'root'
})
export class RoomService {
  private apiUrl = environment.apiUrl + '/Rooms';

  constructor(private http: HttpClient) { }

  getRooms(pageNumber: number, pageSize: number, searchTerm?: string): Observable<any> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber.toString())
      .set('PageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('SearchTerm', searchTerm);
    }

    return this.http.get<any>(this.apiUrl, { params });
  }

  createRoom(room: RoomDto): Observable<any> {
    return this.http.post<any>(this.apiUrl, room);
  }

  updateRoom(id: number, room: RoomDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, room);
  }

  deleteRoom(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
