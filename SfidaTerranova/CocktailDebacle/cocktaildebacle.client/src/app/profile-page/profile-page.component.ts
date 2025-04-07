import { Component, OnInit, HostListener, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { UserService } from '../services/user.service';

interface Cocktail {
  id: string;
  name: string;
  image?: string;
  likes?: number;
}

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatToolbarModule,
    MatMenuModule,
    MatSnackBarModule,
    MatTabsModule
  ],
  templateUrl: './profile-page.component.html',
  styleUrls: ['./profile-page.component.css']
})
export class ProfilePageComponent implements OnInit {
  isLoggedIn = false;
  Token: string = '';
  username: string = '';
  profileImage: string = 'assets/default-profile.png';
  parallaxImage: string = 'assets/default-profile-background.png';
  parallaxTransform = 'translateY(0)';
  currentYear: number = new Date().getFullYear();
  likedCocktails: Cocktail[] = [];
  myCocktails: Cocktail[] = [];
  activeTab: 'cocktails' | 'liked' | 'friends' = 'cocktails';
  Bio: string = "";
  Bio_link: string = "";

  constructor(
    private userService: UserService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.checkAuthStatus();
  }

  @HostListener('window:scroll', ['$event'])
  onWindowScroll() {
    const scrollPosition = window.pageYOffset;
    this.parallaxTransform = `translateY(${scrollPosition * 0.2}px) scale(1.02)`;
  }

  private async checkAuthStatus(): Promise<void> {
    const user = this.userService.getUser();
    
    if (user) {
      this.Token = await this.userService.isLoggedIn(user);
      if (this.Token === null) {
        this.handleUnauthorizedAccess();
      } else {
        this.isLoggedIn = true;
        this.username = user.userName;
        this.profileImage = user.ImgProfile || 'assets/default-profile.png';
        this.parallaxImage = user.ProfileParallaxImg || 'assets/default-profile-background.png';
        this.loadCocktails();
        this.Bio = user.Bio || 'Cocktail enthusiast 🍹 | Creating delicious drinks';
        this.Bio_link = user.Bio_link || 'https://www.example.com';
      }
    } else {
      this.isLoggedIn = false;
      this.handleUnauthorizedAccess('Errore durante il recupero dell\'utente');
    }
  }

  private loadCocktails(): void {
    // Implement your cocktail loading logic here
    this.likedCocktails = [];
    this.myCocktails = [];
  }

  private handleUnauthorizedAccess(message: string = 'Accesso non autorizzato'): void {
    this.snackBar.open(message, 'OK', { duration: 3000 });
    this.router.navigate(['/login-signup']);
  }

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

  getSelectedTabIndex(): number {
    return ['cocktails', 'liked', 'friends'].indexOf(this.activeTab);
  }

  onTabChange(index: number): void {
    const tabs: ('cocktails' | 'liked' | 'friends')[] = ['cocktails', 'liked', 'friends'];
    this.activeTab = tabs[index];
  }

  updateBackground(imageUrl: string): void {
    this.parallaxImage = imageUrl;
    // Add logic to save preference if needed
  }


}
@Component({
  selector: 'app-tabs',
  templateUrl: './tabs.component.html',
  styleUrls: ['./tabs.component.scss'],
  encapsulation: ViewEncapsulation.None, // <--- QUESTO è IL TRUCCO
})
export class TabsComponent { }