import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Payment } from '../models/payment.model';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly apiUrl = `${environment.apiUrl}/Payments`;
  constructor(private http: HttpClient) {}
  processPayment(paymentData: { reservationId: number; amount: number; stripeToken: string }): Observable<Payment> {
    return this.http.post<Payment>(`${this.apiUrl}/process`, paymentData);
  }

  createPaymentIntent(amount: number): Observable<{ clientSecret: string }> {
    return this.http.post<{ clientSecret: string }>(`${this.apiUrl}/Payments/create-payment-intent`, { amount });
  }

  getPaymentHistory(): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.apiUrl}/Payments/history`);
  }

}