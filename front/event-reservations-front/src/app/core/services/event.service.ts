import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EventModel } from '../models/event.model';
import { EventFilters } from '../models/event-filters.model';
import { PagedEvents } from '../models/paged-events.model';

@Injectable({ providedIn: 'root' })
export class EventService {

  private readonly apiUrl = `${environment.apiUrl}/api/Events`;

  constructor(private http: HttpClient) {}

  getEvents(filters: EventFilters): Observable<EventModel[]> {
    let params = new HttpParams();

    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<EventModel[]>(this.apiUrl, { params });
  }

  getEvent(id: number) {
    return this.http.get<EventModel>(`${this.apiUrl}/${id}`);
  }

  createEvent(event: Partial<EventModel>) {
    return this.http.post<EventModel>(this.apiUrl, event);
  }

  updateEvent(id: number, event: Partial<EventModel>) {
    return this.http.put<EventModel>(`${this.apiUrl}/${id}`, event);
  }

  deleteEvent(id: number) {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
