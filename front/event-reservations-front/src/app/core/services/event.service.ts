import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../enviroments/environment';
import { Event } from '../models/event.model';

@Injectable({ providedIn: 'root' })
export class EventService {
  getEvents(filters?: { date?: string; location?: string; minTickets?: number; maxPrice?: number }, page = 1, limit = 10): Observable<{ events: Event[]; total: number }> {
    let params = new HttpParams().set('page', page).set('limit', limit);
    if (filters?.date) params = params.set('date', filters.date);
    if (filters?.location) params = params.set('location', filters.location);
    if (filters?.minTickets) params = params.set('minTickets', filters.minTickets);
    if (filters?.maxPrice) params = params.set('maxPrice', filters.maxPrice);
    return this.http.get<{ events: Event[]; total: number }>(`${environment.apiUrl}/Events`, { params });
  }

  getEvent(id: number): Observable<Event> {
    return this.http.get<Event>(`${environment.apiUrl}/Events/${id}`);
  }

  createEvent(event: Partial<Event>): Observable<Event> {
    return this.http.post<Event>(`${environment.apiUrl}/Events`, event);
  }

  updateEvent(id: number, event: Partial<Event>): Observable<Event> {
    return this.http.put<Event>(`${environment.apiUrl}/Events/${id}`, event);
  }

  deleteEvent(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/Events/${id}`);
  }

  constructor(private http: HttpClient) {}
}