import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EventService } from '../../../core/services/event.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-event-attendees',
  templateUrl: './event-attendees.component.html',
  styleUrls: ['./event-attendees.component.css'],
  imports: [CommonModule]
})

export class EventAttendeesComponent implements OnInit {
  attendees = signal<any[]>([]);
  loading = signal(false);
  eventId = 0;

  constructor(
    private route: ActivatedRoute,
    private eventService: EventService
  ) {}

  ngOnInit() {
    this.eventId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadAttendees();
  }

  loadAttendees() {
    this.loading.set(true);
    this.eventService.getEventAttendees(this.eventId).subscribe({
      next: (data) => {
        this.attendees.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}