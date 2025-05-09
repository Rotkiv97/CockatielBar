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
    likes: number;
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
interface Cocktail {
    id: string;
    idDrink: string;
    ingredients: string[];
    measures: string[];
    strAlcoholic: string;
    strCategory: string;
    strDrink: string;
    strDrinkThumb: string;
    strGlass: string;
    strInstructions: string;
    likes: number;
    isLiked: boolean;
  }

@Injectable({
    providedIn: 'root'
  })

  export class CocktailService {
      

        private apiUrl = "http://localhost:5052/api/cocktails/search?" + "nameCocktail=";

     
      constructor(  
        private http: HttpClient,
        private userService: UserService,
    )
    {}
    searchCocktails(Query: string, filter: string, Alcoholic:boolean) {
        
        let loggedIn = false;
        if (this.userService.getUser())
        {
            loggedIn = true;
        }
        let alcoholic_filter = "";
        if (!loggedIn || !Alcoholic)
        {
            alcoholic_filter =  "&alcoholic=Non%20alcoholic";
        }
        this.apiUrl = "http://localhost:5052/api/cocktails/search?" + filter;
        const Token = this.userService.getToken()
        const headers = new HttpHeaders({
        'Authorization': 'Bearer ' + Token
        });
        console.log(this.apiUrl + Query)
        return this.http.get(this.apiUrl + Query + alcoholic_filter, {headers: headers, responseType: 'text'}).pipe(
            map(response => {
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
        return this.http.get(this.apiUrl, { 
            headers: headers,
            responseType: 'text' 
        }).pipe(
            map(response => {
                return response ?? 'failed';
            }),
            catchError(error => {
                console.error('Errore durante la verifica del login:', error);
                return of('failed');
            })
        ).toPromise() as Promise<string>;
    }

    likeCocktail(id: number) {
        this.apiUrl = "http://localhost:5052/api/Cocktails/like/" + id;
        const headers = new HttpHeaders({
            'Authorization': 'Bearer ' + this.userService.getToken() 
        });
        return this.http.post(this.apiUrl, {}, { 
            headers: headers,
            responseType: 'text' 
        }).pipe(
            map(response => response ?? 'failed'),
            catchError(error => {
                console.error('Errore durante il like del cocktail:', error);
                return of('failed');
            })
        ).toPromise() as Promise<string>;
    }
        
        isLiked(id: number): Promise<boolean> {
            this.apiUrl = "http://localhost:5052/api/Users/ThisYourCocktailLike/" + id;
            const headers = new HttpHeaders({
                'Authorization': 'Bearer ' + this.userService.getToken()
            });
        
            return this.http.get(this.apiUrl, {
                headers: headers,
                responseType: 'text'
            }).pipe(
                map(response => {
                    console.log("Response from API:", response);
                    return response?.trim().toLowerCase() === 'true'; // garantisce booleano
                }),
                catchError(error => {
                    console.error('Errore durante la verifica del like:', error);
                    return of(false); // fallback booleano sicuro
                })
            ).toPromise() as Promise<boolean>;
        }

        getCocktailsLikedProfile(id: number): Promise<Cocktail[]> {
            this.apiUrl = "http://localhost:5052/api/Users/GetMyCocktailLike/" + id;
            const headers = new HttpHeaders({
                'Authorization': 'Bearer ' + this.userService.getToken()
            });
        
            return this.http.get<Cocktail[]>(this.apiUrl, { headers }).pipe(
                map(response => {
                    return response ?? []; 
                }),
                catchError(error => {
                    console.error('Errore durante la verifica del like:', error);
                    return of([]);
                })
            ).toPromise() as Promise<Cocktail[]>; 
        }             
        
    }

    