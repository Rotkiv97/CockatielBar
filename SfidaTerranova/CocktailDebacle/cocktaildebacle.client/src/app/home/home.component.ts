import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Router, RouterModule } from '@angular/router';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { UserService } from '../services/user.service';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-home',
  imports: [
    MatIconModule,
    MatMenuModule
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  constructor(
    private userService: UserService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  logout(): void {
    this.userService.logout().subscribe({
      next: () => this.router.navigate(['/login-signup']),
      error: (err: Error) => {
        console.error('Logout error:', err);
        this.userService.forceLogout();
        this.snackBar.open('Sessione terminata', 'OK', { duration: 3000 });
      }
    });
  }
}