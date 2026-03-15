import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './components/home/home';
import { authGuard } from './guards/auth.guard';
import { LoginComponent } from './components/login/login';
import { RegisterComponent } from './components/register/register';
import { UserManagementComponent } from './components/user-management/user-management';

const routes: Routes = [
  { path: '', component: HomeComponent, canActivate: [authGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'users', component: UserManagementComponent, canActivate: [authGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
