import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="dashboard">
      <h1>Tableau de bord</h1>
      <p>Bienvenue dans votre espace ERP bancaire.</p>
      <button (click)="logout()">Déconnexion</button>
    </section>
  `,
  styles: [
    `
      .dashboard {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        min-height: 100vh;
        padding: 24px;
      }

      .dashboard button {
        margin-top: 24px;
        padding: 10px 16px;
        border: none;
        border-radius: 8px;
        background: #0078d4;
        color: white;
        cursor: pointer;
      }
    `,
  ],
})
export class DashboardComponent {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
