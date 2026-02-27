import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-success',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './success.component.html',
  styleUrls: ['./success.component.css']
})
export class SuccessComponent implements OnInit {
  private router = inject(Router);

  showTicket = signal(false);

  ngOnInit() {
    setTimeout(() => this.showTicket.set(true), 100);
    localStorage.removeItem('pending_payment_reservation_id');
  }
}