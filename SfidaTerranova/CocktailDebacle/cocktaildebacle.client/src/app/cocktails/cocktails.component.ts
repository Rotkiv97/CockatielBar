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


@Component({
  selector: 'app-cocktails',
  standalone: true,
  templateUrl: './cocktails.component.html',
  styleUrls: ['./cocktails.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [CommonModule, FormsModule, CocktailModalComponent]
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
  isLightMode: boolean = false;
  searchQuery: string = '';
  suggestions: any[] = [];
  showSuggestions: boolean = false;
  private searchTerms = new Subject<string>();
  private latestSearch: number = 0;
  selectedCocktail: any = null;


  carouselItems = [
    {
      image: "",
      title: "Something Fresh?",
      description: "Gusto autentico, piacere essenziale."
    },
    {
      image: "https://source.unsplash.com/1200x600/?cocktail,tropical",
      title: "Senza Impegni..",
      description: "Freschezza decisa, gusto da ricordare."
    },
    {
      image: "https://source.unsplash.com/1200x600/?cocktail,party",
      title: "Festa Locale",
      description: "Musica e colori tra le strade."
    }
  ];

  secondaryCarousels = [
    {
      id: "tropical",
      items: [
        {
          image: "https://source.unsplash.com/1200x600/?cocktail,fruit",
          title: "Cucina Tropicale",
          description: "Sapori esotici da provare."
        },
        {
          image: "https://source.unsplash.com/1200x600/?cocktail,sunset",
          title: "Tramonto sul mare",
          description: "Spettacolo naturale ogni sera."
        }
      ]
    },
    {
      id: "boat",
      items: [
        {
          image: "https://source.unsplash.com/1200x600/?cocktail,boat",
          title: "Gita in Barca",
          description: "Scopri luoghi nascosti."
        },
        {
          image: "https://source.unsplash.com/1200x600/?cocktail,exotic",
          title: "Esotico",
          description: "Sapori da tutto il mondo."
        }
      ]
    }
  ];

  constructor(
    private userService: UserService,
    private modalService: NgbModal,
    private CocktailService: CocktailService,
    private cdRef: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.checkLoginStatus();
    this.getRandomSlides();
    this.initializeCarouselData();
    this.setupSearchDebouncer();
  }

  private setupSearchDebouncer() {
    this.searchTerms.pipe(
      debounceTime(300), // Aspetta 300ms dopo l'ultima digitazione
      distinctUntilChanged() // Ignora se il testo non è cambiato
    ).subscribe(term => {
      if (term.length > 0) { // Mostra suggerimenti solo dopo 3 caratteri
        this.fetchSuggestions(term);
      } else {
        this.suggestions = [];
      }
    });
  }

  onKeyUp(event: KeyboardEvent) {
    if (event.key === 'Enter')
      {
        if (this.latestSearch !== 0)
        {
          this.CocktailService.getCocktailById(this.latestSearch);
        }
        return;
      }
    
    this.searchTerms.next(this.searchQuery);
    this.showSuggestions = true;
  }
  
  fetchSuggestions(query: string) {
    this.CocktailService.searchCocktails(query).then((result: any) => {
      const parsed = JSON.parse(result);
  
      try {
        this.suggestions = Array.isArray(parsed?.cocktails) ? parsed?.cocktails : [];
        this.cdRef.detectChanges();
      } catch (e) {
        console.error('Error processing suggestions:', e);
        this.suggestions = [];
      }
    });
  }
  

  selectSuggestion(suggestion: any) {  
    console.log(suggestion, " NEGRONE")
    this.searchQuery = suggestion.strDrink;
    this.latestSearch = suggestion.id;
    this.showSuggestions = false;
  }

  private async checkLoginStatus() {
    const user = this.userService.getUser();
    if (user) {
      try {
        this.Token = await this.userService.isLoggedIn(user);
        console.log("Login status:", this.Token ? "Logged in" : "Not logged in");
      } catch (error) {
        console.error("Error checking login status:", error);
        this.Token = null;
      }
    } else {
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
    // Inizializzazioni future o caricamenti API
  }

  openModal(content: any, imageUrl: string) {
    this.currentModalImage = imageUrl;
    this.currentInteractionImage = imageUrl;

    const modalRef = this.modalService.open(content, {
      size: 'lg',
      centered: true,
      windowClass: 'dark-modal'
    });

    modalRef.result.finally(() => {
      this.currentInteractionImage = "";
    });
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
    
    document.documentElement.style.setProperty(
      '--primary-color',
      this.isLightMode ? '#ff6b6b' : '#00a896'
    );
    document.documentElement.style.setProperty(
      '--secondary-color',
      this.isLightMode ? '#ff8e8e' : '#00c9a7'
    );
    document.documentElement.style.setProperty(
      '--text-color',
      this.isLightMode ? '#212529' : '#212529'
    );

  }

  async handleSearch() {
    this.showSuggestions = false;
    if (this.searchQuery.trim() && this.latestSearch !== 0) {
      try {
        const response = await this.CocktailService.getCocktailById(this.latestSearch);
        this.selectedCocktail = JSON.parse(response);
      } catch (err) {
        console.error("Errore nel parsing del cocktail:", err);
      }
    }
  }
  
  
  


  likePost() {
    console.log("Post liked!");
  }

  sharePost() {
    console.log("Post shared!");
  }

  getLikeCount(item: { image: string }): number {
    return this.isLiked(item.image) ? 1 : 0;
  }

  openComments() {
    console.log('openComments');
  }

  handleSave(event: Event) {
    console.log('handleSave', event);
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
  
}

