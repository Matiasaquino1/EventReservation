import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { EventService } from '../../../core/services/event.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './event-form.component.html',
  styleUrls: ['./event-form.component.css']
})
export class EventFormComponent implements OnInit {
  eventForm: FormGroup;
  isEditMode = signal(false);
  loading = signal(false);
  eventId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private eventService: EventService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.eventForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5)]],
      description: ['', Validators.required],
      eventDate: ['', Validators.required],
      location: ['', Validators.required],
      price: [0, [Validators.required, Validators.min(0)]],
      totalTickets: [0, [Validators.required, Validators.min(1)]],
      status: ['Active']
    });
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.eventId = Number(id);
      this.loadEventData(this.eventId);
    }
  }

  loadEventData(id: number) {
    this.loading.set(true);
    this.eventService.getEvent(id).subscribe({
      next: (event) => {
        // Formatear fecha para el input datetime-local
        const formattedDate = event.eventDate 
          ? new Date(event.eventDate).toISOString().slice(0, 16) 
          : '';
          
        this.eventForm.patchValue({
          ...event,
          eventDate: formattedDate
        });
        this.loading.set(false);
      },
      error: () => {
        alert('Error al cargar el evento');
        this.router.navigate(['/admin/management']);
      }
    });
  }

  onSubmit() {
    if (this.eventForm.invalid) return;

    this.loading.set(true);
    const formData = { ...this.eventForm.value };
    
    // Formato ISO para el backend
    formData.eventDate = new Date(formData.eventDate).toISOString();

    const request$ = this.isEditMode() 
      ? this.eventService.updateEvent(this.eventId!, formData)
      : this.eventService.createEvent(formData);

    request$.subscribe({
      next: () => {
        this.router.navigate(['/admin/management']);
      },
      error: (err) => {
        console.error(err);
        alert('Ocurrió un error al guardar.');
        this.loading.set(false);
      }
    });
  }
}