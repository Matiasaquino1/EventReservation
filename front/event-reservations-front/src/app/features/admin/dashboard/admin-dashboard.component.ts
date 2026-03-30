import {
  Component,
  OnInit,
  OnDestroy,
  viewChild,
  ElementRef,
  inject,
  signal,
  effect
} from '@angular/core';
import { Chart, registerables } from 'chart.js';
import { CommonModule, DatePipe } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';
import { DashboardStats } from '../../../core/models/admin-stats.model';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

Chart.register(...registerables);

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  private adminService = inject(AdminService);

  stats      = signal<DashboardStats | null>(null);
  isLoading  = signal(true);
  hasError   = signal(false);
  today      = new Date();

  chartCanvas = viewChild<ElementRef<HTMLCanvasElement>>('eventChart');
  private chart: Chart | null = null;

  constructor() {
    effect(() => {
      const data = this.stats();
      const canvas = this.chartCanvas();
      
      if (data && canvas) {
        setTimeout(() => this.createChart(data.topEvents), 50);
      }
    });
  }

  ngOnInit() {
    this.adminService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.hasError.set(true);
      },
    });
  }

  ngOnDestroy() {
    this.chart?.destroy();
  }

  createChart(topEvents: { eventName: string; ticketCount: number }[]) {
    const canvasEl = this.chartCanvas();
    if (!canvasEl) return;

    const ctx = canvasEl.nativeElement.getContext('2d');
    if (!ctx) return;

    this.chart?.destroy();

    // Gradient — violet → blue, suits the light glassmorphism theme
    const grad = ctx.createLinearGradient(0, 0, 0, 320);
    grad.addColorStop(0, 'rgba(124, 58, 237, 0.85)');
    grad.addColorStop(1, 'rgba(37, 99, 235, 0.55)');

    const gradHover = ctx.createLinearGradient(0, 0, 0, 320);
    gradHover.addColorStop(0, 'rgba(124, 58, 237, 1)');
    gradHover.addColorStop(1, 'rgba(37, 99, 235, 0.8)');

    this.chart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: topEvents.map((e) => e.eventName),
        datasets: [
          {
            label: 'Entradas Vendidas',
            data: topEvents.map((e) => e.ticketCount),
            backgroundColor: grad,
            hoverBackgroundColor: gradHover,
            borderWidth: 0,
            borderRadius: 8,
            borderSkipped: false,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: { duration: 800, easing: 'easeOutQuart' },
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: 'rgba(255, 255, 255, 0.95)',
            titleColor: '#7c3aed',
            bodyColor: '#475569',
            borderColor: 'rgba(124, 58, 237, 0.2)',
            borderWidth: 1,
            padding: 12,
            cornerRadius: 10,
            titleFont: { size: 12, weight: 'bold' },
            bodyFont: { size: 13 },
            callbacks: {
              label: (ctx) => ` ${ctx.parsed.y} entradas`,
            },
          },
        },
        scales: {
          y: {
            beginAtZero: true,
            grid: { color: 'rgba(0, 0, 0, 0.05)' },
            border: { dash: [4, 4], color: 'transparent' },
            ticks: {
              color: '#94a3b8',
              font: { size: 11 },
              padding: 8,
            },
          },
          x: {
            grid: { display: false },
            border: { color: 'rgba(0,0,0,0.07)' },
            ticks: {
              color: '#94a3b8',
              font: { size: 11 },
              maxRotation: 25,
            },
          },
        },
      },
    });
  }

  getEventPercentage(
    count: number,
    topEvents: { ticketCount: number }[]
  ): number {
    const max = Math.max(...topEvents.map((e) => e.ticketCount));
    return max > 0 ? Math.round((count / max) * 100) : 0;
  }
}