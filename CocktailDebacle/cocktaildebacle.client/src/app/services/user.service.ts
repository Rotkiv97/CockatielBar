import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';


export interface User {
  userName: string;
  name: string;
  lastName: string;
  email: string;
  passwordHash: string; // Hashed password
  personalizedExperience?: boolean;
  acceptCookis?: boolean;
  online?: boolean;
  leanguage?: string;
  imgProfile?: string;
}

@Injectable({
  providedIn: 'root'
})


export class UserService {
  private apiUrl = 'http://localhost:5052/api/Users';

  constructor(private http: HttpClient) { }

  // Metodo per la registrazione di un utente
  registerUser(user: User): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user);
  }

  // Metodo per il login
  login(userName: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, { UserNameRequest: userName, PasswordRequest: password });
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
  logout(usersId: Number): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout/${usersId}`, {});
  }

  // Metodo per il recupero della password
  recoverPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/recoverPassword`, { emailRequest: email });
  }

  // Metodo per il reset della password
  resetPassword(usersId: Number, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resetPassword/${usersId}`, { passwordRequest: password });
  }

  // Metodo per il cambio password
  changePassword(usersId: Number, oldPassword: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/changePassword/${usersId}`, { oldPasswordRequest: oldPassword, newPasswordRequest: newPassword });
  }

  // Metodo per il cambio email
  changeEmail(usersId: Number, email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/changeEmail/${usersId}`, { emailRequest: email });
  }
}
