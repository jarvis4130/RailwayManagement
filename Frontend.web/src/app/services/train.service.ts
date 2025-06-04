import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SeatAvailabilityDTO {
  // classTypeId: number;       // Class Type ID (e.g., 1, 2, 3, 4)
  classType: string;
  remainingSeats: number; // Available seats
  waitingListCount: number; // Number of people on the waiting list
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
  providedIn: 'root',
})
export class TrainService {
  private apiUrl = 'http://localhost:5039/api/Train';

  constructor(private http: HttpClient) {}

  private getHeaders() {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    });
  }

  searchTrains(data: any): Observable<TrainDTO[]> {
    return this.http.post<TrainDTO[]>(`${this.apiUrl}/search`, data);
  }

  getAllTrains(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/all-trains`, {
      headers: this.getHeaders(),
    });
  }

  addTrain(train: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/add-train`, train, {
      headers: this.getHeaders(),
    });
  }

  updateTrain(id: number, train: any): Observable<any> {
    console.log(id);
    return this.http.put(`${this.apiUrl}/${id}`, train, {
      headers: this.getHeaders(),
    });
  }

}
