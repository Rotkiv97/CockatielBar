import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';  
import { LoginSignupComponent } from './login-signup/login-signup.component';
import { SignUpComponent } from './sign-up/sign-up.component';
import { PrivacyPolicyComponent } from './privacy-policy/privacy-policy.component';
import { SettingsComponent } from './settings/settings.component';
import { ProfilePageComponent } from './profile-page/profile-page.component';
import { HelpComp} from './helpcomp/helpcomp.component'
import { CocktailsComponent } from './cocktails/cocktails.component';
import { WhoareWeComponent } from './whoare-we/whoare-we.component';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';


export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent },
  { path: 'profile-page', component: ProfilePageComponent },  
  { path: 'profile-page/:username', component: ProfilePageComponent },  
  { path: 'login-signup', component: LoginSignupComponent },
  { path: 'sign-up', component: SignUpComponent },
  { path: 'privacy-policy', component: PrivacyPolicyComponent },
  { path: 'cocktails', component: CocktailsComponent },
  { path: 'whaw', component: WhoareWeComponent },
  { path: 'help', component: HelpComp},
  { path: 'settings', component: SettingsComponent},
  {
    path: '**',
    resolve: {
      action: () => {
        const snackBar = inject(MatSnackBar);
        const router = inject(Router);
        snackBar.open('Page not found', 'OK', { duration: 3000 });
        router.navigate(['/cocktails']);
      }
    },
    component: ProfilePageComponent,
  }
];