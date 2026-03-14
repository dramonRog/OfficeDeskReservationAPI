import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Component, signal } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrl: './login.css',
  standalone: false
})
export class LoginComponent {
  constructor(private http: HttpClient, private cdr: ChangeDetectorRef, private router: Router) { }

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

        this.router.navigate(['/']);
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.message = 'Invalid email or password!';
        this.cdr.detectChanges();
      }
    });
  }
}
