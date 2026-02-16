import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  template: `
    <section class="auth">
      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-card">
        <h1>Bienvenido de nuevo</h1>
        <p class="subtitle">Ingresa para gestionar tus reservas.</p>

        <label>
          Email
          <input formControlName="email" placeholder="correo@ejemplo.com" type="email">
        </label>

        <label>
          Contraseña
          <input formControlName="password" type="password" placeholder="********">
        </label>

        <button type="submit" [disabled]="form.invalid || loading">
          {{ loading ? 'Ingresando...' : 'Ingresar' }}
        </button>

        <p class="error" *ngIf="error">{{ error }}</p>
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
      max-width: 420px;
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
  `]
})
export class LoginComponent {
  form: FormGroup;
  error = '';
  loading = false;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.form = this.fb.group({ email: ['', Validators.required], password: ['', Validators.required] });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.error = '';
    this.loading = true;
    this.authService.login(this.form.value).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/']);
      },
      error: () => {
        this.error = 'Credenciales inválidas. Intenta nuevamente.';
        this.loading = false;
      }
    });
  }
}
