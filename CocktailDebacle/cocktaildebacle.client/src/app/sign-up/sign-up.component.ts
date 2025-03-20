import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';

interface User {
  userName: string;
  name: string;
  lastName: string;
  email: string;
  passwordHash: string;
  acceptCookis: boolean;
}

@Component({
  selector: 'app-sign-up',
  standalone: true, // Imposta il componente come standalone
  imports: [ReactiveFormsModule, CommonModule], // Importa i moduli necessari
  templateUrl: './sign-up.component.html',
  styleUrls: ['./sign-up.component.css']
})
export class SignUpComponent {
  signupForm: FormGroup;

  constructor(private fb: FormBuilder, private userService: UserService) {
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
      const user: User = {
        userName: this.signupForm.value.userName,
        name: this.signupForm.value.name,
        lastName: this.signupForm.value.lastName,
        email: this.signupForm.value.email,
        passwordHash: this.signupForm.value.password, // L'API ASP.NET gestirà l'hashing
        acceptCookis: this.signupForm.value.acceptCookis
      };

      this.userService.registerUser(user).subscribe({
        next: (response) => console.log('Utente registrato:', response),
        error: (error) => console.error('Errore durante la registrazione:', error)
      });
    } 
    else {
      console.log('Il form non è valido');
    }
  }
}
