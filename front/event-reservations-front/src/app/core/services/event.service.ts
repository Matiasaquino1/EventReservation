import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EventModel } from '../models/event.model';
import { EventFilters } from '../models/event-filters.model';

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

    return this.http.get<any[]>(this.apiUrl, { params }).pipe(
      map(events => events.map(event => this.normalizeEvent(event)))
    );
  }

  getEvent(id: number) {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(event => this.normalizeEvent(event))
    );
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
