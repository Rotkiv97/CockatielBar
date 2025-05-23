
import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewEncapsulation} from '@angular/core';
import { TranslateHtmlService } from '../services/translate-html.service';
@Component({
  selector: 'app-privacy-policy',
  standalone: true, 
  imports: [CommonModule], 
  encapsulation: ViewEncapsulation.None,
  templateUrl: './privacy-policy.component.html',
  styleUrls: ['./privacy-policy.component.css']
})
export class PrivacyPolicyComponent {
    constructor(
    private translateHtmlService: TranslateHtmlService
  ) {}
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

  goBack() {
    window.history.back(); 
  }
}
