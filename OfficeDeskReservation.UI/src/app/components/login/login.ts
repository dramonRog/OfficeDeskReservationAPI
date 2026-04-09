import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Component } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';


@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrl: './login.css',
  standalone: false
})
export class LoginComponent {
  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) { }

  public loginObj = {
    email: '',
    password: ''
  };

  public errorMessage: string = '';
  public successMessage: string = '';
  public hasError: boolean = false; 

  public onLogin(): void {
    const url: string = environment.apiUrl  + '/Auth/login';

    this.errorMessage = '';
    this.successMessage = '';
    this.hasError = false;

    this.http.post(url, this.loginObj).subscribe({
      next: (response: any) => {
        this.successMessage = 'Success! Logging you in...';
        localStorage.setItem('token', response.token);

        this.router.navigate(['/']);
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.hasError = true;

        if (err.error && err.error.message) {
          this.errorMessage = err.error.message;
        } else if (err.error && typeof err.error === 'string') {
          this.errorMessage = err.error;
        } else {
          this.errorMessage = 'Invalid email or password!';
        }

        this.cdr.detectChanges();
      }
    });
  }
}
