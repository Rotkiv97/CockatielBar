import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';
import { Router, RouterModule } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';

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
  imports: [
    ReactiveFormsModule,
    FormsModule,
    CommonModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatCheckboxModule,
    MatSnackBarModule
  ],
  templateUrl: './sign-up.component.html',
  styleUrls: ['./sign-up.component.css']
})
export class SignUpComponent {
  signupForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.signupForm = this.fb.group({
      FirstName: ['', [Validators.required, Validators.minLength(2)]],
      LastName: ['', Validators.required],
      UserName: ['', Validators.required],
      Email: ['', [Validators.required, Validators.email]],
      ConfirmEmail: ['', [Validators.required, Validators.email]],
      Password: ['', [
        Validators.required, 
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[A-Z])(?=.*\d)/)
      ]],
      ConfirmPassword: ['', [Validators.required, Validators.minLength(8)]],
      AcceptCookies: [false, Validators.requiredTrue]
    }, { validators: [this.checkPasswords, this.checkEmails] });
  }

  // Add this method to handle navigation
  navigateToLoginSignup() {
    this.router.navigate(['/login-signup']);
  }

  checkPasswords(group: FormGroup) {
    const password = group.get('Password')?.value;
    const confirmPassword = group.get('ConfirmPassword')?.value;
    return password === confirmPassword ? null : { notSame: true };
  }

  checkEmails(group: FormGroup) {
    const email = group.get('Email')?.value;
    const confirmEmail = group.get('ConfirmEmail')?.value;
    return email === confirmEmail ? null : { emailsNotSame: true };
  }

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
          this.snackBar.open('Registrazione avvenuta con successo!', 'Chiudi', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
          setTimeout(() => {
            this.router.navigate(['/home']);
          }, 1500);
        },
        error: (error) => {
          let errorMessage = 'Errore durante la registrazione';
          console.log('Full error object:', error); // Log the complete error
          console.log('Error status:', error.status);
          console.log('Error response:', error.error);
          if (error.status === 409) {
            errorMessage = 'Email già in uso';
          } else if (error.status === 400) {
            errorMessage = error.error?.message || 'Dati non validi';
          }

          this.snackBar.open(errorMessage, 'Chiudi', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
    } else {
      this.snackBar.open('Compila correttamente tutti i campi', 'Chiudi', {
        duration: 5000,
        panelClass: ['warning-snackbar']
      });
    }
  }
}