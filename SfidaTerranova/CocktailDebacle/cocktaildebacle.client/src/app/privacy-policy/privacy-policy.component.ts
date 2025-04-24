import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-privacy-policy',
  standalone: true, // Imposta il componente come standalone
  imports: [CommonModule], // Importa i moduli necessari
  templateUrl: './privacy-policy.component.html',
  styleUrls: ['./privacy-policy.component.css']
})
export class PrivacyPolicyComponent {
  goBack() {
    window.history.back(); // Torna alla pagina precedente
  }
}
