import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = environment.apiUrl + '/Users';
  private currentUserSource = new BehaviorSubject<any>(this.getUserFromStorage());

  public currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }


  getUsers(pageNumber: number = 1, pageSize: number = 10, searchTerm: string = ''): Observable<any> {
    let url = `${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`;

    if (searchTerm) {
      url += `&FirstNameTerm=${searchTerm}`;
    }

    return this.http.get(url);
  }

  private getUserFromStorage(): any {
    const data = localStorage.getItem('currentUser');
    return data ? JSON.parse(data) : null;
  }

  public updateCurrentUserState(user: any): void {
    localStorage.setItem('currentUser', JSON.stringify(user));
    this.currentUserSource.next(user);
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

  updateProfile(userData: { firstName: string, lastName: string, email: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/profile`, userData);
  }

  changePassword(passwordData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, passwordData);
  }

  deleteAccount(): Observable<any> {
    return this.http.delete(`${this.apiUrl}/profile`);
  }

  getMyProfile(): Observable<any> {
    return this.http.get(`${this.apiUrl}/profile`);
  }
}
