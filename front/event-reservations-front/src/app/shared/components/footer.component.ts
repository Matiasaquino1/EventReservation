import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  standalone: true,
  template: `
    <footer>
      <p>&copy; 2024 Event Reservations. Todos los derechos reservados.</p>
    </footer>
  `,
  styles: ['footer { text-align: center; padding: 1rem; background: #f5f7fa; }']
})
export class FooterComponent {}