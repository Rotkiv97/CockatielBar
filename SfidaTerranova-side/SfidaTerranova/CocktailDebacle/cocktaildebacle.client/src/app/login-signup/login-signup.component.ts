import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; // Importa CommonModule per le direttive comuni
import { RouterModule } from '@angular/router'; // Importa RouterModule per il routing

@Component({
  selector: 'app-login-signup', // Selettore del componente
  standalone: true, // Indica che è un componente standalone
  imports: [ReactiveFormsModule, CommonModule, RouterModule], // Importa i moduli necessari
  templateUrl: './login-signup.component.html', // Percorso del template HTML
  styleUrls: ['./login-signup.component.css'] // Percorso del file CSS
})
export class LoginSignupComponent {
  loginForm: FormGroup; // Definisce il form reattivo

  constructor(private fb: FormBuilder) {
    // Inizializza il form con i campi e le validazioni
    this.loginForm = this.fb.group({
      username: ['', Validators.required], // Campo obbligatorio
      password: ['', Validators.required] // Campo obbligatorio
    });
  }

  // Metodo chiamato alla submit del form
  onSubmit() {
    if (this.loginForm.valid) {
      console.log('Form submitted:', this.loginForm.value);
      // Qui puoi aggiungere la logica per il login (es. chiamata API)
    } else {
      console.log('Form is invalid');
    }
  }
}
