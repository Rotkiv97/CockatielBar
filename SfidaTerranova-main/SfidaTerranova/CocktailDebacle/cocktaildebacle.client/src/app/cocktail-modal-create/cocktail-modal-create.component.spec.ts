import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CocktailModalCreateComponent } from './cocktail-modal-create.component';

describe('CocktailModalCreateComponent', () => {
  let component: CocktailModalCreateComponent;
  let fixture: ComponentFixture<CocktailModalCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CocktailModalCreateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CocktailModalCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
