import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class StationService {
  private baseUrl = 'http://localhost:5039/api/Station';

  constructor(private http: HttpClient) {}

  getStationIdByName(stationName: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/id?name=${stationName}`);
  }
}
