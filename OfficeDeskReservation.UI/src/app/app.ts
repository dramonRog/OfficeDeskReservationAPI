import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Component, signal } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: false
})
export class App {
  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  public password: string = '';
  public email: string = '';
  public message: string = '';

  public login(): void {
    const url: string = 'https://localhost:7115/api/Auth/login';
    const body: { email: string, password: string } = {
      email: this.email,
      password: this.password
    };

    this.http.post(url, body).subscribe({
      next: (response: any) => {
        this.message = 'Success!';
        localStorage.setItem('token', response.token);

        this.cdr.detectChanges();
      },
      error: (err: any) => {
        if (err.status === 400) {
          this.message = 'Error: Incorrect data!';
        } else {
          this.message = 'Error: ' + err.status;
        }

        this.cdr.detectChanges();
      }
    });
  }
}
