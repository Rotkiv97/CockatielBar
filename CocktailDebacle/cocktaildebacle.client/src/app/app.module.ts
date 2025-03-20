import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { HttpClientModule } from '@angular/common/http';

@NgModule({
  imports: [
    HttpClientModule, // Importa HttpClientModule per le richieste HTTP
  ],
})
export class AppModule {}

