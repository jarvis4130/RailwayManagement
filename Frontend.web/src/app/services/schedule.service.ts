import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ScheduleService {
  private baseUrl = 'http://localhost:5039/api/Train';

  constructor(private http: HttpClient) {}

  private getHeaders() {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    });
  }

  getScheduledTrainIDs(date: string): Observable<number[]> {
    return this.http.get<number[]>(
      `${this.baseUrl}/trains-by-date?date=${date}`,
      {
        headers: this.getHeaders(),
      }
    );
  }

  getScheduleByTrainAndDate(trainId: number, date: string) {
    return this.http.get<any[]>(
      `${this.baseUrl}/schedule-by-train-and-date?trainId=${trainId}&date=${date}`,
      {
        headers: this.getHeaders(),
      }
    );
  }

  updateSchedule(train: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/update-schedule`, train, {
      headers: this.getHeaders(),
    });
  }

  addSchedule(schedule: TrainScheduleDTO): Observable<any[]> {
    return this.http.post<any>(`${this.baseUrl}/schedule-train`, schedule, {
      headers: this.getHeaders(),
    });
  }

  deleteSchedule(trainId: number, date: string) {
    return this.http.delete(
      `${this.baseUrl}/delete?trainId=${trainId}&arrivalDate=${date}`,
      {
        headers: this.getHeaders(),
      }
    );
  }
}
export interface TrainScheduleDTO {
  trainID: number;
  stationID: number;
  arrivalTime: string;
  departureTime: string;
  sequenceOrder: number;
  fair: number;
  distanceFromSource: number;
}
