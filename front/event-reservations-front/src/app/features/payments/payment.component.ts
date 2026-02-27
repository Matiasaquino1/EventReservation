import { Component, inject, signal, viewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { loadStripe } from '@stripe/stripe-js';
import type { Stripe, StripeElements, StripeCardNumberElement, StripeCardExpiryElement, StripeCardCvcElement } from '@stripe/stripe-js';
import { ReservationService } from '../../core/services/reservation.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.css']
})
export class PaymentComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private reservationService = inject(ReservationService);

  // Referencias a los contenedores del DOM
  cardNumberRef = viewChild<ElementRef>('cardNumber');
  cardExpiryRef = viewChild<ElementRef>('cardExpiry');
  cardCvcRef = viewChild<ElementRef>('cardCvc');

  stripe: Stripe | null = null;
  elements: StripeElements | null = null;
  
  // Elementos individuales
  cardNumber: StripeCardNumberElement | null = null;
  cardExpiry: StripeCardExpiryElement | null = null;
  cardCvc: StripeCardCvcElement | null = null;

  reservationId: number = 0;
  isProcessing = signal(false);
  errorMessage = signal<string | null>(null);
  clientSecret = signal<string | null>(null);

  async ngOnInit() {
    this.stripe = await loadStripe('pk_test_51SINgXCFUSVETYTsnDEvXAhTT5HIsqXpFH1MHSVIIetaqnWUVXoo3VfHXVXlKwfL6TB7CqFAQQnWyF8awDjW1JVK00lNk0ptpn');
    
    this.reservationId = Number(this.route.snapshot.queryParamMap.get('reservationId'));

    if (!this.reservationId) {
      this.errorMessage.set('No se encontró el ID de la reserva.');
      return;
    }
    
    this.reservationService.createPaymentIntent(this.reservationId).subscribe({
      next: (res: any) => {
        this.clientSecret.set(res.clientSecret);
        this.mountElements();
      },
      error: () => this.errorMessage.set('Error al conectar con la pasarela de pago.')
    });
  }

  mountElements() {
    if (!this.stripe) return;

    this.elements = this.stripe.elements();
    const style = {
      base: {
        fontSize: '16px',
        color: '#1e293b',
        fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
        '::placeholder': { color: '#94a3b8' }
      }
    };

    // Crear y montar Número de tarjeta
    this.cardNumber = this.elements.create('cardNumber', { style });
    this.cardNumber.mount(this.cardNumberRef()?.nativeElement);

    // Crear y montar Fecha de Vencimiento
    this.cardExpiry = this.elements.create('cardExpiry', { style });
    this.cardExpiry.mount(this.cardExpiryRef()?.nativeElement);

    // Crear y montar CVC
    this.cardCvc = this.elements.create('cardCvc', { style });
    this.cardCvc.mount(this.cardCvcRef()?.nativeElement);
  }

  async confirmPayment() {
    if (!this.stripe || !this.cardNumber || !this.clientSecret()) return;

    this.isProcessing.set(true);
    this.errorMessage.set(null);

    const { error, paymentIntent } = await this.stripe.confirmCardPayment(this.clientSecret()!, {
      payment_method: {
        card: this.cardNumber!,
        billing_details: {
          name: 'Usuario Comprador' // Idealmente obtener de AuthService
        }
      }
    });

    if (error) {
      this.errorMessage.set(error.message || 'El pago falló');
      this.isProcessing.set(false);
    } else if (paymentIntent.status === 'succeeded') {
      this.finalizeReservation();
    }
  }

  private finalizeReservation() {
    // Llamamos al backend para actualizar el estado de la reserva
    this.reservationService.confirmReservation(this.reservationId).subscribe({
      next: () => {
        this.router.navigate(['/success']);
      },
      error: (err: any) => {
        console.error('Pago exitoso en Stripe, pero falló actualización en BD', err);
        this.errorMessage.set('Pago confirmado, pero hubo un error al actualizar tu reserva. Contacta a soporte.');
        this.isProcessing.set(false);
      }
    });
  }
}