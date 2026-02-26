import { Component, inject, signal, viewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { loadStripe } from '@stripe/stripe-js';
import type { Stripe, StripeElements, StripeCardElement } from '@stripe/stripe-js';
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

  // Referencia al div del HTML donde se montará la tarjeta
  cardElementRef = viewChild<ElementRef>('cardElement');

  // Estados con Signals
  stripe: Stripe | null = null;
  elements: StripeElements | null = null;
  card: StripeCardElement | null = null;
  
  isProcessing = signal(false);
  errorMessage = signal<string | null>(null);
  clientSecret = signal<string | null>(null);

  async ngOnInit() {
    // 1. Inicializar Stripe con tu Public Key (la de TEST)
    this.stripe = await loadStripe('tu_pk_test_XXXXXX');
    
    // 2. Obtener el clientSecret desde tu controlador de .NET
    const reservationId = Number(this.route.snapshot.queryParamMap.get('reservationId'));
    
    this.reservationService.createPaymentIntent(reservationId).subscribe({
      next: (res: any) => {
        this.clientSecret.set(res.clientSecret);
        this.mountCardElement();
      },
      error: () => this.errorMessage.set('Error al inicializar el pago.')
    });
  }

  mountCardElement() {
    if (!this.stripe || !this.cardElementRef()) return;

    this.elements = this.stripe.elements();
    this.card = this.elements.create('card', {
      style: {
        base: {
          fontSize: '16px',
          color: '#1e293b',
          '::placeholder': { color: '#94a3b8' }
        }
      }
    });
    this.card.mount(this.cardElementRef()?.nativeElement);
  }

  async handleSubmit() {
    if (!this.stripe || !this.card || !this.clientSecret()) return;

    this.isProcessing.set(true);

    const { error, paymentIntent } = await this.stripe.confirmCardPayment(this.clientSecret()!, {
      payment_method: {
        card: this.card,
        billing_details: {
          // Podrías pasar el nombre del usuario desde tu AuthService
          name: 'Usuario Comprador' 
        }
      }
    });

    if (error) {
      this.errorMessage.set(error.message || 'El pago falló');
      this.isProcessing.set(false);
    } else if (paymentIntent.status === 'succeeded') {
      // El Webhook en .NET ya está trabajando en segundo plano.
      // Nosotros llevamos al usuario a una página de éxito.
      this.router.navigate(['/success']);
    }
  }
}