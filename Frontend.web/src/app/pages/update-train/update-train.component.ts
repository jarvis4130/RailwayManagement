import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ScheduleService } from '../../services/schedule.service';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CommonModule } from '@angular/common';
import { TrainService } from '../../services/train.service';
import { StationService } from '../../services/station.service';

@Component({
  selector: 'app-update-train',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './update-train.component.html',
  styleUrl: './update-train.component.css',
})
export class UpdateTrainComponent {
  form: FormGroup;
  trainList: any[] = []; // Now objects with trainID & trainName
  scheduleList: any[] = [];
  stationList: any[] = []; // For all stations
  today: string = new Date().toISOString().split('T')[0];

  constructor(
    private fb: FormBuilder,
    private trainService: TrainService,
    private router: Router,
    private stationService: StationService,
    private scheduleService: ScheduleService
  ) {
    this.form = this.fb.group({
      date: ['', Validators.required],
      trainID: ['', Validators.required],
    });
  }

  ngOnInit() {
    this.loadAllStations();
  }

  loadAllTrains() {
    this.trainService.getAllTrains().subscribe({
      next: (data) => (this.trainList = data),
      error: (err) => console.error('Error loading trains:', err),
    });
  }

  loadAllStations() {
    this.stationService.getAllStations().subscribe({
      next: (data) => (this.stationList = data),
      error: (err) => console.error('Error loading stations:', err),
    });
  }

  onDateSelect() {
    const selectedDate = this.form.value.date;
    if (!selectedDate) return;

    this.scheduleService.getScheduledTrainIDs(selectedDate).subscribe({
      next: (trainIds: number[]) => {
        if (trainIds.length === 0) {
          this.trainList = [];
          alert('No train schedule on this date 🚫');
          return;
        }

        // Fetch all trains
        this.trainService.getAllTrains().subscribe({
          next: (allTrains) => {
            // Filter only those with matching IDs
            this.trainList = allTrains.filter((train: any) =>
              trainIds.includes(train.trainID)
            );
          },
          error: (err) => console.error('Error loading trains:', err),
        });
      },
      error: (err) => console.error('Error fetching train IDs by date:', err),
    });
  }

  extractTimeOnly(dateTimeStr: string): string {
    const date = new Date(dateTimeStr);
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${hours}:${minutes}`;
  }

  onTrainSelect() {
    const { date, trainID } = this.form.value;
    if (!date || !trainID) return;

    this.scheduleService.getScheduleByTrainAndDate(trainID, date).subscribe({
      next: (schedules) => {
        this.scheduleList = schedules.map((s: any) => ({
          ...s,
          arrivalTime: this.extractTimeOnly(s.arrivalTime),
          departureTime: this.extractTimeOnly(s.departureTime),
          fair: s.fair,
        }));
      },
      error: (err) => console.error('Error fetching schedule:', err),
    });
  }

  submitUpdatedSchedule() {
    const { trainID, date } = this.form.value;

    if (!trainID || !date) {
      alert('🚫 Train and Date are required!');
      return;
    }

    const localDateStr = date; // already "YYYY-MM-DD"

    const payload = {
      date: new Date(localDateStr).toISOString(), // schedule date (UTC)
      trainID: +trainID,
      schedules: this.scheduleList.map((s) => {
        const arrivalISO = this.combineDateAndTime(localDateStr, s.arrivalTime);
        const departureISO = this.combineDateAndTime(
          localDateStr,
          s.departureTime
        );

        return {
          stationID: s.stationID,
          arrivalTime: arrivalISO,
          departureTime: departureISO,
          sequenceOrder: +s.sequenceOrder,
          fare: +s.fair,
          distanceFromSource: +s.distanceFromSource,
        };
      }),
    };

    console.log('🧾 Final Payload:', payload);

    this.scheduleService.updateSchedule(payload).subscribe({
      next: () => {
        alert('✅ Schedule successfully updated!');
        this.router.navigate(['/admin/dashboard']);
      },
      error: (err) => {
        console.error('❌ Schedule update failed:', err);
      },
    });
  }

  combineDateAndTime(dateStr: string, timeStr: string): string {
    return `${dateStr}T${timeStr}:00`; // Local time in ISO 8601 format (without timezone shift)
  }
}
