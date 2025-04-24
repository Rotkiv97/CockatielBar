import { CommonModule } from '@angular/common';  // Importa CommonModule
import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UserService } from '../services/user.service';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-home',
  standalone: true,  // Se è un componente standalone
  imports: [
    CommonModule,     // Aggiungi CommonModule
    MatIconModule,
    MatMenuModule
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  Token: string | null = null;
  menuOpen = false;

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

  ngOnInit() {
    document.body.classList.add('cocktail-gradient-bg');
    this.checkLoginStatus();
  }

  ngOnDestroy() {
    document.body.classList.remove('cocktail-gradient-bg');
  }


  toggleMenu() {
    this.menuOpen = !this.menuOpen;
    console.log('Menu is open:', this.menuOpen);
  }

  isLoggedIn(): boolean {
    return !!this.Token; 
  }
  
  // And make sure your checkLoginStatus method is correctly updating the Token value:
  private async checkLoginStatus() {
    const user = this.userService.getUser();
    if (user) {
      try {
        this.Token = await this.userService.isLoggedIn(user);
      } catch (error) {
        console.error("Error checking login status:", error);
        this.Token = null;
      }
    } else {
      this.Token = null;
    }
  }
}
