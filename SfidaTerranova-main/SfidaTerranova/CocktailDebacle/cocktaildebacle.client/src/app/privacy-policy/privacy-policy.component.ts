
import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewEncapsulation} from '@angular/core';

@Component({
  selector: 'app-privacy-policy',
  standalone: true, // Imposta il componente come standalone
  imports: [CommonModule], // Importa i moduli necessari
  encapsulation: ViewEncapsulation.None,
  templateUrl: './privacy-policy.component.html',
  styleUrls: ['./privacy-policy.component.css']
})
export class PrivacyPolicyComponent {
  goBack() {
    window.history.back(); 
  }
}
