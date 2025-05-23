import { Component, AfterViewInit, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; 
import { RouterModule, Router } from '@angular/router'; 
import { UserService } from '../services/user.service'; 
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { initLoginAnimations } from './cartoon';
import { TranslateHtmlService } from '../services/translate-html.service';


var armL: HTMLElement | null;
var armR: HTMLElement | null;

@Component({
  
  selector: 'app-login-signup', 
  standalone: true, 
  imports: [ReactiveFormsModule, CommonModule, RouterModule], 
  templateUrl: './login-signup.component.html', 
  styleUrls: ['./login-signup.component.css'] ,
  encapsulation: ViewEncapsulation.None,
})
export class LoginSignupComponent  implements AfterViewInit{
  loginForm: FormGroup; 
  constructor(
    private fb: FormBuilder, 
    private userService: UserService, 
    private snackBar: MatSnackBar,
    private router: Router,
    private translateHtmlService: TranslateHtmlService) {
    
    this.loginForm = this.fb.group({
      username: ['', Validators.required], 
      password: ['', Validators.required] 
    });
  }

    ngOnInit() {

      const savedLanguage = localStorage.getItem('preferredLanguage');
      console.log('Saved Language:', savedLanguage); 
      if (savedLanguage) {
        console.log("Language found in localStorage:", savedLanguage);
        this.translateHtmlService.translateElements(savedLanguage);
      } else {
        console.log("No language found, setting to default");
        this.translateHtmlService.translateElements('en'); 
      }
  }

  ngAfterViewInit(): void {
    initLoginAnimations();
  }
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







