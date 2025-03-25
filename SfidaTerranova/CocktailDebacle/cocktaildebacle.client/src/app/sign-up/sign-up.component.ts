import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';

interface User {
  UserName: string;
  Name: string;
  LastName: string;
  Email: string;
  PasswordHash: string;
  AcceptCookies: boolean;
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
      FirstName: ['', Validators.required], // Nome
      LastName: ['', Validators.required], // Cognome
      UserName: ['', Validators.required], // Username
      Email: ['', [Validators.required, Validators.email]], // Email
      ConfirmEmail: ['', [Validators.required, Validators.email]], // Conferma email
      Password: ['', [Validators.required, Validators.minLength(8)]], // Password
      ConfirmPassword: ['', [Validators.required, Validators.minLength(8)]], // Conferma password
      AcceptCookies: [false, Validators.requiredTrue] // Accettazione cookie
    }, { validators: [this.checkPasswords, this.checkEmails] }); // Validatori personalizzati
  }

  // Validatore personalizzato per conferma password
  checkPasswords(group: FormGroup) {
    const password = group.get('Password')?.value;
    const confirmPassword = group.get('ConfirmPassword')?.value;
    return password === confirmPassword ? null : { notSame: true };
  }

  // Validatore personalizzato per conferma email
  checkEmails(group: FormGroup) {
    const email = group.get('Email')?.value;
    const confirmEmail = group.get('ConfirmEmail')?.value;
    return email === confirmEmail ? null : { emailsNotSame: true };
  }

  // Metodo chiamato alla submit del form
  onSubmit() {
    if (this.signupForm.valid) {
      const user: User = {
        UserName: this.signupForm.value.UserName, // Usa "UserName"
        Name: this.signupForm.value.FirstName,   // Usa "FirstName"
        LastName: this.signupForm.value.LastName, // Usa "LastName"
        Email: this.signupForm.value.Email,      // Usa "Email"
        PasswordHash: this.signupForm.value.Password, // Usa "Password"
        AcceptCookies: this.signupForm.value.AcceptCookies // Usa "AcceptCookies"
      };
      console.log('Dati utente:', user);
      this.userService.registerUser(user).subscribe({
        next: (response) => console.log('Utente registrato:', response),
        error: (error) => console.error('Errore durante la registrazione:', error)
      });
    } else {
      console.log('Il form non è valido');
    }
  }
}