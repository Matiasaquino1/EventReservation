import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  template: `
    <section class="auth">
      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-card">
        <h1>Crea tu cuenta</h1>
        <p class="subtitle">Empieza a reservar eventos en minutos.</p>

        <label>
          Nombre de usuario
          <input formControlName="name" placeholder="Tu nombre">
        </label>

        <label>
          Email
          <input formControlName="email" placeholder="correo@ejemplo.com" type="email">
        </label>

        <label>
          Contraseña
          <input formControlName="password" type="password" placeholder="********">
        </label>

        <button type="submit" [disabled]="form.invalid || loading">
          {{ loading ? 'Registrando...' : 'Registrarme' }}
        </button>

        <p class="error" *ngIf="error">{{ error }}</p>
        <p class="success" *ngIf="success">Cuenta creada. Redirigiendo...</p>
      </form>
    </section>
  `,
  styles: [`
    .auth {
      display: flex;
      justify-content: center;
      padding: 2rem 1rem;
    }
    .auth-card {
      width: 100%;
      max-width: 440px;
      background: #ffffff;
      padding: 2rem;
      border-radius: 16px;
      box-shadow: 0 16px 40px rgba(15, 23, 42, 0.12);
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    h1 {
      margin-bottom: 0;
    }
    .subtitle {
      margin-top: 0;
      color: #64748b;
    }
    label {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
      font-weight: 600;
    }
    input {
      padding: 0.65rem 0.75rem;
      border-radius: 10px;
      border: 1px solid #d8dde4;
      background: #f9fafc;
    }
    button {
      margin-top: 0.5rem;
      padding: 0.75rem;
      background: #1976d2;
      color: #ffffff;
      border: none;
      border-radius: 10px;
      font-weight: 600;
      cursor: pointer;
    }
    button:disabled {
      background: #9bb7d6;
      cursor: not-allowed;
    }
    .error {
      color: #c62828;
      font-weight: 600;
      margin: 0;
    }
    .success {
      color: #2e7d32;
      font-weight: 600;
      margin: 0;
    }
  `]
})
export class RegisterComponent {
  form: FormGroup;
  error = '';
  loading = false;
  success = false;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.error = '';
    this.loading = true;
    this.success = false;
    const payload = {
      name: this.form.value.name,
      email: this.form.value.email,
      password: this.form.value.password
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.success = true;
        this.loading = false;
        this.router.navigate(['/login']);
      },
      error: () => {
        this.error = 'No pudimos crear la cuenta. Verifica los datos.';
        this.loading = false;
      }
    });
  }

}
