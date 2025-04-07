import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';
import { map, catchError} from 'rxjs/operators'
import { of } from 'rxjs';

export interface User {
  UsersId?: number;
  userName: string;
  Name: string;
  LastName: string;
  Email: string;
  PasswordHash: string;
  AcceptCookies?: boolean;
  Online?: boolean;
  Language?: string;
  ImgProfile?: string;
  ProfileParallaxImg?: string;
  Token?: string;
  Bio?: string;
  Bio_link?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5052/api/Users';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadUserFromStorage(); 
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
        if (response) {
          this.setCurrentUser(response);
          console.log('User logged in:', this.currentUserSubject.value, );
        }
      })
    );
  }

  logout(): Observable<{ message: string }> {
    const payload = { userName: this.currentUserSubject.value?.userName };
    return this.http.post<{ message: string }>(`${this.apiUrl}/logout`, payload).pipe(
      tap(() => this.clearCurrentUser())
    );
  }

  // ---- CLIENT-SIDE AUTH MANAGEMENT ----
  private setCurrentUser(user: User): void {
    this.currentUserSubject.next(user);
    localStorage.setItem('currentUser', JSON.stringify(user));
  }

  private clearCurrentUser(): void {
    this.currentUserSubject.next(null);
    localStorage.removeItem('currentUser');
  }

  private loadUserFromStorage(): void {
    const userData = localStorage.getItem('currentUser');
    if (userData) {
      this.currentUserSubject.next(JSON.parse(userData));
    }
  }

  isLoggedIn(user: User): Promise<string> {
    if (!user || !user.userName) {
      console.log('Utente non valido o username mancante.');
      return Promise.resolve('failed');
    }
  
    const url = `${this.apiUrl}/GetToken?userName=${encodeURIComponent(user.userName)}`;
    console.log('Verifica autenticazione con URL:', url);
  
    return this.http.get(url, { responseType: 'text' }).pipe(
      tap(response => console.log('Token ricevuto:', response)),
      map(response => response ?? 'failed'), // Se `response` è undefined, restituisce 'failed'
      catchError(error => {
        console.error('Errore durante la verifica del login:', error);
        return of('failed');
      })
    ).toPromise() as Promise<string>; // Forziamo il tipo per evitare l'errore TypeScript
  }
  
  

  
  
  

  getToken(): string | null {
    return this.currentUserSubject.value?.Token || null;
  }

  getUser(): User | null {

    return this.currentUserSubject.value || null;
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




}
