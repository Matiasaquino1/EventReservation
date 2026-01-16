import { EventModel } from './event.model';

export interface PagedEvents {
  events: EventModel[];
  total: number;
}
