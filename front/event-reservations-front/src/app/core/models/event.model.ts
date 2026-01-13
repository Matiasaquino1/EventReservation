import { Reservation } from './reservation.model';

export interface Event {
  eventId: number;
  title: string;
  description?: string;
  createdAt: Date;
  eventDate?: Date;
  location: string;
  status: string;
  price: number;
  ticketsAvailable: number;
  totalTickets: number;
  reservations?: Reservation[];
}
