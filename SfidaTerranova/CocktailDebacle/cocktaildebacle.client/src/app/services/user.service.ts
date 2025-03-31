import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';

export interface User {
  UsersId?: number; // Aggiunto ID utente
  UserName: string;
  Name: string;
  LastName: string;
  Email: string;
  PasswordHash: string;
  AcceptCookies?: boolean;
  Online?: boolean;
  Language?: string;
  ImgProfile?: string;
  Token?: string; // Aggiunto per JWT
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5052/api/Users';
  private currentUser: User | null = null;

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadUserFromStorage(); // Carica l'utente al startup
  }
  
  forceLogout() {
    this.clearCurrentUser();
    this.router.navigate(['/login-signup']);
  }

  // ---- API CALLS ----
  registerUser(user: User): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user).pipe(
      tap((response: any) => {
        if (response.success) {
          this.setCurrentUser(response.user);
        }
      })
    );
  }

  login(userName: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, { 
      UserNameRequest: userName, 
      PasswordRequest: password 
    }).pipe(
      tap((response: any) => {
        if (response.success) {
          this.setCurrentUser(response.user);
          this.http.post(`${this.apiUrl}/login`,
            {

            });
        }
      })
    );
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout`, {}).pipe(
      tap(() => {
        this.clearCurrentUser();
        this.router.navigate(['/login-signup']);
      })
    );
  }

  // ---- CLIENT-SIDE AUTH MANAGEMENT ----
  private setCurrentUser(user: User): void {
    this.currentUser = user;
    localStorage.setItem('currentUser', JSON.stringify(user));
  }

  private clearCurrentUser(): void {
    this.currentUser = null;
    localStorage.removeItem('currentUser');
  }

  private loadUserFromStorage(): void {
    const userData = localStorage.getItem('currentUser');
    if (userData) {
      this.currentUser = JSON.parse(userData);
    }
  }


  isLoggedIn(): boolean {
    return !!this.currentUser;
  }


  getToken(): string | null {
    return this.currentUser?.Token || null;
  }

  // ---- REST OF API METHODS ----
  updateUser(usersId: number, user: Partial<User>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${usersId}`, user);
  }

  deleteUser(usersId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${usersId}`).pipe(
      tap(() => this.clearCurrentUser())
    );
  }

  getUser(): Observable<any> {
    var getUrl = "http://localhost:5052/api/getUser"
    return this.http.get(`${getUrl}`);
  }

  getUsers(): Observable<any> {
    return this.http.get(`${this.apiUrl}`);
  }

  recoverPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/recoverPassword`, { 
      EmailRequest: email
    });
  }

  resetPassword(usersId: number, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resetPassword/${usersId}`, { 
      PasswordRequest: password
    });
  }

  changePassword(usersId: number, oldPassword: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/changePassword/${usersId}`, { 
      OldPasswordRequest: oldPassword,
      NewPasswordRequest: newPassword
    });
  }

  changeEmail(usersId: number, email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/changeEmail/${usersId}`, { 
      EmailRequest: email
    });
  }
}