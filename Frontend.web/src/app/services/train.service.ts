import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';


export interface SeatAvailabilityDTO {
  // classTypeId: number;       // Class Type ID (e.g., 1, 2, 3, 4)
  classType: string;
  remainingSeats: number;    // Available seats
  waitingListCount: number;  // Number of people on the waiting list
}


// Define interface for Train
export interface TrainDTO {
  trainId: number;
  trainName: string;
  source: string;
  destination: string;
  departureTime: string;
  arrivalTime: string;
  durationMinutes: number;
  journeyDate?: string; // optional
  seatAvailability: SeatAvailabilityDTO[];
}

@Injectable({
  providedIn: 'root'
})
export class TrainService {
  private apiUrl = 'http://localhost:5039/api/Train';  // Update if your backend URL is different

  constructor(private http: HttpClient) {}

  searchTrains(data: any): Observable<TrainDTO[]> {
    return this.http.post<TrainDTO[]>(`${this.apiUrl}/search`, data);
  }
}
