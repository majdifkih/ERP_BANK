import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService, LoginRequest } from '../../Service/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {
  username = '';
  password = '';
  errorMessage = '';
  loading = false;
  currentYear = new Date().getFullYear();

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  login(): void {
    this.errorMessage = '';
    this.loading = true;

    const request: LoginRequest = {
      username: this.username,
      password: this.password,
    };

    this.authService.login(request)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          if (error?.error?.message) {
            this.errorMessage = error.error.message;
          } else {
            this.errorMessage = 'Nom d’utilisateur ou mot de passe incorrect.';
          }
          console.error(error);
        },
      });
  }
}
