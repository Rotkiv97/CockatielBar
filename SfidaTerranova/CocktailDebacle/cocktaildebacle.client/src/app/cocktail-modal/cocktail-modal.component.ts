// cocktail-modal.component.ts
import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-cocktail-modal',
  templateUrl: './cocktail-modal.component.html',
  styleUrls: ['./cocktail-modal.component.css'],
  imports: [CommonModule]
})
export class CocktailModalComponent implements OnInit {
  @Input() cocktail: any;
  @Output() close = new EventEmitter<void>();
  
  isLiked: boolean = false;
  likeCount: number = 0;
  
  constructor() { }
  
  ngOnInit(): void {
    // Inizializzazione del componente
    this.likeCount = Math.floor(Math.random() * 100); // Solo per esempio
  }
  
  onClose(): void {
    this.close.emit();
  }
  
  getIngredients(): { name: string, measure: string }[] {
    if (!this.cocktail) return [];
    
    console.log(this.cocktail, "IL CAZZO")
    const ingredients = [];
    for (let i = 1; i <= 15; i++) {
      const ingredient = this.cocktail[`strIngredient${i}`];
      const measure = this.cocktail[`strMeasure${i}`];
      
      if (ingredient) {
        ingredients.push({
          name: ingredient,
          measure: measure || ''
        });
      }
    }
    
    return ingredients;
  }
  
  handleLike(): void {
    this.isLiked = !this.isLiked;
    this.likeCount += this.isLiked ? 1 : -1;
  }
  
  handleShare(): void {
    // Implementa la logica di condivisione
    console.log('Sharing cocktail:', this.cocktail?.strDrink);
  }
}