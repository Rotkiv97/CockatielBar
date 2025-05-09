// cocktail-modal.component.ts
import { Component, Input, Output, EventEmitter, OnInit,SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CocktailService } from '../services/cocktail.service';
import { User, UserService } from '../services/user.service';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProfilePageComponent } from '../profile-page/profile-page.component';
import { MatDialogModule } from '@angular/material/dialog';
import { ChangeDetectorRef } from '@angular/core';
import { LikeEventService } from '../services/like-event.service';

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
  Likes: number;
  isLiked: boolean;
}
@Component({
  selector: 'app-cocktail-modal-create',
  templateUrl: './cocktail-modal-create.component.html',
  styleUrls: ['./cocktail-modal-create.component.css'],
  imports: [CommonModule,MatDialogModule],
  standalone: true
  
})


export class CocktailModalComponentCreate implements OnInit {
  @Input() cocktail: any;
  @Output() close = new EventEmitter<void>();
  @Output() liked = new EventEmitter<void>();
  @Output() testEvent = new EventEmitter<void>();

  CocktailIcon:string = "";
  isLoggedin: boolean = false;
  isLiked: boolean = false;
  likeCount: number = 0;
  currentInteractionImage: string = "";
  sharedImages: Set<string> = new Set();
  modalData: Cocktail = {
    id: '',
    idDrink: '',
    ingredients: [],
    measures: [],
    strAlcoholic: '',
    strCategory: '',
    strDrink: '',
    strDrinkThumb: '',
    strGlass: '',
    strInstructions: '',
    Likes:0,
    isLiked:false,
  };
  
  constructor(
    private cdr: ChangeDetectorRef,
    private CocktailService: CocktailService,
    private UserService: UserService,
    private snackBar:MatSnackBar,
    private likeEventService: LikeEventService
  ) { }
  
   async ngOnInit(): Promise<void> {
    this.getRandomIcon()
    await this.getIfLiked()
    if (this.UserService.getUser())
    {
      this.isLoggedin = true;
    }
    this.likeCount = this.cocktail.likes;
    this.fetchModalInfo();
    this.currentInteractionImage = this.cocktail.strDrinkThumb
    
  
  }

  async fetchModalInfo() {
    try {
      const culo = await this.CocktailService.isLiked(this.cocktail.id);
    } catch (error) {
      console.error('Errore nella richiesta:', error);
    }
    if (this.cocktail) {
      this.modalData = { ...this.cocktail };
    } else {
      console.error('Cocktail data is not available.');
    }
  }
  onClose(): void {
    this.close.emit();
  }
  
  getIngredients(): { name: string, measure: string }[] {
    if (!this.cocktail) return [];
    
    const ingredients = [];
    for (let i = 0; i <= this.cocktail.ingredients.length; i++) {
      const ingredient = this.cocktail.ingredients[i];
      const measure = this.cocktail.measures[i];
      if (ingredient) {
        ingredients.push({
          name: ingredient,
          measure: measure || ''
        });
      }
    }
    
    return ingredients;
  }

  splitByDot(text: string): string[] {
    return text.split('.').map(s => s.trim()).filter(s => s.length > 0);
  }
  
  async handleLike(): Promise<void> {
    if (this.isLoggedin) {
      if (this.isLiked) {
        this.likeCount -= 1;
      } else {
        this.likeCount += 1;
      }
  
      this.isLiked = !this.isLiked;
  
      try {
        await this.CocktailService.likeCocktail(this.cocktail.id);
        this.likeEventService.emitLikeEvent();
      } catch (error) {
        console.error("Errore durante il like del cocktail:", error);
        this.snackBar.open('Errore durante il like del cocktail', 'OK', { duration: 3000 });
      }
    } else {
      this.snackBar.open('Devi essere loggato per mettere like', 'OK', { duration: 3000 });
      this.onClose();
    }
    console.log('Modal: like completato, evento emesso');
  }
  


  
  
  
  
  handleShare(event: Event) {
    event.stopPropagation();
    if (!this.modalData) return;
  
    const cocktailUrl = `${window.location.origin}/cocktail/${this.modalData.id}`;
  
    if (navigator.share) {
      navigator.share({
        title: `Guarda questo cocktail: ${this.modalData.strDrink}`,
        text: 'Scopri il cocktail!',
        url: cocktailUrl  
      }).catch(err => {
        console.error('Error sharing:', err);
      });
    } else {
      console.log('Web Share API non supportata');
    }
  }

  getRandomIcon()
  {
    const randomNumber: number = Math.floor(Math.random() * 5) + 1;
    this.CocktailIcon = "/assets/cocktail_modal_icon_1.png";
  }
  
  getInstructionSteps(): string[] {
    if (!this.cocktail?.strInstructions) return [];
  
    return this.cocktail.strInstructions
      .split('.')  
      .map((s: string) => s.trim())  
      .filter((s: string) => s.length > 0) 
      .map((s: string) => `${s}`);  
  }

  
  async getIfLiked() {
    const liked = await this.CocktailService.isLiked(this.cocktail.id);
    this.isLiked = liked;
    this.cocktail.like = liked;

  }
  
  
  
  
  
  
}
  