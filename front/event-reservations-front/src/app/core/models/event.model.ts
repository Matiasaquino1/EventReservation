import { Reservation } from './reservation.model';

export interface EventModel {
  eventId: number;
  title: string;
  description?: string;
  createdAt: string;
  eventDate?: string;
  location: string;
  status: string;
  price: number;
  ticketsAvailable: number;
  totalTickets: number;
  reservations?: Reservation[];
}
