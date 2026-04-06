import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../app/core/services/auth.service';
import { CommonModule } from '@angular/common';
import { NgZone } from '@angular/core';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./auth.styles.css']
})

export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private zone = inject(NgZone);


  form: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  error = '';
  loading = false;

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.error = '';
    this.loading = true;

    this.authService.login(this.form.value).pipe(
      finalize(() => this.zone.run(() => this.loading = false))
    ).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        this.zone.run(() => {
          this.loading = false;
          this.error = err.status === 401
            ? 'Email o contraseña incorrectos.'
            : 'Error de conexión. Intentá de nuevo.';
        });
      }
    });
  }
} 