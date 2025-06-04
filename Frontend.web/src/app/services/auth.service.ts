import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

interface LoginDTO {
  username: string;
  password: string;
}

interface AuthResponseDTO {
  token: string;
  email: string;
  username: string;
  // role: string|null;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'http://localhost:5039/api/Auth';

  constructor(private http: HttpClient) {}

  login(data: LoginDTO): Observable<AuthResponseDTO> {
    return this.http.post<AuthResponseDTO>(`${this.apiUrl}/login`, data);
  }

  register(data: any): Observable<AuthResponseDTO> {
    return this.http.post<AuthResponseDTO>(`${this.apiUrl}/register`, data);
  }

  forgotPassword(email:string):Observable<any>{
    return this.http.post(`${this.apiUrl}/forgot-password`, { email },{ responseType: 'text' });
  }

  resetPassword(payload:any):Observable<any>{
    console.log(payload)
    return this.http.post(`${this.apiUrl}/reset-password`, payload ,{ responseType: 'text' });
  }

  isLoggedIn(): boolean {
    const token = localStorage.getItem('token');
    if (!token) {
      return false;
    }

    const payload = this.decodeToken(token);
    if (!payload) {
      return false;
    }

    const expiry = payload.exp;
    if (!expiry) {
      return false;
    }

    const now = Math.floor(Date.now() / 1000); 
    return expiry > now;
  }

  // Helper method to decode token
  private decodeToken(token: string): any {
    try {
      const payload = token.split('.')[1];
      const decodedPayload = atob(payload);
      return JSON.parse(decodedPayload);
    } catch (error) {
      console.error('Error decoding token', error);
      return null;
    }
  }

  getCurrentUser() {
    const token = localStorage.getItem('token');
    if (!token) return null;
  
    const payload = this.decodeToken(token);
    console.log(payload);
  
    // Accessing claims using their URI-based keys
    const username = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
    const email = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
    const role=payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  
    return username && email && role ? { username, email, role } : null;
  }

  isAdmin(): boolean {
    const user = this.getCurrentUser();
    return user?.role === 'Admin';
  }

  logout() {
    localStorage.clear();
  }
  
}
