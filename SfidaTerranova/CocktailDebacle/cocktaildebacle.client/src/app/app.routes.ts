import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';  
import { LoginSignupComponent } from './login-signup/login-signup.component';
import { SignUpComponent } from './sign-up/sign-up.component';
import { PrivacyPolicyComponent } from './privacy-policy/privacy-policy.component';
import { ProfilePageComponent } from './profile-page/profile-page.component';
export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent },
  { path: 'profile-page', component: ProfilePageComponent},
  { path: 'login-signup', component: LoginSignupComponent },
  { path: 'sign-up', component: SignUpComponent },
  { path: 'privacy-policy', component: PrivacyPolicyComponent },
  { path: 'profile', component: ProfilePageComponent },
];
