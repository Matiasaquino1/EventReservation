import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatExpansionModule } from '@angular/material/expansion';

import { User } from '../../../core/models/user.model';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatPaginatorModule,
    MatExpansionModule
  ],
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.css']
})
export class UsersComponent implements OnInit {

  users: any[] = [];
  displayedColumns = ['username', 'email', 'tickets', 'role', 'actions'];

  page = 1;
  limit = 10;
  total = 0;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.adminService.getUsers(this.page, this.limit)
      .subscribe(res => {
        this.users = res.users;
        this.total = res.total;
      });
  }

  promote(userId: number): void {
    this.adminService.promoteUser(userId)
      .subscribe(() => this.loadUsers());
  }

  remove(userId: number): void {
    if (!confirm('¿Eliminar usuario?')) return;

    this.adminService.deleteUser(userId)
      .subscribe(() => this.loadUsers());
  }

  getEventTitles(user: any): string {
    return user.reservations
      .map((r: any) => r.eventTitle || r.eventId)
      .join(', ');
  }

  onPageChange(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.limit = event.pageSize;
    this.loadUsers();
  }
}
