import { Component, OnInit, HostListener, ViewEncapsulation,TemplateRef, Input, Output, EventEmitter,ViewChild  } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { User, UserService } from '../services/user.service';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { LocationStrategy } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';
import { SearchComponent } from '../search/search.component'; 
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import {  BsModalRef, ModalModule } from 'ngx-bootstrap/modal';
import { MatDialogModule } from '@angular/material/dialog';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CocktailService } from '../services/cocktail.service';
import { CocktailModalComponent } from '../cocktail-modal/cocktail-modal.component';
import { CocktailModalComponentCreate } from '../cocktail-modal-create/cocktail-modal-create.component';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { NgZone, ApplicationRef } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Subscription, interval } from 'rxjs';
import { LikeEventService } from '../services/like-event.service';
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

interface FollowedUser {
  id: string;
  userName: string;
  followed_Users:[];

}


@Component({
  selector: 'app-profile-page',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  imports: [
    CocktailModalComponent,
    CocktailModalComponentCreate,
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatToolbarModule,
    MatMenuModule,
    MatSnackBarModule,
    MatTabsModule,
    FormsModule,
    SearchComponent,
    MatProgressSpinnerModule,
    ModalModule,
    MatDialogModule,
    NgbModule,
  ],
  providers: [
    UserService,
    CocktailService
  ],
  templateUrl: './profile-page.component.html',
  styleUrls: ['./profile-page.component.css']
})
export class ProfilePageComponent implements OnInit {
  @ViewChild('profileModal') profileModal: any;
  @Input() profileImage: string = 'assets/default-profile.png';  
  @Input() userProfile: any = null;
  @Output() close = new EventEmitter<void>();
  @Output() close_create = new EventEmitter<void>();

  CocktailIcon = "/assets/cocktail_modal_icon_1.png";
  private cocktailsSubject = new BehaviorSubject<Cocktail[]>([]);
  cocktails$ = this.cocktailsSubject.asObservable();
  modalRef?: BsModalRef;
  searchQuery: string = '';
  isLoggedIn = false;
  Token: string = '';
  username: string | null = '';
  parallaxImage: string = 'assets/default-profile-background.png';
  parallaxTransform = 'translateY(0)';
  currentYear: number = new Date().getFullYear();
  likedCocktails: Cocktail[] = [];
  myCocktails: Cocktail[] = [];
  activeTab: 'cocktails' | 'liked' | 'friends' = 'cocktails';
  Bio: string = "";
  Bio_link: string = "";
  Language:string = "it";
  isCurrentUser: boolean = false;
  isViewer: boolean = false;
  followingCount: number = 0;
  followersCount: number = 0;
  bio:string = "";
  friends= [];
  followerList: FollowedUser[] = [];
  followingList: FollowedUser[] = [];
  authorized:boolean = true;
  private navigationRedirected: boolean = false;
  isFollowing:boolean = false; 
  public isLoading = true;
  showGear = false;
  isModalOpen = false;
  editedBio = '';
  preUser:any;
  isMobile:boolean = false;
  showPlus:boolean = false;
  modalVisible = false;
  selectedCocktail: any = null;
  private likeSubscription: Subscription | undefined;
  routineSub!: Subscription;
  isLightMode: boolean = true;
  creation:boolean = false;
  showEditForm: boolean = false;
  bioLink: string = '';

  constructor(
    private translateHtmlService: TranslateHtmlService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef,
    private userService: UserService,
    private cocktailService: CocktailService,
    private router: Router,
    private snackBar: MatSnackBar,
    private route: ActivatedRoute,
    private location: LocationStrategy,
    private zone: NgZone,
    private appRef: ApplicationRef,
    private cdRef: ChangeDetectorRef,
    private likeEventService: LikeEventService
  ) {
    this.location.onPopState(() => {
      window.location.reload();
    });
  }

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
    this.isLoading = true;  

