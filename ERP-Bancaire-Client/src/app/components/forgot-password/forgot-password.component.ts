import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../Service/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
})
export class ForgotPasswordComponent {
  email = '';
  message = '';
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

submit(): void {
  this.message = '';
  this.loading = true;

  this.authService.forgotPassword(this.email).subscribe({
    next: () => {
      this.message =
        'Si ce compte existe, un email de réinitialisation a été envoyé.';
      this.loading = false;
    },
    error: () => {
      this.message =
        'Si ce compte existe, un email de réinitialisation a été envoyé.';
      this.loading = false;
    },
  });
}
}
