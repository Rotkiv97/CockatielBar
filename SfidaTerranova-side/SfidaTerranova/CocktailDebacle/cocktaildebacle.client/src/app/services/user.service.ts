import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface User {
  UserName: string;
  Name: string;
  LastName: string;
  Email: string;
  PasswordHash: string; // Hashed password
  PersonalizedExperience?: boolean;
  AcceptCookies?: boolean; // Corretto: "AcceptCookies" invece di "acceptCookis"
  Online?: boolean; // Corretto: "Online" invece di "online"
  Language?: string; // Corretto: "Language" invece di "language"
  ImgProfile?: string; // Corretto: "ImgProfile" invece di "imgProfile"
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5052/api/Users'; // Assicurati che l'URL sia corretto

  constructor(private http: HttpClient) { }

  // Metodo per la registrazione di un utente
  registerUser(user: User): Observable<any> {
    console.log("Invio richiesta di registrazione:", user); // Debug: stampa i dati inviati
    return this.http.post(`${this.apiUrl}/register`, user);
  }

  // Metodo per il login
  login(userName: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, { 
      UserNameRequest: userName, 
      PasswordRequest: password 
    });
  }

  // Metodo per l'aggiornamento di un utente
  updateUser(usersId: number, user: Partial<User>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${usersId}`, user);
  }

  // Metodo per l'eliminazione di un utente
  deleteUser(usersId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${usersId}`);
  }

  // Metodo per il recupero di un utente
  getUser(usersId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${usersId}`);
  }

  // Metodo per il recupero di tutti gli utenti
  getUsers(): Observable<any> {
    return this.http.get(`${this.apiUrl}`);
  }

  // Metodo per il logout
  logout(usersId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout/${usersId}`, {});
  }

  // Metodo per il recupero della password
  recoverPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/recoverPassword`, { 
      EmailRequest: email // Corretto: "EmailRequest" invece di "emailRequest"
    });
  }

  // Metodo per il reset della password
  resetPassword(usersId: number, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resetPassword/${usersId}`, { 
      PasswordRequest: password // Corretto: "PasswordRequest" invece di "passwordRequest"
    });
  }

  // Metodo per il cambio password
  changePassword(usersId: number, oldPassword: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/changePassword/${usersId}`, { 
      OldPasswordRequest: oldPassword, // Corretto: "OldPasswordRequest" invece di "oldPasswordRequest"
      NewPasswordRequest: newPassword // Corretto: "NewPasswordRequest" invece di "newPasswordRequest"
    });
  }

  // Metodo per il cambio email
  changeEmail(usersId: number, email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/changeEmail/${usersId}`, { 
      EmailRequest: email // Corretto: "EmailRequest" invece di "emailRequest"
    });
  }
}