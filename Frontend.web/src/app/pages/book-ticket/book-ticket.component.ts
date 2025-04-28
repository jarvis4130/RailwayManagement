import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-book-ticket',
  standalone: true,
  imports: [CommonModule,FormsModule],
  templateUrl: './book-ticket.component.html',
  styleUrl: './book-ticket.component.css'
})
export class BookTicketComponent implements OnInit {
  // Declare all required properties
  insurance: boolean = false;  // Initialize the insurance property to false
  passengers: any[] = [];      // Initialize the passengers array
  // Add other properties you need like trainId, classType, etc.
  
  trainId: number = 0;
  classType: string = '';
  journeyDate: string = '';
  source: string = '';
  destination: string = '';
  
  constructor(private route: ActivatedRoute) {}

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.trainId = params['trainId'];
      this.classType = params['classType'];
      this.journeyDate = params['journeyDate'];
      this.source = params['source'];
      this.destination = params['destination'];
    });
  }

  // Add a method to handle passenger information and insurance selection
  addPassenger() {
    this.passengers.push({ name: '', age: '', gender: '' });  // Add a new passenger entry
  }

  removePassenger(index: number) {
    this.passengers.splice(index, 1);  // Remove the passenger at the given index
  }

  submitBooking() {
    console.log('Booking Details:', {
      trainId: this.trainId,
      classType: this.classType,
      journeyDate: this.journeyDate,
      source: this.source,
      destination: this.destination,
      passengers: this.passengers,
      insurance: this.insurance
    });
    // Add further logic to submit the booking to the backend
  }
}