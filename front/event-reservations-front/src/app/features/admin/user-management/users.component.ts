import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    MatPaginatorModule,
    MatIconModule,
  ],
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss'],
})
export class UsersComponent implements OnInit {
  private adminService = inject(AdminService);
  private cdr = inject(ChangeDetectorRef);

  users: any[] = [];
  page  = 1;
  limit = 10;
  total = 0;
  isLoading = true;

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.adminService.getUsers(this.page, this.limit).subscribe({
      next: (res) => {
        this.users = res.users;
        this.total = res.total;
        this.isLoading = false;
        this.cdr.detectChanges(); // <--- ESTO obliga a Angular a pintar los datos
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  promote(userId: number): void {
    if (!confirm('¿Promover a este usuario a Admin?')) return;
    this.adminService.promoteUser(userId).subscribe(() => this.loadUsers());
  }

  remove(userId: number): void {
    if (!confirm('¿Eliminar usuario permanentemente?')) return;
    this.adminService.deleteUser(userId).subscribe(() => this.loadUsers());
  }

  onPageChange(event: PageEvent): void {
    this.page  = event.pageIndex + 1;
    this.limit = event.pageSize;
    this.loadUsers();
  }

  resPercent(count: number): number {
    return Math.min(Math.round((count / 10) * 100), 100);
  }
}
