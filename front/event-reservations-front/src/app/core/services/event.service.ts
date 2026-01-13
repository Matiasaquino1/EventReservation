import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../enviroments/environment';
import { Event } from '../models/event.model';
import { EventFilters } from '../models/event-filters.model';
import { PagedEvents } from '../models/paged-events.model';

@Injectable({ providedIn: 'root' })
export class EventService {

  private readonly apiUrl = '/api/events';

  constructor(private http: HttpClient) {}

  getEvents(filters: EventFilters): Observable<PagedEvents> {
    let params = new HttpParams();

    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<PagedEvents>(this.apiUrl, { params });
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
}