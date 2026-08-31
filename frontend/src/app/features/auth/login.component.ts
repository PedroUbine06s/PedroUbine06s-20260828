import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'carbon-components-angular/button';
import { InputModule } from 'carbon-components-angular/input';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, ButtonModule, InputModule],
  template: `
    <form class="cartao" [formGroup]="form" (ngSubmit)="entrar()">
      <div>
        <h1>Gestão de Colaboradores</h1>
        <p class="subtitulo">Entre para administrar unidades, colaboradores e usuários.</p>
      </div>

      <cds-label>
        Login
        <input cdsText formControlName="login" autocomplete="username" />
      </cds-label>

      <cds-label>
        Senha
        <input cdsText type="password" formControlName="senha" autocomplete="current-password" />
      </cds-label>

      @if (erro()) {
        <p class="erro" role="alert">{{ erro() }}</p>
      }

      <button cdsButton="primary" type="submit" [disabled]="form.invalid || carregando()">
        {{ carregando() ? 'Entrando…' : 'Entrar' }}
      </button>
    </form>
  `,
  styles: `
    .cartao {
      max-width: 24rem;
      margin: 10vh auto;
      background: var(--cds-layer, #fff);
      padding: 2rem;
      display: grid;
      gap: 1.5rem;
    }
    h1 {
      margin: 0 0 0.5rem;
      font-size: 1.5rem;
      font-weight: 400;
    }
    .subtitulo {
      margin: 0;
      font-size: 0.875rem;
      color: var(--cds-text-secondary, #525252);
    }
    .erro {
      margin: 0;
      font-size: 0.875rem;
      color: var(--cds-text-error, #da1e28);
    }
    button {
      width: 100%;
    }
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
        // O interceptor não notifica erros de login: a mensagem aparece no próprio formulário.
        this.erro.set(
          err.status === 429
            ? 'Muitas tentativas seguidas. Aguarde um minuto e tente de novo.'
            : (err.error?.detail ?? 'Login ou senha inválidos.')
        );
        this.carregando.set(false);
      }
    });
  }
}
