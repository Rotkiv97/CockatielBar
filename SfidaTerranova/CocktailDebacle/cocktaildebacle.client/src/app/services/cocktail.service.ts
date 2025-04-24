import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';
import { map, catchError} from 'rxjs/operators'
import { of } from 'rxjs';
import { UserService } from './user.service';


export interface CocktailDetails{
    CocktailCreator: string;
    Public: boolean;
    CreationDate: Date;
    Likes: number;
    Id: number;
    Name: string;
    Category: string;
    Aloholic: boolean;
    Glass: string;
    Instructions: string;
    ImgUrl: string;
    ingredients: string[];
    measures: string[];
} 

@Injectable({
    providedIn: 'root'
  })

  export class CocktailService {
      
        private filter = "nameCocktail=";
        private apiUrl = "http://localhost:5052/api/cocktails/search?" + this.filter;

     
      constructor(  
        private http: HttpClient,
        private userService: UserService,
    )
    {
        
    }

    filters(Choice:string)
    {
        switch (Choice) {
            case "Cocktail":
                this.filter = "nameCocktail=";
                break;
            case "Ingredient":
                this.filter = "ingredient=";
                break;
            case "Glass":
                this.filter = "glass=";
                break;
            default:
                break;
        }

    }


    searchCocktails(Query:string)
    {
        this.apiUrl = "http://localhost:5052/api/cocktails/search?" + this.filter;
        return this.http.get(this.apiUrl + Query, { responseType: 'text' }).pipe(
            map(response => {
                 // <-- Aggiunto log
                return response ?? 'failed';
              }), 
            catchError(error => {
              console.error('Errore durante la verifica del login:', error);
              return of('failed');
            })
          ).toPromise() as Promise<string>;
        }

        getCocktailById(id: number) {
            this.apiUrl = "http://localhost:5052/api/Cocktails/cocktail/by-id?id=" + id;
        
            const headers = new HttpHeaders({
                'Authorization': 'Bearer ' + this.userService.getToken() 
            });
            console.log(this.userService.getToken(), " CANE DEDIOCANE")
            return this.http.get(this.apiUrl, { 
                headers: headers,
                responseType: 'text' 
            }).pipe(
                map(response => {
                    console.log("HEU", response);
                    return response ?? 'failed';
                }),
                catchError(error => {
                    console.error('Errore durante la verifica del login:', error);
                    return of('failed');
                })
            ).toPromise() as Promise<string>;
        }
        
    }

    