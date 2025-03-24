import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = 'http://localhost:5000/api/users';
  constructor(private http: HttpClient) { }

  checkUser(user: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/checkUser`, { email: user.email, password: user.password });
  }
}
