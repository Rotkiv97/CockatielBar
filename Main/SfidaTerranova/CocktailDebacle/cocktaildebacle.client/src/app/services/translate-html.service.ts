  import { Injectable } from '@angular/core';
  import { TranslationService } from './translation.service';

  @Injectable({
    providedIn: 'root',
  })
  export class TranslateHtmlService {
    constructor(private translationService: TranslationService) {}

    translateElements(language: string) {
  const elements = document.querySelectorAll('h1, h2, h3, h4, li, span, p, button, span, div, input, a');
  const textsToTranslate: string[] = [];
  const elementMap: Element[] = [];

  elements.forEach(element => {
    const textContent = element.textContent?.trim();
    const isSimpleDiv = element.tagName.toLowerCase() !== 'div' || (element.childNodes.length === 1 && element.childNodes[0].nodeType === Node.TEXT_NODE);

    if (textContent && textContent.length > 0 && isSimpleDiv) {
      textsToTranslate.push(textContent);
      elementMap.push(element);
    }
  });

  if (textsToTranslate.length > 0) {
    this.translationService.translateText(textsToTranslate, language).subscribe({
      next: (response) => {
        if (response.result) {
          const translatedTexts = response.result.split('|||');

          // Applica la traduzione agli elementi che esistono
          translatedTexts.forEach((translatedText: string, index: number) => {
            const element = elementMap[index];
            if (translatedText && element && element.childNodes.length === 1 && element.childNodes[0].nodeType === Node.TEXT_NODE) {
              element.childNodes[0].textContent = translatedText;
            }
          });

          // Gestisce gli eventuali mismatch ma continua comunque con la traduzione
          if (translatedTexts.length < elementMap.length) {
            console.warn('Meno testi tradotti rispetto agli elementi. Traduzioni parziali applicate.');
          } else if (translatedTexts.length > elementMap.length) {
            console.warn('Più testi tradotti rispetto agli elementi. Alcuni testi potrebbero non essere applicati.');
          }
        }
      },
      error: (error) => {
        console.error('Errore durante la traduzione:', error);
      }
    });
  }
  }
}
