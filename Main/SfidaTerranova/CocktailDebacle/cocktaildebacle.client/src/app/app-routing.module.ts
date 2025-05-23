import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component'; 
import { ProfilePageComponent } from './profile-page/profile-page.component';


const routes: Routes = [
  { path: 'home', component: HomeComponent },
  { path: 'profile-page/:username', component: ProfilePageComponent },
  { path: '', redirectTo: '/home', pathMatch: 'full' }
];



@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
