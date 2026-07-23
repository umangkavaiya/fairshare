import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth-service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
})
export class Register {
  email = '';
  password = '';
  displayName = '';
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  onSubmit() {
    this.errorMessage = '';
    this.authService.register(this.email, this.password, this.displayName).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => (this.errorMessage = err.error?.message ?? 'Registration failed.'),
    });
  }
}
