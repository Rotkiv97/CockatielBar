import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { enableProdMode } from '@angular/core';

// Abilita la modalità produzione se necessario (es. in environment.prod.ts)
// if (environment.production) {
//   enableProdMode();
// }

bootstrapApplication(AppComponent, appConfig)
  .then(() => console.log('Applicazione avviata con successo'))
  .catch(err => {
    console.error('Errore durante il bootstrap:', err);
    // Qui puoi aggiungere log aggiuntivi o inviare l'errore a un servizio di tracking
  });
