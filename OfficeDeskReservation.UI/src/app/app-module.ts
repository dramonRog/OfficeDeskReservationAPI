import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { HomeComponent } from './components/home/home';
import { LoginComponent } from './components/login/login';
import { RegisterComponent } from './components/register/register';
import { AuthInterceptor } from './interceptors/auth.interceptor';
import { UserManagementComponent } from './components/user-management/user-management';
;
@NgModule({
  declarations: [
    App,
    HomeComponent,
    LoginComponent,
    RegisterComponent,
    UserManagementComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ],
  bootstrap: [App]
})
export class AppModule { }
