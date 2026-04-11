import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Component } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  styleUrl: './register.css',
  standalone: false
})
export class RegisterComponent {
  public regObj = {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: ''
  };

  public errorMessage: string = '';
  public errorField: string = '';
  public message: string = '';
  public isSuccess: boolean = false;

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) { }

  public onRegister(): void {
    this.errorMessage = '';
    this.errorField = '';
    this.message = '';

    if (this.regObj.password !== this.regObj.confirmPassword) {
      this.errorMessage = "Passwords do not match!";
      this.errorField = 'ConfirmPassword';
      return;
    }

    const regUrl = environment.apiUrl + '/Auth/register';
    const loginUrl = environment.apiUrl + '/Auth/login';
    const { confirmPassword, ...dataToRegister } = this.regObj;

    this.http.post(regUrl, dataToRegister).subscribe({
      next: () => {
        this.isSuccess = true;
        this.message = 'Account created! Signing you in...';
        this.cdr.detectChanges();

        const loginObj = { email: this.regObj.email, password: this.regObj.password };

        this.http.post(loginUrl, loginObj).subscribe({
          next: (response: any) => {
            localStorage.setItem('token', response.token);
            this.router.navigate(['/']);
            this.cdr.detectChanges();
          },
          error: (err) => {
            console.error('Auto-login failed:', err);
            this.router.navigate(['/login']);
          }
        });
      },
      error: (err: any) => {
        this.isSuccess = false;
        this.handlePriorityErrors(err);
        this.cdr.detectChanges();
      }
    });
  }

  private handlePriorityErrors(err: any): void {
    if (err.status === 400 && err.error && err.error.errors) {
      const valErrors = err.error.errors;
      const priority = ['FirstName', 'LastName', 'Email', 'Password'];

      for (const field of priority) {
        if (valErrors[field]) {
          this.errorMessage = valErrors[field][0];
          this.errorField = field;
          return;
        }
      }

      const firstKey = Object.keys(valErrors)[0];
      this.errorMessage = valErrors[firstKey][0];
      this.errorField = firstKey;

    } else if (err.error && err.error.detail) {
      this.errorMessage = err.error.detail;

      if (this.errorMessage.includes("name and surname")) {
        this.errorField = 'FullName';
      } else {
        this.errorField = 'Email';
      }
    } else if (err.error && err.error.message) {
      this.errorMessage = err.error.message;
      this.errorField = 'Email';
    } else if (err.error && typeof err.error === 'string') {
      this.errorMessage = err.error;
      this.errorField = 'Email';
    } else {
      this.errorMessage = "Registration failed. Try again.";
      this.errorField = ''; 
    }
  }
}
