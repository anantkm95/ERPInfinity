import { Injectable, signal, computed } from '@angular/core';
import { UserSession, UserRole } from '../models/erp-models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly STORAGE_KEY = 'erpinfinity_user';
  
  // Angular 19 Reactive Signals
  public currentUser = signal<UserSession | null>(this.loadInitialSession());

  public isAuthenticated = computed(() => this.currentUser() !== null);
  public userRole = computed(() => this.currentUser()?.role || null);

  private loadInitialSession(): UserSession | null {
    const raw = localStorage.getItem(this.STORAGE_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  public loginAsPersona(role: UserRole): UserSession {
    let user: UserSession;
    if (role === 'CEO') {
      user = {
        id: '88888888-8888-8888-8888-888888888888',
        username: 'admin.anant',
        displayName: 'Anant Kumar (CEO)',
        email: 'anant@erpinfinity.com',
        role: 'CEO',
        token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.CEO_ANGULAR_TOKEN'
      };
    } else if (role === 'Admin') {
      user = {
        id: '77777777-7777-7777-7777-777777777777',
        username: 'admin.mumbai',
        displayName: 'Mumbai Admin User',
        email: 'admin.mumbai@erpinfinity.com',
        role: 'Admin',
        token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.ADMIN_ANGULAR_TOKEN'
      };
    } else {
      user = {
        id: '66666666-6666-6666-6666-666666666666',
        username: 'cashier.counter1',
        displayName: 'Cashier Counter #01',
        email: 'cashier1@erpinfinity.com',
        role: 'Cashier',
        token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.CASHIER_ANGULAR_TOKEN'
      };
    }

    this.setSession(user);
    return user;
  }

  public setSession(user: UserSession): void {
    this.currentUser.set(user);
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(user));
  }

  public logout(): void {
    this.currentUser.set(null);
    localStorage.removeItem(this.STORAGE_KEY);
  }
}