    try {
      if (this.navigationRedirected) {
        this.isLoading = false;
        return;
      }
  
      await this.checkAuthStatus();
      
      if (this.navigationRedirected) {
        this.isLoading = false;
        return;
      }
      this.username = this.route.snapshot.paramMap.get('username');
      if (this.username) {
        this.preUser = await this.getUserByUsernameAsync(this.username);
        this.userProfile = this.preUser; 
        this.userProfile.cocktails = await this.cocktailService.getCreatedCocktailById(this.userProfile.id)
      }
      this.likeSubscription = this.likeEventService.likeEvent$.subscribe(() => {
        console.log('Profile: evento like ricevuto tramite service');
        if (this.userProfile && this.userProfile.id) {
          console.log('Profile: ricaricamento cocktails...');
          this.loadCocktails(this.userProfile.id);
        }
      });
      
      const currentUser = this.userService.getUser();
      
      if (currentUser) {
        this.isLoggedIn = true;
        if ((this.username === currentUser.userName) || (currentUser.userName && this.username === null)) {
          this.isCurrentUser = true;
        } else {
          this.isCurrentUser = false;
        }
      }
    
      if (this.isLoggedIn && this.isCurrentUser) {
        await this.router.navigate([`/profile-page/${currentUser?.userName}`]);
        this.navigationRedirected = true;
        this.isLoading = false;
        await this.GetFollowingCount(currentUser!.id as number);
        await this.getFollowersCount()
        return;
      }
      await this.manageVisitor();
    }
    catch (error) {
      console.error('Errore durante l\'inizializzazione del componente:', error);
    } finally {
        this.getIfMobile()  
      this.isLoading = false;
      this.routineSub = interval(1805000 / 4).subscribe(() => {
        this.routineCheckAuth();
      });
    }
    this.routineCheckAuth();
  }

  ngOnDestroy() {
    if (this.likeSubscription) {
      this.likeSubscription.unsubscribe();
    }
    if (this.routineSub) {
      this.routineSub.unsubscribe(); 
    }
  
  }

  toggleEditForm() {
      this.showEditForm = !this.showEditForm;
    }
      
  private getUserByUsernameAsync(username: string): Promise<any> {
    return new Promise((resolve, reject) => {
      this.userService.getUserByUsername(username).subscribe({
        next: (result) => resolve(result),
        error: (err) => reject(err)
      });
    });
  }

  async manageVisitor() {
    if (this.username && !this.navigationRedirected) {
      try {
        this.followerList = await this.userService.getFollowers(this.preUser.id);
        await this.GetFollowingCount(this.preUser.id);
        
      } catch (error) {
        console.error('Errore nel recupero dei followers:', error);
        this.followerList = [];
      }
      
      try {
        const profile = await firstValueFrom(this.userService.getUserByUsername(this.userProfile?.userName));

        
        if (profile === "404") {
          this.snackBar.open(profile, 'OK', { duration: 3000 });
          await this.router.navigate(['/profile-page']);
          this.navigationRedirected = true;
          this.isLoading = false;
          return;
        }
        const user_cocktails = await this.cocktailService.getCreatedCocktailById(profile.id)
        const liked_cocktails = await this.cocktailService.getCocktailsLikedProfile(profile.id);

        console.log(profile, " QUA")
  
        this.userProfile = {
          coverImage: profile.coverImage || 'assets/default-profile-background.png',
          profileImage: profile.imgProfileUrl || 'assets/default-profile.png',
          bio: profile.bio || '',
          bioLink: profile.bioLink || 'www.YourBar.com',
          followersCount: this.followerList.length, 
          cocktails: user_cocktails || [],
          followingCount: this.followingList.length || 0,
          liked: liked_cocktails || [],

          ...profile
        };
        await this.ViewedProfileFollowers(this.userProfile.userName);
      } catch (error) {
        console.error('Errore nel recupero dei dati dell\'utente:', error);
      }
    }
  }

  async GetFollowingCount(userName_id:number)
  {
    const user = this.userService.getUser()
    const Followers = await this.userService.getFollowing(userName_id);
    if (this.isCurrentUser)
    {
      this.followingCount = Followers.length
      return ;
      
    }
    this.followingList = Followers;
  }
  
  async ViewedProfileFollowers(userName:string) {
    const to_follow_followers = await this.userService.getFollowers(this.userProfile.id);
    const asking_to_follow = this.getCurrentUserName();
    
    try {
      if (Array.isArray(to_follow_followers) && to_follow_followers.length > 0) {
        to_follow_followers.forEach(to_follow_followers => {
          if (to_follow_followers.userName === asking_to_follow) {
            this.isFollowing = true;
          }
          else {
            this.isFollowing = false;
          }
        });
      } else {
        console.log("followed_Users non trovato");
      } 
    }
    catch (error) {
      console.error('Errore durante il follow/unfollow:', error);
    }
  }

  async getFollowersCount() {
    if (!this.username)
      return ;
    const user = this.userService.getUser()
    this.userService.getFollowers(user?.id as number).then(
      followers => {
        this.followerList = followers; 
        this.followersCount = followers.length
      }
    ).catch(
      error => {
        console.error('Errore nel recupero dei followers:', error);
      }
    );
  }
    
      async manageFollow(userName: string) {
        if (!userName) return;

        try {
          const profile = await firstValueFrom(this.userService.getUserByUsername(userName));

          const user_cocktails = await this.cocktailService.getCreatedCocktailById(profile.id);

          await this.userService.followUp(this.userProfile.id);
          this.isFollowing = !this.isFollowing;

          const followers = await this.userService.getFollowers(this.userProfile.id);

          this.followerList = followers;
          this.userProfile = {
            coverImage: profile.coverImage || 'assets/default-profile-background.png',
            profileImage: profile.imgProfileUrl || 'assets/default-profile.png',
            bio: profile.bio || '',
            bioLink: profile.bioLink || 'www.YourBar.com',
            followersCount: followers.length,
            cocktails: user_cocktails || [],
            followingCount: this.followingList.length || 0,
            ...profile
          };

          console.log('Profilo aggiornato con:', followers.length, 'follower');
        } catch (error) {
          console.error('Errore durante il follow/unfollow:', error);
        }
      }

  getCurrentUserName(): string | null {
    return this.userService.getUser()?.userName || null;
  }

  @HostListener('window:scroll', ['$event'])
  onWindowScroll() {
    const scrollPosition = window.pageYOffset;
    this.parallaxTransform = `translateY(${scrollPosition * 0.2}px) scale(1.02)`;
  }

  private async checkAuthStatus(): Promise<void> {
    const user = this.userService.getUser();
    if (user) {
      this.Token = await this.userService.isLoggedIn(user);
      if (this.Token === "" || this.Token === null) {
        this.handleUnauthorizedAccess();
        return;
      } else {
        this.isLoggedIn = true;
        this.username = user.userName;
        this.profileImage = user.imgProfileUrl || 'assets/default-profile.png';
        this.parallaxImage = user.ProfileParallaxImg || 'assets/profile-pic.png';
        this.loadCocktails(user.id as number);
        this.Bio = user.Bio || 'Cocktail enthusiast 🍹 | Creating delicious drinks';
        this.Bio_link = user.Bio_link || 'www.YourBar.com';
      }
    } else {
      this.isLoggedIn = false;
      this.handleUnauthorizedAccess('Not logged in.');
    }
  }

  async loadCocktails(id: number): Promise<void> {
    this.routineCheckAuth();
    console.log("loadCocktails chiamato con userId:", id);
    try {
        this.likedCocktails = await this.cocktailService.getCocktailsLikedProfile(id);
        this.myCocktails =  await this.cocktailService.getUserCocktails();
        this.cocktailsSubject.next(this.likedCocktails);
    } catch (error) {
        console.error('Errore durante il caricamento dei cocktail:', error);
    }
}




  private handleUnauthorizedAccess(message: string = 'Session time out'): void {
    this.navigationRedirected = true;
    this.snackBar.open(message, 'OK', { duration: 3000 });
    this.userService.clearCurrentUser()
    this.router.navigate(['/login-signup']);
  }

  logout(): void {
    this.userService.logout().subscribe({
      next: () => this.router.navigate(['/login-signup']),
      error: (err: Error) => {
        console.error('Logout error:', err);
        this.userService.forceLogout();
        this.snackBar.open('Sessione terminata', 'OK', { duration: 3000 });
      }
    });
  }

  getSelectedTabIndex(): number {
    return ['cocktails', 'liked', 'friends'].indexOf(this.activeTab);
  }

  onTabChange(event: MatTabChangeEvent): void {
    const tabs: ('cocktails' | 'liked' | 'friends')[] = ['cocktails', 'liked', 'friends'];
    this.activeTab = tabs[event.index];
  
    if (event.tab.textLabel === 'Liked') {
      setTimeout(() => {
        this.startGlowFlicker('.liked-grid-item-frame');
      }, 300);  
    }
  }
  
  

  updateBackground(imageUrl: string): void {
    this.parallaxImage = imageUrl;
  }

  navigateToLogin() {
    this.router.navigate(['/login']);
  }
  
  navigateToCreate() {
    this.creation = true;
  }
  
  navigateToExplore() {
    this.router.navigate(['/cocktails']);
  }
  
  async navigateToCocktail(cocktailId: any) {
    const response = await this.cocktailService.getCocktailById(cocktailId);
    this.selectedCocktail = JSON.parse(response);
  }

  openModal() {
    this.editedBio = this.userProfile.bio;
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
    this.modalRef?.hide();
  }

  saveBio() {
    this.userProfile.bio = this.editedBio;
    this.isModalOpen = false;
  }

  getIfMobile()
  {
    let screenSize = this.getScreenSize()
    if (screenSize.width < 800)
    {
      this.isMobile = true
    }
    else if (screenSize.width > 800)
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
  openEditModal(content: any): void {
    this.isModalOpen = true;
  }

  onModifyImage() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
  
    this.closeModal();
    
    input.onchange = (event: Event) => {
      const target = event.target as HTMLInputElement;
      if (target.files && target.files.length > 0) {
        this.snackBar.open('Processing...', '', { duration: 5000 });
        
        const file = target.files[0];
        console.log('Immagine caricata:', file);
        
        const currentUser = this.userService.getUser();
        if (currentUser?.id) {
          this.userService.updateProfileImg(currentUser.id, file)
            .then(response => {
              if (response && (response as any).url) {
                this.profileImage = (response as any).url + '?t=' + new Date().getTime();
                const user = this.userService.getUser()
                const updated_user = {
                  id:user?.id!,
                  userName: user?.userName,
                  name: user?.name,
                  lastName: user?.lastName,
                  email: user?.email,
                  PasswordHash: user?.PasswordHash,
                  acceptCookies: user?.acceptCookies,
                  imgProfileUrl: this.profileImage,
                  ProfileParallaxImg: user?.ProfileParallaxImg,
                  token: user?.token,
                  Bio: user?.Bio,
                  Bio_link: user?.Bio_link,
                }
                this.userService.setCurrentUser(updated_user as User);
                this.snackBar.open('Saved', '', { duration: 3000 });
              } else {
                this.snackBar.open('Error: Invalid response', '', { duration: 3000 });
              }
              console.log('Risposta aggiornamento immagine:', response);
            })
            .catch(error => {
              console.error('Errore aggiornamento immagine:', error);
              this.snackBar.open('Error uploading image', '', { duration: 3000 });
            });
        } else {
          console.error('Utente non trovato');
          this.snackBar.open('User not found', '', { duration: 3000 });
        }
      }
    };
  
    input.click();
  }
  
  
  startGlowFlicker(selector: string) {
    this.cdRef.detectChanges();  
    const elements = document.querySelectorAll(selector);
    console.log('Elements selected: ', elements);
    if (elements.length === 0) {
      console.log(`No elements found for selector: ${selector}`);
    } else {
      elements.forEach((element) => {
        console.log('Adding glow-on to element:', element);
        element.classList.add('glow-on');
      });
    }
  }
  
  
  getIngredients(cocktail: any) {
    const ingredients = cocktail.ingredients || [];
    const measures = cocktail.measures || [];
    
    return ingredients.map((ingredient:any, index:any) => ({
      name: ingredient,
      measure: measures[index] || '' 
    }));
  }
  
  onClose(): void {
    this.isModalOpen = false;
    this.close.emit();
  }
  
  onClose_Create(): void {

    this.creation = false;
    this.close_create.emit();
  }
  

  handleTouch() {
    this.getIfMobile()
    if (this.isMobile && this.isCurrentUser) {
      this.openEditModal(this.profileModal);
    }
  }


  async routineCheckAuth()
  {

    const user = this.userService.getUser();  
    const update_user = this.userService.getUserByUsername(user?.userName!).subscribe(
      (response) => {
        console.log('Dati dell\'utente:', response);
        if (response === "404" || !response )
        {
          this.handleUnauthorizedAccess();
        }
      },
      (error) => {
        console.error('Errore durante il recupero dei dati dell\'utente:', error);
      }
    );
    
    if (user) {
      this.Token = await this.userService.isLoggedIn(user);
      if (this.Token === "" || this.Token === null) {
        this.handleUnauthorizedAccess();
        
        return;
      }
    }
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
  }

  async editBio()
  {
      let data = this.userService.getUser();
      const userPw = await this.userService.getUserPassword(data?.id as number)
      const new_data:any = {
      id: data?.id,
      userName: data?.userName,
      name:  data?.name,
      lastName:  data?.lastName,
      email:  data?.email,
      PasswordHash:  userPw?.password,
      acceptCookies:  data?.acceptCookies,
      token: this.userService.getToken() || "",
      Bio: this.bio || "" , 
      Bio_link: this.bioLink || "",
      
    }
    if (data?.id !== undefined) {
          try {
            let res;
            res = await this.userService.updateUser(data?.id, new_data);
            if (res)
            {
              this.checkAuthStatus()
              this.snackBar.open('Profilo aggiornato con successo!', 'Chiudi', {
                duration: 3000,
                panelClass: ['success-snackbar']
              });
            }
          } catch (error: any) {
            this.snackBar.open(error, 'Chiudi', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        } else {
          this.snackBar.open('Error.', 'Chiudi', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
    this.showEditForm = false;
  }
  
}