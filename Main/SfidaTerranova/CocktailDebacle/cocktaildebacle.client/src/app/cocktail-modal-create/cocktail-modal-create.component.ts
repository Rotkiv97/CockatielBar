
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
import { FormsModule } from '@angular/forms';
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
  Likes: number;
  isLiked: boolean;
}
@Component({
  selector: 'app-cocktail-modal-create',
  templateUrl: './cocktail-modal-create.component.html',
  styleUrls: ['./cocktail-modal-create.component.css'],
  imports: [CommonModule,MatDialogModule,  FormsModule],
  standalone: true
  
})


export class CocktailModalComponentCreate implements OnInit {
  @Input() cocktail: any;
  @Output() close = new EventEmitter<void>();
  @Output() liked = new EventEmitter<void>();
  @Output() created = new EventEmitter<void>();
  @Input() userId!: number;

  CocktailIcon: string = "";
  isLoggedin: boolean = false;
  isLiked: boolean = false;
  likeCount: number = 0;
  currentInteractionImage: string = "";
  sharedImages: Set<string> = new Set();

  isPublic:boolean = false;
  categoryResults: string[] = [];
  categoryQuery: string = '';
  cocktailTitle: string = "";
  cocktailCat: string = "";
  cocktailGlass: string = "";
  instructions: string = "";
  ingredients: { name: string, measure: string }[] = [];
  newInstructions = '';
  ingredientQuery: string = '';
  measureQuery: string = '';
  selectedIngredients: string[] = [];
  selectedQuantities: string[] = [];  
  selectedMeasures: string[] = [];    
  measureResults: string[] = ["ml", "oz", "cl", "cup", "tsp", "tbsp", "dash", "shot", "part"]; 
  ingredientResults: string[] = [];
  isAddingDetails: boolean = false;
  pendingIngredientIndex: number = -1;
  showTitleInput: boolean = false;
  showCategoryInput: boolean = false;
  showInstructionsForm = false;
  cocktailImg:string = ""

  tempTitle = '';
  quantities: string[] = [
  '1/4', '1/3', '1/2', '2/3', '3/4',
  '1', '1.25', '1.5', '1.75',
  '2', '2.5', '3', '3.5', '4', '4.5',
  '5', '5.5', '6', '6.5', '7',
  '7.5', '8', '8.5', '9', '9.5', '10'
];
  glass = new Map<string, number>([
    ["Highball glass", 350],
    ["Cocktail glass", 150],
    ["Old-fashioned glass", 300],
    ["Collins glass", 400],
    ["Margarita glass", 300],
    ["Pint glass", 500],
    ["Shot glass", 50],
    ["Whiskey sour glass", 250],
    ["Hurricane glass", 400],
    ["Champagne flute", 200],
    ["Beer mug", 500],
    ["Brandy snifter", 300],
    ["Cordial glass", 100],
    ["Copper mug", 350],
    ["Irish coffee cup", 300]
  ]);
  isSaveDisabled: boolean = false;
  showGlassList: boolean = false;
  isExceededCapacity: boolean = false;

  glassCapacity: number = 0;
  createdCoktail:Cocktail = 
  {
    id: "0",
    idDrink: "0",
    ingredients: ["string"],
    measures: ["string"],
    strAlcoholic: "string",
    strCategory: "string",
    strDrink: "string",
    strDrinkThumb: "string",
    strGlass: "string",
    strInstructions: "string",
    Likes: 0,
    isLiked: false

  }


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
    private likeEventService: LikeEventService,
    private translateHtmlService: TranslateHtmlService,
  ) { }
  
async ngOnInit(): Promise<void> {
  const savedLanguage = localStorage.getItem('preferredLanguage');
  console.log('Saved Language:', savedLanguage);

  if (savedLanguage) {
    console.log("Language found in localStorage:", savedLanguage);
    this.translateHtmlService.translateElements(savedLanguage);
  } else {
    console.log("No language found, setting to default");
    this.translateHtmlService.translateElements('en'); 
  }


  if (this.cocktailGlass && this.glass.has(this.cocktailGlass)) {
    this.glassCapacity = this.glass.get(this.cocktailGlass) || 0;
  }
  this.getRandomIcon();
  this.ingredients = this.getIngredients() || [];
  this.checkSaveButtonState();
}

  

  onCategoryInput(): void {
    const trimmed = this.categoryQuery.trim();
    if (!trimmed) {
      this.categoryResults = [];
      return;
    }
  
    const user = this.UserService.getUser();
  
    this.CocktailService.searchCat(trimmed, user?.id as number).then((res) => {
      console.log('Risultato della ricerca categoria:', res);
  
      if (res && res.categoryTypes && Array.isArray(res.categoryTypes)) {
        this.categoryResults = res.categoryTypes.sort();
      } else {
        console.error('Errore: categoryTypes non è un array o non esiste', res);
        this.categoryResults = [];
      }
    }).catch((err) => {
      console.error('Errore nella ricerca categoria:', err);
      this.categoryResults = [];
    });
  }
  
