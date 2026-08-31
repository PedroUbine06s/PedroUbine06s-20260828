import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

/** COMPONENTE DE REFERÊNCIA — Reactive Forms + signals + tratamento de erro. */
@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  template: `
    <form class="cartao" [formGroup]="form" (ngSubmit)="entrar()">
      <h1>Entrar</h1>

      <label>Login
        <input formControlName="login" autocomplete="username" />
      </label>

      <label>Senha
        <input type="password" formControlName="senha" autocomplete="current-password" />
      </label>

      @if (erro()) {
        <p class="erro">{{ erro() }}</p>
      }

      <button type="submit" [disabled]="form.invalid || carregando()">
        {{ carregando() ? 'Entrando…' : 'Entrar' }}
      </button>
    </form>
  `,
  styles: `
    .cartao { max-width: 340px; margin: 10vh auto; background: #fff; padding: 2rem; border-radius: .5rem; display: grid; gap: 1rem; }
    label { display: grid; gap: .25rem; font-size: .9rem; }
    input { padding: .5rem; border: 1px solid #ccd2d9; border-radius: .25rem; }
    .erro { color: #a12020; margin: 0; font-size: .9rem; }
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    login: ['', Validators.required],
    senha: ['', Validators.required]
  });

  entrar(): void {
    if (this.form.invalid) return;

    this.carregando.set(true);
    this.erro.set(null);

    const { login, senha } = this.form.getRawValue();
    this.auth.login(login, senha).subscribe({
      next: () => this.router.navigate(['/']),
      error: err => {
        this.erro.set(err.error?.detail ?? 'Login ou senha inválidos.');
        this.carregando.set(false);
      }
    });
  }
}
