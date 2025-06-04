import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class StationService {
  private baseUrl = 'http://localhost:5039/api/Station';

  constructor(private http: HttpClient) {}

  private getHeaders() {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    });
  }

  getAllStations(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}`, {
      headers: this.getHeaders(),
    });
  }

  addStation(station: {
    stationName: string;
    location: string;
  }): Observable<any> {
    return this.http.post(this.baseUrl, station, {
      headers: this.getHeaders(),
    });
  }

  updateStation(id: number, updatedData: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, updatedData, {
      headers: this.getHeaders(),
    });
  }

  deleteStation(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, {
      headers: this.getHeaders(),
    });
  }

  getStationIdByName(stationName: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/id?name=${stationName}`, {
      headers: this.getHeaders(),
    });
  }
}
