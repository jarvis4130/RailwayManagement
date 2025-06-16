// import { CommonModule } from '@angular/common';
// import { Component, OnInit } from '@angular/core';
// import { TrainService } from '../../services/train.service';
// import { StationService } from '../../services/station.service';
// import { ScheduleService } from '../../services/schedule.service';
// import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

//
// export class AddScheduleComponent implements OnInit {
//   form!: FormGroup;
//   trains:any = [];

//   scheduledTrainIDs: number[] = [2]; // fetched from backend, here mocked

//   stations:any = [];

//   constructor(private fb: FormBuilder,private trainService: TrainService, private stationService: StationService, private scheduleService: ScheduleService) {}

//   ngOnInit() {
//     this.form = this.fb.group({
//       trainID: [null, Validators.required],
//       date: [null, Validators.required],
//       schedules: this.fb.array([]),
//     });
//     this.trainService.getAllTrains().subscribe(data => this.trains = data);
//     this.stationService.getAllStations().subscribe(data => this.stations = data);
//     this.addStop(); // start with 1 stop
//   }

//   get schedules() {
//     return this.form.get('schedules') as FormArray;
//   }

//   addStop() {
//     const stopForm = this.fb.group({
//       stationID: [null, Validators.required],
//       arrivalTime: ['', Validators.required],
//       departureTime: ['', Validators.required],
//       sequenceOrder: [this.schedules.length + 1], // auto-sequence
//       distanceFromSource: [0, [Validators.required, Validators.min(0)]],
//       fare: [0, [Validators.required, Validators.min(0)]],
//     });

//     this.schedules.push(stopForm);
//   }

//   submitSchedule() {
//     console.log(this.form.value);
//     // send to backend
//   }

//   isTrainDisabled(trainID: number): boolean {
//     return this.scheduledTrainIDs.includes(trainID);
//   }
// }

import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { TrainService } from '../../services/train.service';
import { StationService } from '../../services/station.service';
import { ScheduleService } from '../../services/schedule.service';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { Router } from '@angular/router';

@Component({
  selector: 'app-addschedule',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './addschedule.component.html',
  styleUrl: './addschedule.component.css',
})
export class AddScheduleComponent implements OnInit {
  form!: FormGroup;
  trains: any[] = [];
  stations: any[] = [];
  today: string = new Date().toISOString().split('T')[0];

  scheduledTrainIDs: number[] = [];
  selectedDate: string | null = null;

  constructor(
    private fb: FormBuilder,
    private trainService: TrainService,
    private stationService: StationService,
    private scheduleService: ScheduleService,
    private router: Router
  ) {}

  ngOnInit() {
    this.form = this.fb.group({
      date: [null, Validators.required],
      trainID: [{ value: null, disabled: true }, Validators.required],
      schedules: this.fb.array([]),
    });

    this.trainService
      .getAllTrains()
      .subscribe((data: any[]) => (this.trains = data));
    this.stationService
      .getAllStations()
      .subscribe((data: any[]) => (this.stations = data));
    this.addStop();
  }

  get schedules(): FormArray {
    return this.form.get('schedules') as FormArray;
  }

  addStop() {
    const stopForm = this.fb.group({
      stationID: [null, Validators.required],
      arrivalTime: ['', Validators.required],
      departureTime: ['', Validators.required],
      sequenceOrder: [this.schedules.length + 1],
      distanceFromSource: [0, [Validators.required, Validators.min(0)]],
      fare: [0, [Validators.required, Validators.min(0)]],
    });

    this.schedules.push(stopForm);
  }

  onDateChange() {
    const date = this.form.get('date')?.value;
    this.selectedDate = date;
    console.log(date);
    if (date) {
      this.scheduleService
        .getScheduledTrainIDs(date)
        .subscribe((ids: number[]) => {
          this.scheduledTrainIDs = ids;
          this.form.get('trainID')?.enable(); // Enable train selection
        });
    } else {
      this.scheduledTrainIDs = [];
      this.form.get('trainID')?.disable(); // Disable if date is cleared
    }
  }

  isTrainDisabled(trainID: number): boolean {
    return this.scheduledTrainIDs.includes(trainID);
  }

  combineDateAndTime(dateStr: string, timeStr: string): string {
    const [hours, minutes] = timeStr.split(':').map(Number);
    const date = new Date(dateStr);
    date.setHours(hours, minutes, 0, 0);
    return date.toISOString();
  }

  submitSchedule() {
    if (this.form.valid) {
      const formValue = this.form.value;
      const trainID = parseInt(formValue.trainID); // Ensure it's a number
      const journeyDate = formValue.date;

      const requests = formValue.schedules.map((stop: any) => {
        const stationID = parseInt(stop.stationID); // Ensure it's a number

        // Combine date with time to form ISO datetime strings
        const arrivalTime = this.combineDateAndTime(
          journeyDate,
          stop.arrivalTime
        );
        const departureTime = this.combineDateAndTime(
          journeyDate,
          stop.departureTime
        );

        const payload = {
          trainID,
          stationID,
          arrivalTime,
          departureTime,
          sequenceOrder: stop.sequenceOrder,
          fair: stop.fare,
          distanceFromSource: stop.distanceFromSource,
        };

        return this.scheduleService.addSchedule(payload);
      });

      // Send all requests
      forkJoin(requests).subscribe({
        next: () => {
          alert('✅ All stops scheduled successfully!');
          this.form.reset(); // 💡 Reset form
          this.schedules.clear(); // 💡 Clear all stop fields
          this.router.navigate(['/admin/dashboard']);
        },
        error: (err) => console.error('❌ Error scheduling stops:', err),
      });
    } else {
      console.warn('❌ Form is invalid');
    }
  }
}
