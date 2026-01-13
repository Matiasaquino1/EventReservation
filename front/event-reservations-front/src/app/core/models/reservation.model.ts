import { User } from './user.model';
import { Event } from './event.model';
import { Payment } from './payment.model';

export interface Reservation {
  reservationId: number;
  userId: number;
  eventId: number;
  status?: string;
  reservationDate: String;
  createdAt: String;
  numberOfTickets: number;
  user?: User;
  event?: Event;
  payments?: Payment[];
}
