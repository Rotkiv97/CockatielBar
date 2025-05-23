import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// Material Modules
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input'; 
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBarModule } from '@angular/material/snack-bar';

import { TranslateHtmlService } from './services/translate-html.service';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    HttpClientModule,
    TranslateModule,
    
    // Material Modules
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatCheckboxModule,
    MatSnackBarModule,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  
  constructor(
    private translateService: TranslateService,
    private translateHtmlService: TranslateHtmlService
  ) {}
  
  ngOnInit() {
    // Impostazione della lingua predefinita
    this.translateService.setDefaultLang('en');
  }
  
  // Metodo per cambiare lingua
  changeLanguage(language: string) {
    // Aggiorna lingua per NGX-Translate
    this.translateService.use(language);
    
    // Utilizza anche il servizio personalizzato per elementi dinamici
    this.translateHtmlService.translateElements(language);
  }
}