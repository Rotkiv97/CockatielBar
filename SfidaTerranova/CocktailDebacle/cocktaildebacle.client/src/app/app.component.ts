import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <!-- Header/Navbar può essere aggiunto qui -->
    <router-outlet></router-outlet>
    <!-- Footer può essere aggiunto qui -->
  `,
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Cokatiels';
}