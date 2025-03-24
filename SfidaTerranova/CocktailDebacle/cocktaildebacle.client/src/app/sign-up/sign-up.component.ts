import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';
import { Router } from '@angular/router';

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
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './sign-up.component.html',
  styleUrls: ['./sign-up.component.css']
})
export class SignUpComponent {
  signupForm: FormGroup;
  message: string = '';
  isSuccess: boolean = false;

  constructor(
    private fb: FormBuilder, 
    private userService: UserService,
    private router: Router
  ) {
    this.signupForm = this.fb.group({
      FirstName: ['', Validators.required],
      LastName: ['', Validators.required],
      UserName: ['', Validators.required],
      Email: ['', [Validators.required, Validators.email]],
      ConfirmEmail: ['', [Validators.required, Validators.email]],
      Password: ['', [Validators.required, Validators.minLength(8)]],
      ConfirmPassword: ['', [Validators.required, Validators.minLength(8)]],
      AcceptCookies: [false, Validators.requiredTrue]
    }, { validators: [this.checkPasswords, this.checkEmails] });
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
        UserName: this.signupForm.value.UserName,
        Name: this.signupForm.value.FirstName,
        LastName: this.signupForm.value.LastName,
        Email: this.signupForm.value.Email,
        PasswordHash: this.signupForm.value.Password,
        AcceptCookies: this.signupForm.value.AcceptCookies
      };
      
      this.userService.registerUser(user).subscribe({
        next: (response) => {
          this.showMessage('Iscrizione avvenuta con successo!', true);
          // Reindirizza alla home dopo 2 secondi
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 2000);
        },
        error: (error) => {
          console.error('Errore durante la registrazione:', error);
          const errorMessage = error.error?.message || 'Errore durante la registrazione. Riprova più tardi.';
          this.showMessage(errorMessage, false);
        }
      });
    } else {
      this.showMessage('Per favore, compila correttamente tutti i campi.', false);
    }
  }

  // Mostra un messaggio di stato
  showMessage(msg: string, isSuccess: boolean) {
    this.message = msg;
    this.isSuccess = isSuccess;
    
    // Nascondi automaticamente il messaggio dopo 5 secondi
    if (isSuccess) {
      setTimeout(() => {
        this.dismissMessage();
      }, 5000);
    }
  }

  // Nasconde il messaggio
  dismissMessage() {
    this.message = '';
  }
}