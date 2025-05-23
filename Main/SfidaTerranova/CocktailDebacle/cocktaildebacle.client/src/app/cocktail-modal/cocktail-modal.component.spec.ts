import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CocktailModalComponent } from './cocktail-modal.component';

describe('CocktailModalComponent', () => {
  let component: CocktailModalComponent;
  let fixture: ComponentFixture<CocktailModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CocktailModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CocktailModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
