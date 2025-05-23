import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

const API_URL = 'http://localhost:5052/api/Translation/translate';

interface TranslationRequest {
  Text: string;
  ToLanguage: string;
  FromLanguage: string;
}

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  constructor(private http: HttpClient) {}

  translateText(texts: string[], language: string, fromLanguage: string = 'en'): Observable<any> {
    const cleanedTexts = texts
      .map(text => text.trim())
      .filter(text => text.length > 0);

    const joinedText = cleanedTexts.join('|||');

    const requestPayload: TranslationRequest = {
      Text: joinedText,
      ToLanguage: language,
      FromLanguage: fromLanguage
    };

    return this.http.post<any>(API_URL, requestPayload);
  }
}
