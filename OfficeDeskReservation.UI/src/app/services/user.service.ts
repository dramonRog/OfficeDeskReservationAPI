import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'https://localhost:7115/api/Users';

  constructor(private http: HttpClient) { }

  getUsers(pageNumber: number = 1, pageSize: number = 10, searchTerm: string = ''): Observable<any> {
    let url = `${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`;

    if (searchTerm) {
      url += `&FirstNameTerm=${searchTerm}`;
    }

    return this.http.get(url);
  }

  deleteUser(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  updateUser(id: number, userData: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, userData);
  }

  changeUserRole(id: number, role: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/role`, { role: role });
  }
}
