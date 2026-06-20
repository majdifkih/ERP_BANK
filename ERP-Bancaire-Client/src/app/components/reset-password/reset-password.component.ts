import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../Service/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent {

  newPassword = '';
  confirmPassword = '';
  token = '';
  message = '';
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    private router: Router
  ) {
    this.token = this.route.snapshot.queryParams['token'] || '';
  }

  submit() {

    this.message = '';

    if (!this.token) {
      this.message = "Token manquant ou invalide";
      return;
    }

    if (!this.newPassword || !this.confirmPassword) {
      this.message = "Veuillez remplir tous les champs";
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.message = "Les mots de passe ne correspondent pas";
      return;
    }

    this.loading = true;

    this.authService.resetPassword(this.token, this.newPassword)
      .subscribe({
        next: () => {
          this.loading = false;
          this.message = "Mot de passe modifié avec succès";

          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 1000);
        },
        error: (err) => {
          this.loading = false;
          this.message = err.error?.message || "Erreur serveur";
        }
      });
  }
}