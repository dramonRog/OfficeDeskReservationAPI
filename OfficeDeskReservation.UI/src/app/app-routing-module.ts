import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './components/home/home';
import { authGuard } from './guards/auth.guard';
import { LoginComponent } from './components/login/login';
import { RegisterComponent } from './components/register/register';
import { UserManagementComponent } from './components/user-management/user-management';
import { DeskManagementComponent } from './components/desk-management/desk-management';
import { roleGuard } from './guards/role.guard';
import { RoomManagementComponent } from './components/room-management/room-management';
import { ReservationsComponent } from './components/reservation-management/reservations-management';
import { HelpSupportComponent } from './components/help-support/help-support';

const routes: Routes = [
  { path: '', component: HomeComponent, canActivate: [authGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'reservations', component: ReservationsComponent, canActivate: [authGuard] },
  {
    path: 'users',
    component: UserManagementComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin']}
  },

  {
    path: 'desks',
    component: DeskManagementComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin', 'Manager', 'User'] }
  },

  {
    path: 'rooms',
    component: RoomManagementComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin', 'Manager', 'User'] }
  },

  {
    path: 'help',
    component: HelpSupportComponent,
    canActivate: [authGuard]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
