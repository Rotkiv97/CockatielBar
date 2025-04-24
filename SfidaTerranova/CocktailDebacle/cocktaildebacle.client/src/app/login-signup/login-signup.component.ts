import { Component, AfterViewInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; // Importa CommonModule per le direttive comuni
import { RouterModule, Router } from '@angular/router'; // Importa RouterModule per il routing
import { UserService } from '../services/user.service'; // Importa il servizio utente
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { initLoginAnimations } from './cartoon';


var armL: HTMLElement | null;
var armR: HTMLElement | null;

@Component({
  
  selector: 'app-login-signup', 
  standalone: true, 
  imports: [ReactiveFormsModule, CommonModule, RouterModule], 
  templateUrl: './login-signup.component.html', 
  styleUrls: ['./login-signup.component.css'] 
})
export class LoginSignupComponent  implements AfterViewInit{
  loginForm: FormGroup; // Definisce il form reattivo
  constructor(
    private fb: FormBuilder, 
    private userService: UserService, 
    private snackBar: MatSnackBar,
    private router: Router,) {
    // Inizializza il form con i campi e le validazioni
    
    this.loginForm = this.fb.group({
      username: ['', Validators.required], // Campo obbligatorio
      password: ['', Validators.required] // Campo obbligatorio
    });
  }

  ngAfterViewInit(): void {
    initLoginAnimations();
  }
  // Metodo chiamato alla submit del form
  onSubmit() {
    if (this.loginForm.valid) {
      this.userService.login(
        this.loginForm.value.username,
        this.loginForm.value.password
      ).subscribe({
        next: (response) => {
          this.snackBar.open('Success', 'Chiudi', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
          //console.log( this.userService.getUser());
          setTimeout(() => {
            this.router.navigate(['/profile-page']); 
          }, 1500);
        },
        error: (error) => {
          console.log('Error response:', error.error);
          this.snackBar.open(error.error, 'Chiudi', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
          this.snackBar.open(error.error, 'Chiudi', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
    }
    else 
    {  
      this.snackBar.open("Invalid Username or Password.", 'Chiudi', {
        duration: 5000,
        panelClass: ['error-snackbar']
      });
    }
  }
}







