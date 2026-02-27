import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User } from '../models/user.model';
import { jwtDecode } from 'jwt-decode';


@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/api/Auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    const token = localStorage.getItem('token');
    if (token) {
      const user = this.decodeUser(token);
      if (user) {
        this.currentUserSubject.next(user);
      }
    }
  }

  login(credentials: { email: string; password: string }) {
    return this.http.post(`${this.baseUrl}/login`, credentials).pipe(
      tap((res: any) => {
        const token = res.token ?? res.Token;
        if (!token) return;
        localStorage.setItem('token', token);

        const user = this.decodeUser(token);
        if (user) {
          this.currentUserSubject.next(user);
        }
      })
    );
  }

  register(user: { name: string; email: string; password: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, user);
  }

  logout(): void {
    localStorage.removeItem('token');
    this.currentUserSubject.next(null);
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.role === role;
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('token');

    if (!token) return false;

    try {
      const decoded = jwtDecode<{ exp?: number }>(token);

      if (!decoded.exp) return false;

      const now = Math.floor(Date.now() / 1000);

      if (decoded.exp <= now) {
        localStorage.removeItem('token');
        return false;
      }

      return true;
    } catch (error) {
      localStorage.removeItem('token');
      return false;
    }
  } 

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  private decodeUser(token: string): User | null {
    try {
      const decoded: any = jwtDecode(token);
      const id =
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
        decoded['nameid'] ??
        decoded['sub'];
      const email =
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ??
        decoded['email'] ??
        '';
      const username =
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ??
        decoded['unique_name'] ??
        decoded['name'] ??
        email;
      const role =
        decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
        decoded['role'] ??
        'User';

      if (!id) return null;

      return {
        id: Number(id),
        username,
        email,
        role
      };
    } catch {
      return null;
    }
  }
}
