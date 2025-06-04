import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

// export interface PassengerDTO {
//   name: string;
//   gender: string;
//   age: number;
// }

// export interface FareResponseDTO {
//   fare: number;
//   classTypeId: number;
//   totalSeats: number;
//   availableSeats: number;
//   passengerCount: number;
//   isAvailable: boolean;
// }

@Injectable({
  providedIn: 'root',
})
export class TicketService {
  private apiUrl = 'http://localhost:5039/api/Ticket';
  private paymentApi = 'http://localhost:5039/api/Payment';

  constructor(private http: HttpClient) {}

  private getHeaders() {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    });
  }

  initiateBooking(payload: any): Observable<any> {
    console.log(payload);
    return this.http.post(`${this.apiUrl}/initiate`, payload, {
      headers: this.getHeaders(),
    });
  }

  confirmPayment(paymentData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/confirm-payment`, paymentData, {
      headers: this.getHeaders(),
    });
  }

  createRazorpayOrder(amount: number): Observable<any> {
    return this.http.post(
      `${this.paymentApi}/create-order?amount=${amount}`,
      {},
      { headers: this.getHeaders() }
    );
  }
  getUserTickets(username: string): Observable<any[]> {
    return this.http.post<any[]>(
      `${this.apiUrl}/user-tickets`,
      { username },
      { headers: this.getHeaders() }
    );
  }
  getTicketById(ticketId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/details/${ticketId}`, {
      headers: this.getHeaders(),
    });
  }

  cancelTicket(body:any):Observable<any>{
    return this.http.post<any>(`${this.apiUrl}/cancel-passenger`, body,{
      headers:this.getHeaders(),
    });
  }
}
