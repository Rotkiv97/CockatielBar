import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-sign-up',
  standalone: true, // Imposta il componente come standalone
  imports: [ReactiveFormsModule, CommonModule], // Importa i moduli necessari
  templateUrl: './sign-up.component.html',
  styleUrls: ['./sign-up.component.css']
})
export class SignUpComponent {
  signupForm: FormGroup;

  constructor(private fb: FormBuilder) {
    this.signupForm = this.fb.group({
      firstName: ['', Validators.required], // Nome
      lastName: ['', Validators.required], // Cognome
      username: ['', Validators.required], // Username
      email: ['', [Validators.required, Validators.email]], // Email
      confirmEmail: ['', [Validators.required, Validators.email]], // Conferma email
      password: ['', [Validators.required, Validators.minLength(8)]], // Password
      confirmPassword: ['', [Validators.required, Validators.minLength(8)]], // Conferma password
      acceptCookies: [false, Validators.requiredTrue] // Accettazione cookie
    }, { validator: this.checkPasswords }); // Validatore personalizzato per conferma password
  }

  // Validatore personalizzato per conferma password
  checkPasswords(group: FormGroup) {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { notSame: true };
  }

  // Metodo chiamato alla submit del form
  onSubmit() {
    if (this.signupForm.valid) {
      console.log('Form submitted:', this.signupForm.value);
      // Qui puoi aggiungere la logica per la registrazione (es. chiamata API)
    } else {
      console.log('Form is invalid');
    }
  }
}
