import { Component, OnInit } from '@angular/core';
import { TrainService } from '../../services/train.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-train-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './train-list.component.html',
  styleUrl: './train-list.component.css',
})
// export class TrainListComponent implements OnInit {
//   trains: any[] = [];
//   loading = true;

//   constructor(private trainService: TrainService) {}

//   ngOnInit() {
//     this.trainService.getAllTrains().subscribe({
//       next: (data) => {
//         this.trains = data;
//         this.loading = false;
//       },
//       error: (err) => {
//         console.error('Failed to fetch trains', err);
//         this.loading = false;
//       },
//     });
//   }

//   newTrain = {
//     trainName: '',
//     trainType: '',
//     totalSeats: 0,
//   };

//   onTrainTypeChange() {
//     if (this.newTrain.trainType === 'Superfast') {
//       this.newTrain.totalSeats = 840;
//     } else if (this.newTrain.trainType === 'Express') {
//       this.newTrain.totalSeats = 948;
//     } else {
//       this.newTrain.totalSeats = 0;
//     }
//   }

//   addTrain() {
//     // Here you can call your service to save train
//     console.log('Train to add:', this.newTrain);

//     // Reset form
//     this.newTrain = { trainName: '', trainType: '', totalSeats: 0 };
//   }
// }
export class TrainListComponent implements OnInit {
  trains: any[] = [];
  loading = true;

  showAddTrainForm = false;

  newTrain = {
    trainName: '',
    trainType: '',
    totalSeats: 0,
  };

  editingTrain: any = null;

  editTrain(train: any) {
    this.editingTrain = { ...train }; // clone the train object
  }

  cancelEdit() {
    this.editingTrain = null;
  }

  constructor(private trainService: TrainService) {}

  ngOnInit() {
    this.fetchTrains();
  }

  fetchTrains() {
    this.loading = true;
    this.trainService.getAllTrains().subscribe({
      next: (data) => {
        this.trains = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  toggleAddTrainForm() {
    this.showAddTrainForm = !this.showAddTrainForm;
  }


  // onTrainTypeChange() {
  //   if (this.newTrain.trainType === 'Superfast') {
  //     this.newTrain.totalSeats = 840;
  //   } else if (this.newTrain.trainType === 'Express') {
  //     this.newTrain.totalSeats = 948;
  //   } else {
  //     this.newTrain.totalSeats = 0;
  //   }
  // }

  onTrainTypeChange(context: 'new' | 'edit') {
  let targetTrain = context === 'new' ? this.newTrain : this.editingTrain;

  if (targetTrain.trainType === 'Superfast') {
    targetTrain.totalSeats = 840;
  } else if (targetTrain.trainType === 'Express') {
    targetTrain.totalSeats = 948;
  } else {
    targetTrain.totalSeats = 0;
  }
}

  addTrain() {
    if (!this.newTrain.trainName || !this.newTrain.trainType) return;

    this.trainService.addTrain(this.newTrain).subscribe({
      next: () => {
        this.newTrain = { trainName: '', trainType: '', totalSeats: 0 };
        this.showAddTrainForm = false;
        this.fetchTrains(); // Refresh list
      },
      error: (err) => {
        console.error('Failed to add train:', err);
        // Optionally show an error message to user
      },
    });
  }

  updateTrain() {
    const updatePayload = {
      trainName: this.editingTrain.trainName,
      trainType: this.editingTrain.trainType,
      totalSeats: this.editingTrain.totalSeats,
    };

    console.log(this.editingTrain);

    this.trainService.updateTrain(this.editingTrain.trainID,updatePayload).subscribe({
      next: () => {
        this.fetchTrains();
        this.cancelEdit();
      },
      error: (err) => {
        console.error('Failed to update train', err);
      },
    });
  }
}
