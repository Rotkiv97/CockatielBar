import { CommonModule} from '@angular/common';  // Importa CommonModule
import { Component, ViewChild,  ElementRef, ViewEncapsulation  } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UserService } from '../services/user.service';
import { MatMenuModule } from '@angular/material/menu';
import { gsap } from 'gsap/gsap-core';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,  
  imports: [
    CommonModule,     
    MatIconModule,
    MatMenuModule
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class HomeComponent {
  @ViewChild('explosionEmitter', { static: true }) explosionEmitter!: ElementRef;

  Token: string | null = null;
  menuOpen = false;
  emitterSize = 20;
  dotQuantity = 40;
  dotSizeMin = 6;
  dotSizeMax = 8;
  explosionQuantity = 5;
  routineSub!: Subscription;
  

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
    const container = this.createExplosionContainer();
    this.createExplosion(container).restart();
    
    this.routineSub = interval(3000).subscribe(() => {
      this.triggerRandomExplosion();    
      });
  }
  ngAfterViewInit(): void {
    this.setupExplosionHandler();
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

  getRandom(min: number, max: number): number {
    return min + Math.random() * (max - min);
  }
  createExplosion(container: HTMLElement): gsap.core.Timeline {
    const tl = gsap.timeline({ paused: true });
    const dots: HTMLElement[] = [];
  
    for (let i = 0; i < this.dotQuantity; i++) {
      const dot = document.createElement('div');
      dot.className = 'dot';
  
      const size = this.getRandom(this.dotSizeMin, this.dotSizeMax);
      const angle = this.getRandom(0, Math.PI * 2);
      const distance = this.getRandom(30, 100);
      const x = Math.cos(angle) * distance;
      const y = Math.sin(angle) * distance;
  
      dot.style.position = 'absolute';
      dot.style.width = `${size}px`;
      dot.style.height = `${size}px`;
      dot.style.borderRadius = '50%';
      dot.style.backgroundColor = `rgb(${this.getRandom(30, 255)}, ${this.getRandom(30, 230)}, ${this.getRandom(30, 230)})`;
      dot.style.left = '0px';
      dot.style.top = '0px';
      dot.style.pointerEvents = 'none';
  
      container.appendChild(dot);
      dots.push(dot);
  
      tl.fromTo(dot, {
        x: 0,
        y: 0,
        scale: 1,
        opacity: 1,
      }, {
        x,
        y,
        scale: 0.2,
        opacity: 0,
        duration: 1.5 + Math.random(),
        ease: 'power3.out',
      }, 0);
    }
  
    return tl;
  }
  

  setupExplosionHandler(): void {
    const emitter = this.explosionEmitter.nativeElement;
    emitter.addEventListener('click', () => {
      const container = this.createExplosionContainer();
      this.createExplosion(container).restart();
    });
  }

  triggerRandomExplosion(): void {
    // Posizione casuale sulla pagina
    const x = Math.random() * window.innerWidth;
    const y = Math.random() * window.innerHeight;
  
    const container = document.createElement('div');
    container.className = 'dot-container';
    document.body.appendChild(container);
  
    Object.assign(container.style, {
      position: 'absolute',
      left: `${x}px`,
      top: `${y}px`,
      pointerEvents: 'none',
      width: '0px',
      height: '0px',
      overflow: 'visible'
    });
    
  
    const explosion = this.createExplosion(container);
    explosion.restart();
  }


  createExplosionContainer(): HTMLElement {
    const container = document.createElement('div');
    container.className = 'dot-container';
    container.style.position = 'absolute';
    container.style.left = `${this.getRandom(0, window.innerWidth)}px`;
    container.style.top = `${this.getRandom(0, window.innerHeight)}px`;
    document.body.appendChild(container);
    return container;
  }
  
  
  
  
}
