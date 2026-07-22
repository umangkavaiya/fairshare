import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HealthService } from './core/services/health.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  protected readonly title = signal('fairshare-client');

  private health = inject(HealthService);

  ngOnInit() {
    this.health.check().subscribe({
      next: (res) => console.log('API health check:', res),
      error: (err) => console.error('API health check failed:', err),
    });
  }
}
