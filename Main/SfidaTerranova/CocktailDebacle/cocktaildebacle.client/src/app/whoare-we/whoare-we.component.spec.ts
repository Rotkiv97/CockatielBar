import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WhoareWeComponent } from './whoare-we.component';

describe('WhoareWeComponent', () => {
  let component: WhoareWeComponent;
  let fixture: ComponentFixture<WhoareWeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WhoareWeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WhoareWeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
