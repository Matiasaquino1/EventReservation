import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User } from '../../core/models/user.model';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { environment } from '../../../enviroments/environment';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatPaginatorModule],
  template: `
    <h2>Administrar Usuarios</h2>
    <table mat-table [dataSource]="users">
      <ng-container matColumnDef="username">
        <th mat-header-cell *matHeaderCellDef>Username</th>
        <td mat-cell *matCellDef="let user">{{ user.username }}</td>
      </ng-container>
      <ng-container matColumnDef="email">
        <th mat-header-cell *matHeaderCellDef>Email</th>
        <td mat-cell *matCellDef="let user">{{ user.email }}</td>
      </ng-container>
      <ng-container matColumnDef="role">
        <th mat-header-cell *matHeaderCellDef>Rol</th>
        <td mat-cell *matCellDef="let user">{{ user.role }}</td>
      </ng-container>
      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef>Acciones</th>
        <td mat-cell *matCellDef="let user">
          <button mat-button (click)="promote(user.id)">Promover</button>
          <button mat-button color="warn" (click)="delete(user.id)">Eliminar</button>
        </td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
    </table>
    <mat-paginator [length]="total" [pageSize]="limit" (page)="onPageChange($event)"></mat-paginator>
  `
})
export class UsersComponent implements OnInit {
  users: User[] = [];
  displayedColumns = ['username', 'email', 'role', 'actions'];
  page = 1;
  limit = 10;
  total = 0;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.http.get<{ users: User[]; total: number }>(`${environment.apiUrl}/v1/users?page=${this.page}&limit=${this.limit}`)
      .subscribe(res => {
        this.users = res.users;
        this.total = res.total;
      });
  }

  promote(id: number) {
    this.http.post(`${environment.apiUrl}/Admin/promote/${id}`, {}).subscribe(() => this.loadUsers());
  }

  delete(id: number) {
    if (confirm('¿Eliminar usuario?')) {
      this.http.delete(`${environment.apiUrl}/v1/users/${id}`).subscribe(() => this.loadUsers());
    }
  }

  onPageChange(event: PageEvent) {
    this.page = event.pageIndex + 1;
    this.limit = event.pageSize;
    this.loadUsers();
  }
}