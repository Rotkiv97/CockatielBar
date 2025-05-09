import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';
import { map, catchError} from 'rxjs/operators'
import { of } from 'rxjs';
import { firstValueFrom } from 'rxjs';

export interface User {
  id?: number;
  userName: string;
  name: string;
  lastName: string;
  email: string;
  PasswordHash: string;
  acceptCookies?: boolean;
  Online?: boolean;
  Language?: string;
  imgProfileUrl?: string;
  ProfileParallaxImg?: string;
  token?: string;
  Bio?: string;
  Bio_link?: string;
}

interface FollowedUser {
  id: string;
  userName: string;
  followed_Users:[];
  followers_Users:[];

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


  getUserByUsername(username: string) {

    const url = this.apiUrl + '/GetUser/' + username;
    
    const token = this.getToken();
    
    const headers = new HttpHeaders({
      'Authorization': 'Bearer ' + token,
      'Content-Type': 'application/json'
    });
  
    
    return this.http.get<any>(url, {headers: headers}).pipe(
      map(response => response), 
      catchError(error => {
        console.error('Errore durante la verifica del login:', error);
        console.error('Stato risposta:', error.status);
        console.error('Messaggio errore:', error.error || error.message);
        return of('404');
      })
    );
  }
  

  forceLogout() {
    this.clearCurrentUser();
    this.router.navigate(['/login-signup']);
  }

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

   setCurrentUser(user: User): void {
    this.currentUserSubject.next(user);
    localStorage.setItem('currentUser', JSON.stringify(user));
  }

  clearCurrentUser(): void {
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
      map(response => response ?? 'failed'), 
      catchError(error => {
        console.error('Errore durante la verifica del login:', error);
        return of('failed');
      })
    ).toPromise() as Promise<string>;
  }


  followUp(id: number) {
    const url = this.apiUrl + "/FollowedNewUser/" + id;
    const headers = new HttpHeaders({
      'Authorization': 'Bearer ' + this.getToken()
    });
    
    return this.http.post(url, {}, { 
      headers: headers,
      responseType: 'json'
    }).pipe(
      map(response => {
        console.log('Follow response:', response);
        return response;
      }),
      catchError(error => {
        console.error('Errore durante il follow/unfollow:', error);
        return of({ Message: 'failed' });
      })
    ).toPromise();
  }

  getFollowers(id: number): Promise<FollowedUser[]> {
    const url = this.apiUrl + "/GetFollowersUsers/" + id;
    const headers = new HttpHeaders({
      'Authorization': 'Bearer ' + this.getToken()
    });
  
    return firstValueFrom(this.http.get<any>(url, { 
      headers: headers 
    }).pipe(
      map(response => {
        if (typeof response === 'string') {
          return [];
        }
        
        if (response && response.followed_Users) {
          return response.followed_Users;
        }
        
        if (Array.isArray(response)) {
          return response;
        }
        return [];
      }),
      catchError(error => {
        console.error('Errore:', error);
        return of([]);
      })
    ));
  }

  getUserPassword(userid:number)
  { 
    const url = this.apiUrl + "/getPassword/" + userid
    const header = new HttpHeaders({
      'Authorization': 'Bearer ' + this.getToken()
    });
    return firstValueFrom(this.http.get<any>(url, { 
      headers: header
    }).pipe(
      map(response => {
          console.log(response, " ? VICTROR ? ES")
          return response;
      }),
      catchError(error => {
        console.error('Errore:', error.error);
        return of([]);
      })
    ));
}

  
  getFollowing(id:number): Promise<FollowedUser[]> {
    const url = this.apiUrl + "/GetFollowedUsers/" + id;
    const headers = new HttpHeaders({
      'Authorization': 'Bearer ' + this.getToken()
    });
  
    return firstValueFrom(this.http.get<any>(url, { 
      headers: headers 
    }).pipe(
      map(response => {
        if (typeof response === 'string') {
          return [];
        }
  
        if (response && response.followed_Users) {
          return response.followed_Users;
        }
  
        if (Array.isArray(response)) {
          return response;
        }
  
        return [];
      }),
      catchError(error => {
        console.error('Errore:', error);
        return of([]);
      })
    ));
  }
  
  

  getToken(): string | null {
    return this.currentUserSubject.value?.token || null;
  }

  getUser(): User | null {

    return this.currentUserSubject.value || null;
  }

  async updateUser(userId: number, user: Partial<User>) {
    const url = `${this.apiUrl}/${userId}`;
    
    const headers = new HttpHeaders({
      'Authorization': 'Bearer ' + this.getToken(),
      'Content-Type': 'application/json'
    });
  
    try {
      const response = await this.http.put(url, user, {
        headers: headers,
        responseType: 'json'
      }).toPromise();
  
      console.log('User updated:', response);
      this.clearCurrentUser();
      const token = await this.updateToken(user?.userName!); 
      user.token = token;
      this.setCurrentUser(user as User);
  
      return response;
    } catch (error: any) {
      console.error('Errore durante l\'aggiornamento dell\'utente:', error);
      console.error('Dettagli errore:', error.error);
      throw error.error;
    }
  }
  

  deleteUser(usersId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${usersId}`).pipe(
      tap(() => this.clearCurrentUser())
    );
  }

  async updateToken(userName: string): Promise<any> {
    const url = `${this.apiUrl}/GetToken?userName=${userName}`;
  
    try {
      const response = await firstValueFrom(this.http.get(url, { responseType: 'text' }));
  
      console.log("Token ricevuto:", response);
      return response;
    } catch (error) {
      console.error('Errore nel recupero del token:', error);
      return [];
    }
  }

  updateProfileImg(id: number, file: File) {
    const headers = new HttpHeaders({
      'Authorization': 'Bearer ' + this.getToken()
    });
    
    const formData = new FormData();
    formData.append('file', file);
    
    const url = `${this.apiUrl}/upload-profile-image-local/${id}`;
    
    try {
      return this.http.post(url, formData, { 
        headers: headers,
        responseType: 'json'
      }).pipe(
        map(response => {
          console.log('Upload response:', response);
          return response as { url: string };
        }),
        catchError(error => {
          console.error('Errore durante l\'upload dell\'immagine:', error);
          return of({ Message: 'failed' });
        })
      ).toPromise();
    }
    catch (error) {
      console.error("ERROR", error);
      return Promise.reject(error);
    }
  }

  fetchCustomSuggestions(id: number, filter:string) {
    const url = `${this.apiUrl}/SuggestionsCocktailByUser/${id}` + "?type=" + filter;
    const headers = new HttpHeaders({
      'Authorization': 'Bearer ' + this.getToken(),
    });
    console.log(url, " DAHANE")
    return this.http.get<any>(url, { headers}).pipe(
      map((response) => {
        if (typeof response === 'string') {
          return [];
        } 
  
        if (response) {
          return response;
        }
  
        if (Array.isArray(response)) {
          return response;
        }
  
        return [];
      }),
      catchError((error) => {
        console.error('Errore:', error);
        return of([]);
      })
    );
  }


}
