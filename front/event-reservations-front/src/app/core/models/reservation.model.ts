import { User } from './user.model';
import { EventModel } from './event.model';
import { Payment } from './payment.model';

export interface Reservation {
  reservationId: number;
  userId: number;
  eventId: number;
  status?: string;
  reservationDate: string;
  createdAt: string;
  numberOfTickets: number;
  user?: User;
  event?: EventModel;
  payments?: Payment[];
}
