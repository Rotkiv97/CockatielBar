import { HttpClient } from '@angular/common/http';

interface User {
  id: number;
  name: string;

  // add other properties as needed
}
import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})

export class HomeComponent {

  public users: User[] = [];
  constructor(private http: HttpClient){}

  ngOnInit(): void {
    this.getUsers();
  }

  getUsers(): void {
    this.http.get<User[]>('https://localhost:5000/api/users').subscribe(users => {
      this.users = users;
    });
  }
  title = 'cocktaildebacle.client';
}
