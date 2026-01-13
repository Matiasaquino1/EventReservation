import { Event } from './event.model';

export interface PagedEvents {
  events: Event[];
  total: number;
}
