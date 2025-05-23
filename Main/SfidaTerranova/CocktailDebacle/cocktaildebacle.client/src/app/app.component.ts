import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { TranslateHtmlService } from './services/translate-html.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <!-- Header/Navbar can be added here -->
    <div *ngIf="no" class="language-selector">
      <button (click)="toggleLanguageMenu($event)">🌍 Language</button>

      <div *ngIf="isMenuOpen" class="dropdown-menu" [ngStyle]="{ 'top': menuPosition.top, 'left': menuPosition.left }">
        <div (click)="changeLanguage('en')" class="language-option">
          🇬🇧 English
        </div>
        <div (click)="changeLanguage('it')" class="language-option">
          🇮🇹 Italian
        </div>
        <div (click)="changeLanguage('es')" class="language-option">
          🇪🇸 Spanish
        </div>
        <div (click)="changeLanguage('de')" class="language-option">
          🇩🇪 German
        </div>
        <div (click)="changeLanguage('fr')" class="language-option">
          🇫🇷 French
        </div>
      </div>
    </div>

    <router-outlet></router-outlet>

    <!-- Footer can be added here -->
  `,
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Cokatiels';
  isMenuOpen = false;
  no = false;
  menuPosition = { top: '0px', left: '0px' }; // Initial position for the dropdown menu

  constructor(private translateHtmlService: TranslateHtmlService) {}

  // Toggle the language menu visibility
  toggleLanguageMenu(event: MouseEvent) {
    this.isMenuOpen = !this.isMenuOpen;

    // Position the menu based on the button click
    const button = event.target as HTMLElement;
    const rect = button.getBoundingClientRect();
    this.menuPosition.top = `${rect.bottom + window.scrollY}px`; // Below the button
    this.menuPosition.left = `${rect.left + window.scrollX}px`; // Align with the button
  }

  // Function to change the language
  changeLanguage(language: string) {
    this.translateHtmlService.translateElements(language);
  }
}
