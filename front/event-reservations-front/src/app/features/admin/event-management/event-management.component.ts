import { Component, OnInit, signal } from '@angular/core';
import { EventService } from '../../../core/services/event.service'; 
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-event-management',
  imports: [CommonModule, RouterModule],
  templateUrl: './event-management.component.html',
  styleUrls: ['./event-management.component.css']
})
export class EventManagementComponent implements OnInit {
  events = signal<any[]>([]);
  loading = signal(false);

  constructor(
    private eventService: EventService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadEvents();
  }

  loadEvents() {
    this.loading.set(true);
    this.eventService.getAllEvents().subscribe({
      next: (data) => {
        this.events.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  deleteEvent(id: number) {
    if (confirm('¿Estás seguro de eliminar este evento? Esta acción no se puede deshacer.')) {
      this.eventService.deleteEvent(id).subscribe({
        next: () => {
          this.events.update(list => list.filter(e => e.eventId !== id));
        },
        error: (err) => alert('Error al eliminar el evento.')
      });
    }
  }

  goToAttendees(id: number) {
    this.router.navigate(['/admin/events', id, 'attendees']);
  }

  goToEdit(id: number) {
    this.router.navigate(['/admin/events/edit', id]);
  }
}