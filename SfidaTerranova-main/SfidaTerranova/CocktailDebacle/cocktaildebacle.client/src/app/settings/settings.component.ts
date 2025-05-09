import { Component, OnInit, TemplateRef, ViewChild, ViewEncapsulation, PLATFORM_ID, Inject,  HostListener} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { UserService } from '../services/user.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BsModalService, BsModalRef, ModalModule } from 'ngx-bootstrap/modal';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatRadioModule } from '@angular/material/radio';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCheckboxModule,
    MatIconModule,
    MatRadioModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatButtonModule,
    ModalModule
  ],
  providers: [
    BsModalService,
    UserService
  ],
  encapsulation: ViewEncapsulation.None,
})
export class SettingsComponent implements OnInit {
  
  @HostListener('document:keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    if (event.key === "Enter" && this.modalVisible)
    {
      this.onSave()
    }
  }
  @ViewChild('editModal') editModal!: TemplateRef<any>;
  userProfile: any;
  editForm: FormGroup;
  modalRef?: BsModalRef;
  hidePassword = true;
  formError = false;
  modalVisible = false;
  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private router: Router,
    private snackBar: MatSnackBar,
    private modalService: BsModalService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.editForm =  this.fb.group({
      FirstName: ['', [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-ZÀ-ÿ\s']+$/)]],
      LastName: ['', [Validators.required, Validators.pattern(/^[a-zA-ZÀ-ÿ\s']+$/)]],
      userName: ['', [Validators.required, Validators.minLength(4), Validators.pattern(/^[a-zA-Z0-9_]+$/)]],
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', [
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$/)
      ]],
      AcceptCookies: [false],
      IsAdult: [false],
    }, {
      validators: []
    }); 
  }

  ngOnInit(): void {
    this.loadUserProfile();
  }



  async loadUserProfile(): Promise<void> {
    const data = this.userService.getUser();
    console.log("data:", data)
    if (data) {
      this.userProfile = data;
      this.editForm.patchValue({
        FirstName: data.name,
        LastName: data.lastName,
        userName: data.userName,
        Email: data.email,
        AcceptCookies: data.acceptCookies,
      });
    } else {
      this.snackBar.open('Failed to load profile', 'Close', { duration: 3000 });
    }
  }
  
  openEditModal(): void {
    this.modalVisible = true;
    this.editForm.get('Password')?.setValue('');
    
    this.modalRef = this.modalService.show(this.editModal, {
      class: 'modal-lg',
      backdrop: 'static'
    });
  }

  async onSave(): Promise<void> {
    if (this.editForm.invalid) {
      this.formError = true;
      this.markAllAsTouched();
    
      const controls = this.editForm.controls;
    
      for (const key in controls) {
        if (controls[key].invalid) {
          console.log(`Errore nel campo '${key}':`, controls[key].errors);
        }
      }
    
      return;
    }
    
    let data = this.userService.getUser();
    console.log("PRIMA DI:", data)
    const userPw = await this.userService.getUserPassword(data?.id as number)
    const new_data:any = {
      id: data?.id,
      userName: this.editForm.get('userName')?.value || data?.userName,
      name: this.editForm.get('FirstName')?.value || data?.name,
      lastName: this.editForm.get('LastName')?.value || data?.lastName,
      email: this.editForm.get('Email')?.value || data?.email,
      PasswordHash: this.editForm.get('Password')?.value || userPw?.password,
      acceptCookies: this.editForm.get('AcceptCookies')?.value || data?.acceptCookies,
      token: this.userService.getToken() || ""
    };
    console.log(new_data, " VICTORES")
    
    if (data?.id !== undefined) {
      try {
        let res;
        res = await this.userService.updateUser(data?.id, new_data);
        if (res)
        {
          this.snackBar.open('Profilo aggiornato con successo!', 'Chiudi', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
          this.closeModal()
          await this.loadUserProfile(); 

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
    this.closeModal()
  }

  private markAllAsTouched() {
    Object.values(this.editForm.controls).forEach(control => {
      control.markAsTouched();
    });
  }
  get f() { return this.editForm.controls; }

  goBack() {
    window.history.back(); 
  }

  closeModal()
  {
    this.modalVisible = false;
    this.modalRef?.hide()
  }  
  
}