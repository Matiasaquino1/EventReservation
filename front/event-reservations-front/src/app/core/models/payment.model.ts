import { Reservation } from './reservation.model';

export interface Payment {
  paymentId: number;
  reservationId: number;
  status: string;
  amount: number;
  paymentDate: Date;
  stripePaymentIntentId: string;
  reservation?: Reservation;
}
