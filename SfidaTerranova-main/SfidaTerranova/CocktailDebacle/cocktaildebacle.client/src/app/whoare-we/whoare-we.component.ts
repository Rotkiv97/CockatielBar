import { Component, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { FormsModule } from '@angular/forms';
import { Location } from '@angular/common'; // Importa il modulo Location per la navigazione

@Component({
  selector: 'app-whoare-we',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatToolbarModule,
    MatMenuModule,
    MatSnackBarModule,
    MatTabsModule,
    FormsModule,
  ],
  templateUrl: './whoare-we.component.html',
  styleUrls: ['./whoare-we.component.css']
})
export class WhoareWeComponent {
  menuOpen = false;
  selectedProfile: string = '';
  modalDescription: string = '';
  modalImageSrc: string = '';

  profileDescriptions: { [key in 'mmar' | 'vic']: string } = {
    mmar: 'Creative Director con passione per il design elegante e funzionale.',
    vic: 'Developer instancabile con la mente sempre proiettata al futuro.'
  };

  profileImages: { [key in 'mmar' | 'vic']: string } = {
    mmar: 'assets/mmar.png',
    vic: 'assets/vic.png'
  };

  constructor(private location: Location) {} // Aggiungi il costruttore per iniettare Location

  ngOnInit() {
    // Aggiungi il listener per il tasto 'Esc'
    document.addEventListener('keydown', this.onKeyDown.bind(this));
  }
  
  ngOnDestroy() {
    // Rimuovi il listener quando il componente viene distrutto
    document.removeEventListener('keydown', this.onKeyDown.bind(this));
  }
  openModal(profile: 'mmar' | 'vic'): void {
    this.selectedProfile = profile;
    this.modalDescription = this.profileDescriptions[profile] || '';
    this.modalImageSrc = this.profileImages[profile];
    const modal = document.getElementById('modal');
    if (modal) {
      modal.style.display = 'block';
    }
  }


  closeModal(): void {
    const modal = document.getElementById('modal');
    if (modal) {
      modal.style.display = 'none';
    }
  }
  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      this.closeModal();
    }
  }
  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }
  

  isLoggedIn(): boolean {
    return true; // Logica di controllo login (da sostituire con la tua logica effettiva)
  }

  logout(): void {
    console.log('User logged out');
  }

  goBack(): void {
    this.location.back(); // Naviga indietro usando il router
  }
}
