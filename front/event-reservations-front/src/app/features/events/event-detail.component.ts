import {
  Component,
  OnInit,
  ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EventService } from '../../core/services/event.service';
import { EventModel } from '../../core/models/event.model';
import { inject } from '@angular/core';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './event-detail.component.html',
  styleUrls: ['./event-detail.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})

export class EventDetailComponent implements OnInit {

  private cdr = inject(ChangeDetectorRef);

  event: EventModel | null = null;
  loading = true;
  notFound = false;
  constructor(
    private route: ActivatedRoute,
    private eventService: EventService
  ) {}

  ngOnInit(): void {
  const id = Number(this.route.snapshot.paramMap.get('id'));
  if (!id || Number.isNaN(id)) {
      this.notFound = true;
      this.loading = false;
      return;}

  this.eventService.getEvent(id).subscribe({
    next: (event) => {
      this.event = event;
      this.loading = false;
       
    },
    error: () => {
        this.event = null;
        this.notFound = true;
        this.loading = false;
    },
  });
  }
}

