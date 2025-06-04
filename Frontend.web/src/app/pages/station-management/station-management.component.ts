// // station-management.component.ts
// import { Component, OnInit } from '@angular/core';
// import { StationService } from '../../services/station.service';
// import { CommonModule } from '@angular/common';
// import { RouterModule } from '@angular/router';

// @Component({
//   standalone: true,
//   selector: 'app-station-management',
//   imports: [CommonModule, RouterModule],
//   templateUrl: './station-management.component.html',
//   styleUrls: ['./station-management.component.css']
// })
// export class StationManagementComponent implements OnInit {
//   stations: any[] = [];

//   constructor(private stationService: StationService) {}

//   ngOnInit(): void {
//     this.fetchStations();
//   }

//   fetchStations(): void {
//     this.stationService.getAllStations().subscribe((data) => {
//       this.stations = data;
//     });
//   }

//   confirmDelete(station: any): void {
//     const isConfirmed = confirm(`Are you sure you want to delete ${station.stationName}?`);
//     if (isConfirmed) {
//       this.stationService.deleteStation(station.stationID).subscribe(() => {
//         this.fetchStations(); // Refresh list
//       });
//     }
//   }
// }

import { Component, OnInit } from '@angular/core';
import { StationService } from '../../services/station.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  standalone: true,
  selector: 'app-station-management',
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './station-management.component.html',
  styleUrls: ['./station-management.component.css'],
})
export class StationManagementComponent implements OnInit {
  stations: any[] = [];
  showForm = false;
  editStation: any = null;
  showModal = false;
  stationToDelete: any = null;

  newStation = {
    stationName: '',
    location: '',
  };

  constructor(private stationService: StationService) {}

  ngOnInit(): void {
    this.fetchStations();
  }

  fetchStations(): void {
    this.stationService.getAllStations().subscribe((data) => {
      this.stations = data;
    });
  }

  toggleForm(): void {
    this.showForm = !this.showForm;
  }

  addStation(): void {
    this.stationService.addStation(this.newStation).subscribe(() => {
      this.newStation = { stationName: '', location: '' };
      this.showForm = false;
      this.fetchStations();
    });
  }

  startEdit(station: any): void {
    // Clone the station so changes don't instantly affect the original list
    this.editStation = { ...station };
  }

  cancelEdit(): void {
    this.editStation = null;
  }

  saveEdit(): void {
    this.stationService
      .updateStation(this.editStation.stationID, {
        stationName: this.editStation.stationName,
        location: this.editStation.location,
      })
      .subscribe(() => {
        this.fetchStations();
        this.editStation = null;
      });
  }

  confirmDelete(station: any) {
    this.stationToDelete = station;
    this.showModal = true;
  }

  // User confirms deletion
  confirmCancel() {
    if (!this.stationToDelete) return;

    // Call your delete method here, e.g.,
    this.stationService
      .deleteStation(this.stationToDelete.stationID)
      .subscribe({
        next: () => {
          // Remove from local list or reload stations
          this.stations = this.stations.filter(
            (s) => s.stationID !== this.stationToDelete.stationID
          );
          this.closeModal();
        },
        error: (err) => {
          console.error('Failed to delete station', err);
          this.closeModal();
        },
      });
  }

  // User cancels modal or clicks "No, Go Back"
  closeModal() {
    this.showModal = false;
    this.stationToDelete = null;
  }
}
