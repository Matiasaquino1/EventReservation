import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EventModel } from '../models/event.model';
import { EventFilters } from '../models/event-filters.model';

@Injectable({ providedIn: 'root' })
export class EventService {

  private readonly apiUrl = 'https://localhost:7059/api/events';

  constructor(private http: HttpClient) {}

   getEvents(filters: EventFilters): Observable<EventModel[]> {

    let params = new HttpParams();

    if (filters.location) {
      params = params.set('location', filters.location);
    }

    if (filters.date) {
      params = params.set('date', filters.date);
    }

    if (filters.availability !== undefined) {
      params = params.set('availability', filters.availability);
    }

    return this.http.get<EventModel[]>(this.apiUrl, { params });
  }

  getEvent(id: number): Observable<EventModel> {
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

  private normalizeEvent(event: any): EventModel {
    return {
      eventId: Number(event?.eventId ?? event?.EventId ?? event?.id ?? 0),
      title: event?.title ?? event?.Title ?? '',
      description: event?.description ?? event?.Description,
      createdAt: event?.createdAt ?? event?.CreatedAt ?? '',
      eventDate: event?.eventDate ?? event?.EventDate,
      location: event?.location ?? event?.Location ?? '',
      status: event?.status ?? event?.Status ?? 'Active',
      price: Number(event?.price ?? event?.Price ?? 0),
      ticketsAvailable: Number(event?.ticketsAvailable ?? event?.TicketsAvailable ?? 0),
      totalTickets: Number(event?.totalTickets ?? event?.TotalTickets ?? 0),
      reservations: event?.reservations ?? event?.Reservations
    };
  }
}
