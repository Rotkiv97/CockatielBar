import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CocktailModalComponentCreate } from './cocktail-modal-create.component';

describe('CocktailModalCreateComponent', () => {
  let component: CocktailModalComponentCreate;
  let fixture: ComponentFixture<CocktailModalComponentCreate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CocktailModalComponentCreate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CocktailModalComponentCreate);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
