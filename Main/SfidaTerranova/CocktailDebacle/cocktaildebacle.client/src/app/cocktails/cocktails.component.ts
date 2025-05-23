import { Component, OnInit, ViewEncapsulation} from '@angular/core';
import { UserService } from '../services/user.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CocktailService } from '../services/cocktail.service';
import { Subject } from 'rxjs';
import { ChangeDetectorRef } from '@angular/core';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CocktailModalComponent } from '../cocktail-modal/cocktail-modal.component';
import { Router, RouterModule } from '@angular/router';
import { SearchComponent } from '../search/search.component';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { random } from 'gsap';
import { TranslateHtmlService } from '../services/translate-html.service';

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
@Component({
  selector: 'app-cocktails',
  standalone: true,
  templateUrl: './cocktails.component.html',
  styleUrls: ['./cocktails.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [CommonModule, FormsModule, CocktailModalComponent, SearchComponent, MatIconModule, MatMenuModule]
})
export class CocktailsComponent implements OnInit {
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
  searchQuery: string = '';
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
  Alcoholic = false;
  notLoggedin = true;
  menuOpen = false;
  isMobile = false;
  currentUser:any;
  firstCocktailslist:Cocktail [] = [];
  secondCocktailslist:Cocktail [] = [];
  thirdCocktailslist:Cocktail [] = [];
  suggestion_data_1:any;
  suggestion_data_2:any;
  suggestion_data_3:any;
  currentIndex = 0;
  currentIndex_2 = 0;
  currentIndex_3 = 0;
  autoSlideInterval: any;
  suggestedTitle:string = ""
  suggestedTitle2: string = ""

  carouselItems = [
    {
      image: "",
      title: "Something Fresh?",
      description: "Gusto autentico, piacere essenziale.",
      id: -1
    },
    {
      image: "https://source.unsplash.com/1200x600/?cocktail,tropical",
      title: "Senza Impegni..",
      description: "Freschezza decisa, gusto da ricordare."
    },
    {
      image: "https://source.unsplash.com/1200x600/?cocktail,party",
      title: "Festa Locale",
      description: "Musica e colori tra le strade.",
      id: -1
    }
  ];

  secondaryCarousels = [
    {
      id: "tropical",
      items: [
        {
          image: "https://source.unsplash.com/1200x600/?cocktail,fruit",
          title: "Cucina Tropicale",
          description: "Sapori esotici da provare.",
          id: -1
        },
        {
          image: "https://source.unsplash.com/1200x600/?cocktail,sunset",
          title: "Tramonto sul mare",
          description: "Spettacolo naturale ogni sera.",
          id: Math.floor(Math.random() * 300) + 1
        }
      ]
    },
    {
      id: "boat",
      items: [
        {
          image: "https://source.unsplash.com/1200x600/?cocktail,boat",
          title: "Gita in Barca",
          description: "Scopri luoghi nascosti.",
          id: Math.floor(Math.random() * 300) + 1        },
        {
          image: "https://source.unsplash.com/1200x600/?cocktail,exotic",
          title: "Esotico",
          description: "Sapori da tutto il mondo.",
          id: Math.floor(Math.random() * 300) + 1        }
      ]
    }
  ];

  constructor(
    private userService: UserService,
    private modalService: NgbModal,
    private CocktailService: CocktailService,
    private cdRef: ChangeDetectorRef,
    private router: Router,
    private translateHtmlService: TranslateHtmlService,
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

  this.getTitleSuggestion();
  this.getSeasonalTitleSuggestion();
  this.getIfMobile();
  this.checkLoginStatus();
  this.getRandomSlides();
  this.initializeCarouselData();
  this.setupSearchDebouncer();
  this.populateCarousels();
  this.startAutoSlide();
}

  startAutoSlide() {
    this.autoSlideInterval = setInterval(() => {
      this.nexT(0, true); 
    }, Math.floor(Math.random() * (5000 - 3000 + 1)) + 3000
  );
  }
  resetAutoSlide() {
    clearInterval(this.autoSlideInterval);
    this.startAutoSlide();
  }
  
  ngOnDestroy() {
    clearInterval(this.autoSlideInterval);
  }

    getTitleSuggestion() {
      const now = new Date();
      const hour = now.getHours();

      if (hour < 13) {
        this.suggestedTitle = '☀️ Rise and Shine: Your Morning Mix Awaits!';
      } else if (hour < 18) {
        this.suggestedTitle = '🍹 Afternoon Delight: Sip into the Sunset';
      } else {
        this.suggestedTitle = '🌙 Night Vibes: Unwind with Our Evening Elixirs';
      }
    }
    getSeasonalTitleSuggestion() {
      const now = new Date();
      const month = now.getMonth() + 1;

      let season = '';

      if (month >= 3 && month <= 5) {
        season = 'spring';
      } else if (month >= 6 && month <= 8) {
        season = 'summer';
      } else if (month >= 9 && month <= 11) {
        season = 'autumn';
      } else {
        season = 'winter';
      }

        const titles:any = {
          spring: {
            light: [
              '🌸 Fresh Flavors for a New Season',
              '🌞 Wake Up Your Senses This Spring',
              '🍃 Bloom and Sip Brightly'
            ],
            dark: [
              '🌙 Twilight Blooms: Sip in the Shadows',
              '🖤 Night Petals and Cool Vibes',
              '🌌 After-Dusk Drinks with Spring Flair'
            ]
          },
          summer: {
            light: [
              '☀️ Chill Vibes and Citrus Highs',
              '🌴 Taste the Tropics',
              '🍉 Fresh, Cool, and Ready to Pour'
            ],
            dark: [
              '🌅 Sunset Sips for Hot Nights',
              '🌙 Midnight Mojitos and More',
              '🔥 Summer Heat, After Dark'
            ]
          },
          autumn: {
            light: [
              '🍁 Crisp Air, Smooth Flavors',
              '🎃 Spiced Sips for Cozy Moments',
              '☕ Golden Hours & Golden Drinks'
            ],
            dark: [
              '🌌 Fireside Flavors After Dusk',
              '🦉 Nocturnal Notes for Autumn Evenings',
              '🍂 Dark Leaves, Deep Sips'
            ]
          },
          winter: {
            light: [
              '❄️ Light Up Cold Days with Warm Notes',
              '☃️ Chilled Outside, Warm Inside',
              '🍊 Bright Flavors for Frosty Moods'
            ],
            dark: [
              '🌙 Deep Winter, Deep Cocktails',
              '🔥 Bold Spirits for Cold Nights',
              '🌌 Sip by the Firelight'
            ]
          }
        };

        const mode = this.isLightMode ? 'light' : 'dark';
        const seasonalTitles = titles[season][mode];
        const randomIndex = Math.floor(Math.random() * seasonalTitles.length);

        this.suggestedTitle2 = seasonalTitles[randomIndex];
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

  populateCarousels() {
    let user = this.userService.getUser();

    if (user === null || !user)
    {
      user = {
        id: -1,
        userName:"",
        name:"",
        lastName:"",
        email:"",
        PasswordHash:"",
      };
    }
  
    this.userService.fetchCustomSuggestions(user?.id as number, "likes").subscribe({
      next: (data) => {
        this.suggestion_data_1 = data;
  
        this.carouselItems = data.slice(0, 10).map((item: any) => ({
          image: item.strDrinkThumb || "https://source.unsplash.com/1200x600/?cocktail",
          title: item.strDrink || "",
          description: item.strGlass || "",
          id: item.id || "-1"
        }));
  
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Errore nel recupero dei suggerimenti:', err);
      },
      complete: () => {
      }
    });
  

    this.userService.fetchCustomSuggestions(user?.id as number, "search").subscribe({
      next: (data) => {
        this.suggestion_data_2 = data;
  
        this.secondaryCarousels = this.secondaryCarousels.map(carousel => {
          if (carousel.id === "tropical") {
            carousel.items = data.slice(0, 10).map((item: any) => ({
              image: item.strDrinkThumb || "https://source.unsplash.com/1200x600/?cocktail",
              title: item.strDrink || "",
              description: item.strGlass || "",
              id: item.id || "-1"
            }));
          }
          return carousel;
        });
  
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Errore nel recupero dei suggerimenti:', err);
      },
      complete: () => {
      }
    });
  
    this.userService.fetchCustomSuggestions(user?.id as number, "category").subscribe({
      next: (data) => {
        this.suggestion_data_3 = data;
  
        this.secondaryCarousels = this.secondaryCarousels.map(carousel => {
          if (carousel.id === "boat") {
            carousel.items = data.slice(0, 10).map((item: any) => ({
              image: item.strDrinkThumb || "https://source.unsplash.com/1200x600/?cocktail",
              title: item.strDrink || "",
              description: item.strGlass || "" , 
              id: item.id || "-1"
            }));
          }
          return carousel;
        });
  
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Errore nel recupero dei suggerimenti:', err);
      },
      complete: () => {
      }
    });
  }
  

  toggleMobileFilterOptions(): void {
    this.showMobileFilterOptions = !this.showMobileFilterOptions;
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

  private async checkLoginStatus() {
    const user = this.userService.getUser();
    if (user) {
      try {
        this.Token = await this.userService.isLoggedIn(user);
        this.currentUser = user;
        this.notLoggedin = false;
        console.log("Login status:", this.Token ? "Logged in" : "Not logged in");
      } catch (error) {
        console.error("Error checking login status:", error);
        this.Token = null;
      }
    } else {
      this.notLoggedin = true;
      this.Token = null;
    }
  }

  private getRandomSlides() {
    const randomNumber: number = Math.floor(Math.random() * 3) + 1;
    this.slide1 = `/assets/cocktails-bg-slide-${randomNumber}.png`;
    this.carouselItems[0].image = this.slide1;
    console.log("Random slide selected:", this.slide1);
  }

  private initializeCarouselData() {
  }


  closeFilterOptions() {
    this.showFilterOptions = false; 
  }
  toggleFilterButton() {
    this.showFilterButton = !this.showFilterButton;
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
  
  async openModal(id:any) {
      const response = await this.CocktailService.getCocktailById(id);
      this.selectedCocktail = JSON.parse(response);
  }

  handleLike(event: any, imageUrl: string) {
    event.stopPropagation();
    if (!this.currentInteractionImage) return;

    if (this.likedImages.has(this.currentInteractionImage)) {
      this.likedImages.delete(this.currentInteractionImage);
    } else {
      this.likedImages.add(this.currentInteractionImage);
    }

    console.log(`Image ${this.currentInteractionImage} liked:`, this.isLiked(this.currentInteractionImage));
  }

  handleShare(event: Event) {
    event.stopPropagation();
    if (!this.currentInteractionImage) return;

    this.sharedImages.add(this.currentInteractionImage);
    console.log(`Sharing image: ${this.currentInteractionImage}`);

    if (navigator.share) {
      navigator.share({
        title: 'Guarda questo cocktail!',
        text: 'Ho trovato questo fantastico cocktail su Mixology Bar',
        url: this.currentInteractionImage
      }).catch(err => {
        console.error('Error sharing:', err);
      });
    } else {
      console.log('Web Share API not supported, implement fallback');
    }
  }

  isLiked(imageUrl: string): boolean {
    return this.likedImages.has(imageUrl);
  }

  toggleTheme() {
    this.isLightMode = !this.isLightMode;
  
    const bgOverlay = document.getElementById('bg-overlay');
    const root = document.documentElement;
  
    const accentDark = '#01786F';
  
    if (!this.isLightMode) {
      bgOverlay?.classList.add('visible');
      document.body.style.backgroundSize = "cover";
      document.body.style.backgroundRepeat = "no-repeat";
      document.body.style.backgroundPosition = "center";
  
      document.querySelector('.container')?.classList.add('low-alpha');
      document.querySelectorAll('.menu-item').forEach((element) => {
        element.classList.add('low-alpha');
      });
  

      const searchButton = document.querySelector('.sidebar .search button') as HTMLElement | null;
      if (searchButton) {
        searchButton.classList.add('dark-theme');
      }
  
      document.querySelectorAll('.menu-item i').forEach(icon => {
        (icon as HTMLElement).style.color = accentDark;
      });
      document.querySelectorAll('.menu-item').forEach(icon => {
        (icon as HTMLElement).style.color = accentDark;
      });
      document.querySelectorAll('.icon-sidebar .menu-item').forEach(icon => {
        (icon as HTMLElement).style.color = accentDark;
      });
  
      const profilePic = document.querySelector('.profile-pic') as HTMLElement;
      if (profilePic) {
        profilePic.classList.add('dark-theme');
        profilePic.style.borderColor = accentDark;
        profilePic.style.filter = 'brightness(0.8)';
      }
  
    } else {
      bgOverlay?.classList.remove('visible');
      document.querySelector('.container')?.classList.remove('low-alpha');
      root.style.setProperty('--darkest', '#02a697');
      document.body.style.backgroundImage = "";
  
      document.querySelector('.container')?.classList.remove('low-alpha');
  

      const searchButton = document.querySelector('.sidebar .search button') as HTMLElement | null;
      if (searchButton) {
        searchButton.classList.remove('dark-theme');
      }
  
      document.querySelectorAll('.menu-item i').forEach(icon => {
        (icon as HTMLElement).style.color = '';
      });
      document.querySelectorAll('.menu-item').forEach(icon => {
        (icon as HTMLElement).style.color = '';
      });
  
      const profilePic = document.querySelector('.profile-pic') as HTMLElement;
      if (profilePic) {
        profilePic.classList.remove('dark-theme');
        profilePic.style.borderColor = '';
        profilePic.style.filter = '';
      }
    }
    this.getSeasonalTitleSuggestion()
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

  



  isSaved(image: string): boolean {
    return false; 
  }
  toggleSearch() {
    const searchBar = document.querySelector('.icon-sidebar .search');
    if (searchBar) {
      (searchBar as HTMLElement).style.display = (searchBar as HTMLElement).style.display === 'block' ? 'none' : 'block';
    }
  }

  
  toggleMenu() {
    this.router.navigate([`/profile-page/`]);
  }

  getIfMobile()
  {
    let screenSize = this.getScreenSize()
    if (screenSize.width < 600)
    {
      this.isMobile = true
    }
    else if (screenSize.width > 600)
    {
      this.isMobile = false
    }
  }

  getScreenSize(): { width: number; height: number } {
    return {
      width: window.innerWidth,
      height: window.innerHeight,
    };
  }

  nexT(id:number, auto = false ) {
    switch (id)
    {
      case (1):
      {
        this.currentIndex  = (this.currentIndex + 1) % this.carouselItems.length; 
        break;
      }
      case (2):
      {
        this.currentIndex_2 = (this.currentIndex_2 + 1) % this.carouselItems.length; 
        break;
      }
      case (3):
      {
        this.currentIndex_3 = (this.currentIndex_3 + 1) % this.carouselItems.length; 
        break;
      }
      default:
      {
        this.currentIndex = (this.currentIndex + 1) % this.carouselItems.length; 
        break;
      }
    }
    setTimeout(() => {
      this.currentIndex_2 = (this.currentIndex_2 + 1) % this.carouselItems.length; 
      
    },  Math.floor(Math.random() * (5000 - 3000 + 1)) + 3000);
    setTimeout(() => {
      this.currentIndex_3 = (this.currentIndex_3 + 1) % this.carouselItems.length; 
      
    },  Math.floor(Math.random() * (5000 - 3000 + 1)) + 3000);
    if (!auto) this.resetAutoSlide();
  }
  
  bacK(id:number) {
    switch (id)
    {
      case (1):
      {
        this.currentIndex = (this.currentIndex - 1 + this.carouselItems.length) % this.carouselItems.length;
        break;
      }
      case (2):
      {
        this.currentIndex_2 = (this.currentIndex_2 - 1 + this.carouselItems.length) % this.carouselItems.length;
        break;
      }
      case (3):
      {
        this.currentIndex_3 = (this.currentIndex_3 - 1 + this.carouselItems.length) % this.carouselItems.length;
        break;
      }
      default:
      {
        this.currentIndex = (this.currentIndex + 1) % this.carouselItems.length; 
        break;
      }
    }
    this.resetAutoSlide();
  }
}

