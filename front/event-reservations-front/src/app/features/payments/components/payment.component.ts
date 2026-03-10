import { Component, inject, signal, viewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { loadStripe } from '@stripe/stripe-js';
import type { Stripe, StripeElements, StripeCardNumberElement, StripeCardExpiryElement, StripeCardCvcElement } from '@stripe/stripe-js';
import { ReservationService } from '../../../core/services/reservation.service';

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
  
  cardNumber: StripeCardNumberElement | null = null;
  cardExpiry: StripeCardExpiryElement | null = null;
  cardCvc: StripeCardCvcElement | null = null;

  reservationId = signal<number | null>(null);
  stripePaymentIntentId = signal<string | null>(null); 
  isProcessing = signal(false);
  errorMessage = signal<string | null>(null);
  clientSecret = signal<string | null>(null);

  async ngOnInit() {
    this.stripe = await loadStripe('pk_test_51SINgXCFUSVETYTsnDEvXAhTT5HIsqXpFH1MHSVIIetaqnWUVXoo3VfHXVXlKwfL6TB7CqFAQQnWyF8awDjW1JVK00lNk0ptpn');

    const idFromUrl = this.route.snapshot.queryParamMap.get('reservationId');
    const id = Number(idFromUrl);

    if (!id || isNaN(id)) {
      this.errorMessage.set('No se encontró un ID de reserva válido.');
      return;
    }
    this.reservationId.set(id);

    this.reservationService.getPaymentIntent(id).subscribe({
      next: (res: any) => {
        const secret = res.clientSecret || res.ClientSecret;     
        if (secret) {
          this.clientSecret.set(secret);
          this.mountElements();
        } else {
          this.errorMessage.set('El servidor no devolvió el secreto de pago.');
        }
      },
      error: (err) => {
        this.errorMessage.set('Error al recuperar el intento de pago.');
        console.error('Error en getPaymentIntent:', err);
      }
    });
  }

  mountElements() {
    if (!this.stripe || !this.clientSecret()) return;

    this.elements = this.stripe.elements({ clientSecret: this.clientSecret()! });
    const style = {
      base: {
        fontSize: '16px',
        color: '#1e293b',
        fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
        '::placeholder': { color: '#94a3b8' }
      }
    };

    this.cardNumber = this.elements.create('cardNumber', { style });
    this.cardNumber.mount(this.cardNumberRef()?.nativeElement);

    this.cardExpiry = this.elements.create('cardExpiry', { style });
    this.cardExpiry.mount(this.cardExpiryRef()?.nativeElement);

    this.cardCvc = this.elements.create('cardCvc', { style });
    this.cardCvc.mount(this.cardCvcRef()?.nativeElement);
  }

  async confirmPayment() {
    if (!this.stripe || !this.cardNumber || !this.clientSecret()) return;

    this.isProcessing.set(true);
    this.errorMessage.set(null);

    const { paymentIntent: existingPI, error: retrieveError } = 
      await this.stripe.retrievePaymentIntent(this.clientSecret()!);

    if (retrieveError) {
      this.errorMessage.set(retrieveError.message || 'Error al verificar el pago.');
      this.isProcessing.set(false);
      return;
    }

    if (existingPI?.status === 'succeeded') {
      this.stripePaymentIntentId.set(existingPI.id);
      this.finalizeReservation();
      return;
    }

    const { error, paymentIntent } = await this.stripe.confirmCardPayment(this.clientSecret()!, {
      payment_method: {
        card: this.cardNumber!,
        billing_details: { name: 'Usuario Comprador' }
      }
    });

    if (error) {
      this.errorMessage.set(error.message || 'El pago falló');
      this.isProcessing.set(false);
    } else if (paymentIntent?.status === 'succeeded') {
      this.stripePaymentIntentId.set(paymentIntent.id);
      this.finalizeReservation();
    }
  }

  private finalizeReservation() {
    const id = this.reservationId();
    const piId = this.stripePaymentIntentId(); 

    if (!id || !piId) {
      this.errorMessage.set('Error interno: Faltan datos de confirmación.');
      this.isProcessing.set(false);
      return;
    }

    const confirmDto = { paymentIntentId: piId };

    this.reservationService.confirmReservation(id, confirmDto).subscribe({
      next: () => {
        this.isProcessing.set(false);
        this.router.navigate(['/success']);
      },
      error: (err) => {
        console.error('Error en confirmación:', err);
        this.errorMessage.set(err.error?.message || 'Pago exitoso, pero hubo un error al actualizar el stock.');
        this.isProcessing.set(false);
      }
    });
  }
}