toggleGlassList() {
  this.showGlassList = !this.showGlassList;
}

selectGlass(glassName: string) {
  this.cocktailGlass = glassName;
  this.glassCapacity = this.glass.get(glassName) || 0;
  this.showGlassList = false;
  this.calculateTotalQuantity(); 
  this.checkSaveButtonState()
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
calculateTotalQuantity() {
  let totalQuantityMl = 0;
  
  for (let i = 0; i < this.selectedQuantities.length; i++) {
    const quantity = parseFloat(this.selectedQuantities[i]) || 0;
    const measure = this.selectedMeasures[i];
    
    let quantityInMl = 0;
    
    switch(measure) {
      case 'oz':
        quantityInMl = quantity * 29.57; 
        break;
      case 'cl':
        quantityInMl = quantity * 10; 
        break;
      case 'ml':
        quantityInMl = quantity; 
        break;
      case 'dash':
        quantityInMl = quantity * 0.92;
        break;
      case 'splash':
        quantityInMl = quantity * 5; 
        break;
      case 'tbsp':
        quantityInMl = quantity * 15; 
        break;
      case 'tsp':
        quantityInMl = quantity * 5; 
        break;
      default:
        quantityInMl = quantity; 
    }
    
    totalQuantityMl += quantityInMl;
  }

  this.isExceededCapacity = totalQuantityMl > this.glassCapacity;
  
  if (this.isExceededCapacity) {
    console.warn(`Attenzione: la quantità totale (${totalQuantityMl} ml) supera la capacità del bicchiere (${this.glassCapacity} ml)`);
  }
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
  


  addInstructions()
  {
    this.showInstructionsForm = true;

  }
  saveInstructions() {
    this.instructions = this.newInstructions.trim();
    this.showInstructionsForm = false;
    this.checkSaveButtonState()
  }

  addCat(query: string): void {
    const trimmedQuery = query.trim();
    const user = this.UserService.getUser();
    
    if (trimmedQuery) {
      this.CocktailService.searchCat(trimmedQuery, user?.id as number).then((res: any) => {
        if (res ) {
          this.cocktailCat = res.categoryTypes[0];
          this.showCategoryInput = false;
          this.categoryResults = [];
  ;
        } else {
          this.cocktailCat = '';
          this.showCategoryInput = false;
        }
        this.showCategoryInput = false;
      }).catch((err) => {
        this.cocktailCat = '';
        this.showCategoryInput = false;
      });
    } else {
      this.cocktailCat = '';
      this.showCategoryInput = false;
    }
    this.checkSaveButtonState()
  }
  
  selectCategory(cat: string): void {
    this.cocktailCat = cat;
    this.categoryQuery = cat;
    this.showCategoryInput = false;
    this.categoryResults = [];
    this.checkSaveButtonState()
  }
  
  addTitle()
  {
    this.showTitleInput = true;
    this.tempTitle = this.cocktailTitle;
    this.checkSaveButtonState()
  }
  saveTitle(newTitle: string) {
    this.cocktailTitle = newTitle.trim();
    this.showTitleInput = false;
    this.checkSaveButtonState()
  }
  cancelTitleEdit() {
  this.showTitleInput = false;
}
saveCocktail() {

  if (this.isSaveDisabled) return;


  const defaultimg = "cocktails-bg-slide-2.png"
  const user = this.UserService.getUser();
  let new_Cocktail: {
    UserIdCocktail: number
    PublicCocktail: boolean;
    StrDrink: string;
    StrCategory: string;
    StrAlcoholic: string;
    StrGlass: string;
    StrInstructions: string;
    [key: string]: string | boolean | number | undefined;
  } = {
    UserIdCocktail: user?.id || -1,
    PublicCocktail: Boolean(this.isPublic), 
    StrDrink: this.cocktailTitle,
    StrCategory: this.cocktailCat,
    StrAlcoholic: "",
    StrGlass: this.cocktailGlass,
    StrInstructions: this.instructions,
    StrDrinkThumb: String(this.cocktailImg) || "/assets/cocktails-bg-slide-3.png"
  };

  for (let i = 0; i < 15; i++) {
    const ingredient = this.selectedIngredients[i] || "";
    const quantity = this.selectedQuantities[i] || "";
    const measure = this.selectedMeasures[i] || "";

    new_Cocktail[`StrIngredient${i + 1}`] = ingredient;
    new_Cocktail[`StrMeasure${i + 1}`] = quantity + (measure ? ` ${measure}` : "");
  }
  this.CocktailService.createCocktail(new_Cocktail);
  setTimeout(() => {
    this.created.emit()
    this.onClose()
  }, 1000);
}


  
checkSaveButtonState() {
  this.isSaveDisabled = !(
    this.isPublic !== null &&
    this.cocktailTitle &&
    this.cocktailCat &&
    this.cocktailGlass &&
    this.instructions &&
    this.selectedIngredients.length > 0
  );
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
  const source = this.instructions?.trim() || this.cocktail?.strInstructions?.trim();
  if (!source) return [];

  return source
    .split('.')
    .map((s: string) => s.trim())
    .filter((s: string) => s.length > 0);
}


getIngredients(): { name: string, measure: string }[] {
  if (!this.ingredients || this.ingredients.length === 0) {
    return [];
  }

  const ingredientList = [];
  
  for (const ingredient of this.ingredients) {
    ingredientList.push({
      name: ingredient.name,
      measure: ingredient.measure || '' 
    });
  }

  return ingredientList;
}
  
onIngredientInput(): void {
  const trimmed = this.ingredientQuery.trim();
  if (!trimmed || this.selectedIngredients.length >= 15) {
    this.ingredientResults = [];
    return;
  }

  const user = this.UserService.getUser();

  this.CocktailService.searchIngredients(trimmed, user?.id as number).then((res: any) => {
    if (res === 'failed') {
      this.ingredientResults = [];
      return;
    }

    if (typeof res === 'string') {
      try {
        const parsed = JSON.parse(res);
        this.ingredientResults = parsed.ingredients || [];
      } catch (e) {
        console.error('Errore nel parsing JSON:', e);
        this.ingredientResults = [];
      }
    } else {
      this.ingredientResults = res.ingredients || [];
    }
  }).catch((err) => {
    console.error('Errore nella ricerca ingredienti:', err);
    this.ingredientResults = [];
  });
}

addIngredient(ingredient: string): void {
  if (this.selectedIngredients.length < 15) {
    this.selectedIngredients.push(ingredient);
    this.selectedQuantities.push(''); 
    this.selectedMeasures.push('');   
      this.ingredientResults = [];
  }
  this.calculateTotalQuantity(); 
  this.checkSaveButtonState()
}

addDetails(index: number): void {
  this.pendingIngredientIndex = index; 
  this.isAddingDetails = true;         
}
saveIngredientDetails(): void {
  if (this.selectedQuantities[this.pendingIngredientIndex] && this.selectedMeasures[this.pendingIngredientIndex]) {
    this.isAddingDetails = false; 
    this.pendingIngredientIndex = -1;
  } else {
    alert('Please enter both quantity and measure');
  }
    this.calculateTotalQuantity();
  this.checkSaveButtonState()
}
  
onMeasureInput(): void {
  const trimmed = this.measureQuery.trim();
  if (!trimmed || this.selectedIngredients.length === 0) {
    this.measureResults = [];
    return;
  }

  const user = this.UserService.getUser();

  this.CocktailService.searchMeasures(trimmed, user?.id as number).then((res: any) => {
    this.measureResults = res.measureTypes || [];
  }).catch((err) => {
    console.error('Errore nella ricerca misure:', err);
    this.measureResults = [];
  });
}
  
addMeasure(measure: string): void {
  if (this.selectedIngredients.length === 0) return;

  this.selectedMeasures.push(measure);
  this.measureQuery = '';
  this.measureResults = [];
}

removeIngredient(index: number): void {
  this.selectedIngredients.splice(index, 1);
  this.selectedQuantities.splice(index, 1);
  this.selectedMeasures.splice(index, 1);
  this.calculateTotalQuantity()
  this.checkSaveButtonState()
}
cancelIngredientDetails() {
this.isAddingDetails = false;
this.pendingIngredientIndex = -1;
}
  

tryAddIngredient(query: string): void {
const trimmedQuery = query.trim();
const user = this.UserService.getUser();

if (!trimmedQuery || this.selectedIngredients.includes(trimmedQuery)) return;

this.CocktailService.searchIngredients(trimmedQuery, user?.id as number)
  .then((res: string) => {
    if (res && res.trim() !== '') {
      this.addIngredient(res.trim());
      this.ingredientQuery = '';
      this.ingredientResults = [];
    }
  })
  .catch((err) => {
    console.error('Errore nella ricerca ingrediente:', err);
  });
  this.checkSaveButtonState()
}
async addImage() {
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = 'image/*';
  const user = this.UserService.getUser()

  input.onchange = async (event: Event) => {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {

      const file = target.files[0];

      try {
        const result:any = await this.CocktailService.CreatedCocktailImg(file, user?.id as number);

        if (result) {
          this.cocktailImg = result.imageUrl;
          console.log('URL immagine salvata:', this.cocktailImg);
        } else {
          this.snackBar.open('Errore durante l\'upload dell\'immagine', '', { duration: 3000 });
          console.error('Risposta inattesa:', result);
        }

      } catch (error) {
        console.error('Eccezione durante l\'upload:', error);
        this.snackBar.open('Upload fallito', '', { duration: 3000 });
      }
    }
  };

  input.click();
}

  
}


  