import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteTrainComponent } from './delete-train.component';

describe('DeleteTrainComponent', () => {
  let component: DeleteTrainComponent;
  let fixture: ComponentFixture<DeleteTrainComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteTrainComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeleteTrainComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
