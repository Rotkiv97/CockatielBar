import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';
import { map, catchError} from 'rxjs/operators'
import { of } from 'rxjs';
import { UserService } from './user.service';
import { MatSnackBar } from '@angular/material/snack-bar';

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
         private snackBar: MatSnackBar
    )
    {}



        async updateCocktail( cocktail: any, id: number,) {
          const url = `http://localhost:5052/api/Cocktails/CocktailUpdate/${id}`;
          
          const headers = new HttpHeaders({
            'Authorization': 'Bearer ' + this.userService.getToken(),
            'Content-Type': 'application/json'
          });

          try {
            const response = await this.http.put(url, cocktail, {
              headers: headers,
              responseType: 'json'
            }).toPromise();

            console.log('Cocktail aggiornato:', response);
            // Eventualmente esegui altre operazioni come aggiornare il token o lo stato dell'utente
            // Esegui eventuali operazioni aggiuntive, es. aggiornare il token dell'utente
            return response;
          } catch (error: any) {
            console.error('Errore durante l\'aggiornamento del cocktail:', error);
            console.error('Dettagli errore:', error.error);
            throw error.error;
          }
        }

  

    createCocktail(Cocktail: any): Promise<string> {
    this.apiUrl = "http://localhost:5052/api/Cocktails/CocktailCreate";
    const headers = new HttpHeaders({
        'Authorization': 'Bearer ' + this.userService.getToken()
    });

    return this.http.post(this.apiUrl, Cocktail, { headers })
        .pipe(
            map(response => response as any ?? 'failed'),
            catchError(error => {
                const backendMessage = error?.error || 'Unknown error';
                console.error('Errore durante la creazione del cocktail:', backendMessage);

                this.snackBar.open(`Error: ${backendMessage}`, 'Close', {
                    duration: 5000,
                    panelClass: ['snackbar-error']
                });

                return of('failed');
            })
        ).toPromise() as Promise<string>;
}

    getUserCocktails()
    {
        this.apiUrl = "http://localhost:5052/api/cocktails/MyCocktails"
        const Token = this.userService.getToken();
        const headers = new HttpHeaders({
          'Authorization': 'Bearer ' + Token
        });
      
        return this.http.get(this.apiUrl, {headers: headers, responseType: 'text'}).pipe(
          map(response => {
            try {

              const parsedResponse = JSON.parse(response);
              return parsedResponse; 
            } catch (error) {
              console.error('Errore durante il parsing della risposta:', error);
              return 'failed';  
            }
          }), 
          catchError(error => {
            console.error('Errore durante la verifica del login:', error);
            return of('failed');
          })
        ).toPromise() as Promise<any>;  
      }
  
  

    searchCat(Query: string, id: number) {
        this.apiUrl = "http://localhost:5052/api/cocktails/SearchCategory/searchCategory?id=" + id + "&category=" + Query;
        const Token = this.userService.getToken();
        const headers = new HttpHeaders({
          'Authorization': 'Bearer ' + Token
        });
      
        return this.http.get(this.apiUrl, {headers: headers, responseType: 'text'}).pipe(
          map(response => {
            try {

              const parsedResponse = JSON.parse(response);
              return parsedResponse; 
            } catch (error) {
              console.error('Errore durante il parsing della risposta:', error);
              return 'failed';  
            }
          }), 
          catchError(error => {
            console.error('Errore durante la verifica del login:', error);
            return of('failed');
          })
        ).toPromise() as Promise<any>;  
      }


      searchMeasures(Query: string, id: number) {
        this.apiUrl = "http://localhost:5052/api/Cocktails/SearchMeasureType/searchMeasure?id=" + id + "&measure=" + Query;
            const token = this.userService.getToken();
            const headers = new HttpHeaders({
              'Authorization': 'Bearer ' + token
            });
          
            return this.http.get<any>(this.apiUrl, { headers }).pipe(
              catchError(error => {
                console.error('Errore durante la ricerca degli ingredienti:', error);
                throw error; // Rilancia l'errore per gestirlo dove viene chiamato il metodo
              })
            ).toPromise().then(response => {
              console.log('Risposta dell\'API:', response);
              return response;
            });
          }
          


      searchIngredients(query: string, id: number) {
        const url = `http://localhost:5052/api/cocktails/IngredientSearch/SearchIngredient?id=${id}&ingredient=${query}`;
        const token = this.userService.getToken();
        const headers = new HttpHeaders({
          'Authorization': 'Bearer ' + token
        });
      
        return this.http.get<any>(url, { headers }).pipe(
          catchError(error => {
            console.error('Errore durante la ricerca degli ingredienti:', error);
            throw error; // Rilancia l'errore per gestirlo dove viene chiamato il metodo
          })
        ).toPromise().then(response => {
          console.log('Risposta dell\'API:', response);
          return response;
        });
      }
      
      
      
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
      getCreatedCocktailById(id: number): Promise<Cocktail[] | undefined> {
        const url = `http://localhost:5052/api/Cocktails/GetCocktailCreatorByUser/${id}`;

        return this.http.get<Cocktail[]>(url).pipe(
          map(response => (response ?? []) as Cocktail[]),
          catchError(error => {
            console.error('Errore durante la richiesta cocktail:', error);
            return of([] as Cocktail[]);
          })
        ).toPromise();
      }

    getSingleCocktailLikes(id:number)
    {
        this.apiUrl = "http://localhost:5052/api/Cocktails/GetCountCocktailLikes" + id;
    
        return this.http.get(this.apiUrl, { 
            responseType: 'text' 
        }).pipe(
            map(response => {
              console.log(response, " HERLO")
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
                    const backendError = error?.error || 'Errore sconosciuto';
                    console.error('Errore durante la verifica del like:', backendError);

                    return of([]); // In caso di errore, ritorna un array vuoto
                })
            ).toPromise() as Promise<Cocktail[]>; 
        }

        
      CreatedCocktailImg(file: File, id:number) {
      const headers = new HttpHeaders({
        'Authorization': 'Bearer ' + this.userService.getToken()
      });
      
      const formData = new FormData();
      formData.append('file', file);
      
      const url = "http://localhost:5052/api/Cocktails/" + id + "/UploadImageCocktail-local";
      
      try {
        return this.http.post(url, formData, { 
          headers: headers,
          responseType: 'json'
        }).pipe(
          map(response => {
            console.log('Upload response:', response);
            return response as { url: string };
          }),
          catchError(error => {
            console.error('Errore durante l\'upload dell\'immagine:', error);
            return of({ Message: 'failed' });
          })
        ).toPromise();
      }
      catch (error) {
        console.error("ERROR", error);
        return Promise.reject(error);
      }
    }
          deleteCocktail(id: number): Promise<string> {
              this.apiUrl = `http://localhost:5052/api/Cocktails/CocktailDelete/${id}`;
              const headers = new HttpHeaders({
                  'Authorization': 'Bearer ' + this.userService.getToken()
              });

              return this.http.delete<any>(this.apiUrl, { headers }).pipe(
                  map(response => {
                    console.log(response, " CIAO CIAO CIAO CIAO")
                      if (response) {
                          this.snackBar.open('Deleted', 'Close', { duration: 3000 });
                          return response;
                      }
                      this.snackBar.open('Error', 'Close', { duration: 3000 });
                      return 'failed';
                  }),
                  catchError(error => {
                      this.snackBar.open('Error', 'Close', { duration: 3000 });
                      return of('failed');
                  })
              ).toPromise() as Promise<string>;
          }


        
  }

    