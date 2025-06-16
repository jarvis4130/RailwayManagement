import { Component } from '@angular/core';
import { FormBuilder, FormGroup , FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TrainService } from '../../services/train.service';
import { ScheduleService } from '../../services/schedule.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-delete-train',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './delete-train.component.html',
  styleUrl: './delete-train.component.css'
})

export class DeleteTrainComponent {
  form: FormGroup;
  trainList: any[] = [];
  today: string = new Date().toISOString().split('T')[0];

  constructor(
    private fb: FormBuilder,
    private trainService: TrainService,
    private scheduleService: ScheduleService,
    private router: Router
  ) {
    this.form = this.fb.group({
      date: ['', Validators.required],
      trainID: ['', Validators.required],
    });
  }

  onDateSelect() {
    const selectedDate = this.form.value.date;
    if (!selectedDate) return;

    this.scheduleService.getScheduledTrainIDs(selectedDate).subscribe({
      next: (trainIds: number[]) => {
        if (trainIds.length === 0) {
          this.trainList = [];
          alert('❌ No train schedules found on this date!');
          return;
        }

        this.trainService.getAllTrains().subscribe({
          next: (allTrains) => {
            this.trainList = allTrains.filter((train: any) =>
              trainIds.includes(train.trainID)
            );
          },
          error: (err) => console.error('Error loading trains:', err),
        });
      },
      error: (err) => console.error('Error fetching scheduled train IDs:', err),
    });
  }

  deleteSchedule() {
    const { trainID, date } = this.form.value;

    if (!trainID || !date) {
      alert('🚨 Please select both Date and Train!');
      return;
    }

    console.log(date)

    this.scheduleService.deleteSchedule(trainID, date).subscribe({
      next: () => {
        alert('✅ Schedule deleted successfully!');
        this.router.navigate(['/admin/dashboard']);
      },
      error: (err) => {
        console.error('❌ Failed to delete schedule:', err);
        alert('Something went wrong while deleting schedule!');
      },
    });
  }
}
