import { Component, ViewEncapsulation, OnInit, Input  } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';
import { CocktailService } from '../services/cocktail.service';
import { Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CocktailModalComponent } from '../cocktail-modal/cocktail-modal.component';

@Component({
  selector: 'app-search',
  imports: [
    FormsModule,
    CommonModule,
    CocktailModalComponent,
  ],
  encapsulation: ViewEncapsulation.None,
  templateUrl: './search.component.html',
  styleUrl: './search.component.css',
})


export class SearchComponent  implements OnInit {

  @Input() showArrow = true;
  @Input() showSideBarIcon = true;

  searchQuery: string = '';
  totalLikes: number = 0;
  currentCaption: string = '';
  currentDate: Date = new Date();
  currentModalImage: string = '';
  likedImages: Set<string> = new Set();
  sharedImages: Set<string> = new Set();
  currentInteractionImage: string = "";
  Token: string | null = null;
  slide1: string = "";
  isLightMode: boolean = true;
  suggestions: any[] = [];
  showSuggestions: boolean = false;
  private searchTerms = new Subject<string>();
  private latestSearch:any;
  selectedCocktail: any = null;
  showFilterOptions = false;
  filter:string = "nameCocktail=";
  showFilterButton = false; 
  selectedFilter: string = "nameCocktail=";
  showMobileFilterOptions: boolean = false;
  isClosing = false; 
  isLoggedIn = false;
  Alcoholic = true;
  
  constructor(
    private userService: UserService,
    private CocktailService: CocktailService,
    
    private router: Router,
    private cdRef: ChangeDetectorRef,
    ) {}

    ngOnInit() {
      const user = this.userService.getUser();
      this.isLoggedIn = user ? true : false
      this.setupSearchDebouncer();
    }
      private setupSearchDebouncer() {
        this.searchTerms.pipe(
          debounceTime(300), //
          distinctUntilChanged() 
        ).subscribe(term => {
          if (term.length > 0) {
            this.fetchSuggestions(term);
          } else {
            this.suggestions = [];
          }
        });
      }
    async selectSuggestion(suggestion: any) {  
    
      switch (this.selectedFilter)
      {
        case ("nameCocktail="):
        {
          this.searchQuery = suggestion.strDrink;
          this.latestSearch = suggestion.id;
          break;
        }
        case ("UserSearch="):
        {
          this.searchQuery = suggestion.userName;
          this.latestSearch = suggestion.userName;
          break;
        }
        case ("ingredient="):
        {
          this.searchQuery = suggestion.strDrink;
          this.latestSearch = suggestion.id;
          break;

        }
        default:
        {
          break;
        }
  
      }
      if (this.latestSearch !== 0)
        {
          try {
            if(this.selectedFilter === "nameCocktail=" || this.selectedFilter == "ingredient=")
            {
              const response = await this.CocktailService.getCocktailById(this.latestSearch);
              this.selectedCocktail = JSON.parse(response);
            }
            else if (this.selectedFilter === 'UserSearch=')
            {
              this.userService.getUserByUsername(this.latestSearch).subscribe(
                profile => {
                  window.location.href = `/profile-page/${profile.userName}`;
                },
                error => {
                  console.error('Errore nel recupero dei dati dell\'utente:', error);
                }
              );
 
            }
          } catch (err) {
            console.error("Errore nel parsing del cocktail:", err);
          }
        }
      this.showSuggestions = false;
    }

    fetchSuggestions(query: string) {
      this.CocktailService.searchCocktails(query, this.filter, this.Alcoholic).then((result: any) => {
        const parsed = JSON.parse(result);
        try {
          if (this.selectedFilter === "nameCocktail=" || this.selectedFilter == "ingredient=")
          {
            this.suggestions = Array.isArray(parsed?.cocktails) ? parsed?.cocktails : [];
          }
          else if (this.selectedFilter === "UserSearch=")
          {
            this.suggestions = Array.isArray(parsed?.users) ? parsed?.users : [];
          }
          this.cdRef.detectChanges();
        } catch (e) {
          console.error('Error processing suggestions:', e);
          this.suggestions = [];
        }
      });
  }


  async onKeyUp(event: KeyboardEvent) {
    if (event.key === 'Enter')
      {
        if (this.selectedFilter === "nameCocktail=" || this.selectedFilter === "ingredient=")
        {
          this.searchQuery = this.suggestions[0].strDrink;
          this.latestSearch = this.suggestions[0].id;
        }
        else if (this.selectedFilter === "UserSearch=")
        {
          this.searchQuery,this.latestSearch = this.suggestions[0].userName
        }
        
        if (this.latestSearch !== 0 && (this.selectedFilter === "nameCocktail=" || this.selectedFilter === "ingredient="))
        {
          try {
            const response = await this.CocktailService.getCocktailById(this.latestSearch);
            this.selectedCocktail = JSON.parse(response);
          } catch (err) {
            console.error("Errore nel parsing del cocktail:", err);
          }
        }
        else if (this.selectedFilter === 'UserSearch=')
        {
          this.userService.getUserByUsername(this.latestSearch).subscribe(
            profile => {
              this.router.navigate([`/profile-page/${profile.userName}`]);
            },
            error => {
              console.error('Errore nel recupero dei dati dell\'utente:', error);
            }
          );
        }
        return;
      }
    
    this.searchTerms.next(this.searchQuery);
    this.showSuggestions = true;
  }

  async handleSearch() {
    this.showSuggestions = false;
    if (this.selectedFilter === "nameCocktail=" || this.selectedFilter === "ingredient=")
      {
        this.searchQuery = this.suggestions[0].strDrink;
        this.latestSearch = this.suggestions[0].id;
      }
      else if (this.selectedFilter === "UserSearch=")
      {
        this.searchQuery,this.latestSearch = this.suggestions[0].userName;
      }
    if (this.searchQuery.trim() && this.latestSearch !== 0) {
      try {
        if (this.selectedFilter === "nameCocktail=" || this.selectedFilter == "ingredient=")
        {
          const response = await this.CocktailService.getCocktailById(this.latestSearch);
          this.selectedCocktail = JSON.parse(response);
        }
        else if (this.selectedFilter === 'UserSearch=')
          {
            this.userService.getUserByUsername(this.latestSearch).subscribe(
              profile => {
                this.router.navigate([`/profile-page/${profile.userName}`]);
              },
              error => {
                console.error('Errore nel recupero dei dati dell\'utente:', error);
              }
            );
          }
      } catch (err) {
        console.error("Errore nel parsing del cocktail:", err);
      }
    }
  }

  toggleFilterOptions() {
    this.showFilterOptions = !this.showFilterOptions;
  }

  applyFilter(option: string) {
    if (option !== "nonAlcoholic")
    {
      this.selectedFilter = option;
      this.Alcoholic = true;
    }
    else
    {
      this.Alcoholic = false
    }
    this.searchQuery = "";
    this.suggestions = [];
    this.filter = option;
    this.startClosingAnimation();
  }

  startClosingAnimation() {
    this.isClosing = true;

    setTimeout(() => {
      this.showMobileFilterOptions = false;
      this.showFilterOptions = false;
      this.isClosing = false;
    }, 300); 
  }

  closeFilterOptions() {
    this.showFilterOptions = false; 
  }

  isDesktopScreen(): boolean {
    return window.innerWidth <= 991;
  }
  

}
