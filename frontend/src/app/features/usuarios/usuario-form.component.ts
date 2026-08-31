import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'carbon-components-angular/button';
import { CheckboxModule } from 'carbon-components-angular/checkbox';
import { InputModule } from 'carbon-components-angular/input';
import { ModalModule } from 'carbon-components-angular/modal';
import { Usuario } from '../../core/models/modelos';
import { NotificacaoService } from '../../core/services/notificacao.service';
import { UsuariosService } from './usuarios.service';

const MINIMO_SENHA = 8;

/**
 * Formulário de usuário em modal, servindo criação e edição.
 *
 * Na edição o contrato da API só aceita senha e status, então o login aparece
 * desabilitado: a tela espelha a restrição em vez de oferecer um campo que
 * seria descartado no servidor.
 */
@Component({
  selector: 'app-usuario-form',
  imports: [ReactiveFormsModule, ModalModule, ButtonModule, InputModule, CheckboxModule],
  template: `
    <cds-modal [open]="aberto()" size="sm" (close)="cancelado.emit()">
      <cds-modal-header (closeSelect)="cancelado.emit()">
        <h3 cdsModalHeaderHeading>{{ usuario() ? 'Editar usuário' : 'Novo usuário' }}</h3>
      </cds-modal-header>

      <section cdsModalContent>
        <form [formGroup]="form" class="campos">
          <cds-label
            [helperText]="usuario() ? 'O login não é alterável por contrato da API.' : ''">
            Login
            <input cdsText formControlName="login" autocomplete="off" />
          </cds-label>

          <cds-label
            [invalid]="senhaInvalida()"
            [invalidText]="'A senha precisa de pelo menos ' + minimoSenha + ' caracteres.'"
            [helperText]="usuario() ? 'Deixe em branco para manter a senha atual.' : ''">
            Senha
            <input cdsText type="password" formControlName="senha" autocomplete="new-password" />
          </cds-label>

          <cds-checkbox formControlName="ativo">Ativo</cds-checkbox>
        </form>
      </section>

      <cds-modal-footer>
        <button cdsButton="secondary" (click)="cancelado.emit()">Cancelar</button>
        <button cdsButton="primary" [disabled]="form.invalid || salvando()" (click)="salvar()">
          {{ salvando() ? 'Salvando…' : 'Salvar' }}
        </button>
      </cds-modal-footer>
    </cds-modal>
  `,
  styles: `
    .campos { display: grid; gap: 1.5rem; }
  `
})
export class UsuarioFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(UsuariosService);
  private readonly notificacao = inject(NotificacaoService);

  /** `null` significa criação. */
  readonly usuario = input<Usuario | null>(null);
  readonly aberto = input.required<boolean>();

  readonly salvo = output<void>();
  readonly cancelado = output<void>();

  readonly salvando = signal(false);
  readonly minimoSenha = MINIMO_SENHA;

  readonly form = this.fb.nonNullable.group({
    login: ['', Validators.required],
    senha: ['', [Validators.required, Validators.minLength(MINIMO_SENHA)]],
    ativo: [true]
  });

  constructor() {
    // Reage à troca de usuário (ou à reabertura do modal) recarregando o formulário.
    effect(() => {
      const atual = this.usuario();
      this.aberto();

      this.form.reset({ login: atual?.login ?? '', senha: '', ativo: atual?.ativo ?? true });

      const senha = this.form.controls.senha;
      // Na edição a senha é opcional; quando preenchida ainda precisa do tamanho mínimo.
      senha.setValidators(
        atual
          ? [Validators.minLength(MINIMO_SENHA)]
          : [Validators.required, Validators.minLength(MINIMO_SENHA)]
      );
      senha.updateValueAndValidity();

      if (atual) this.form.controls.login.disable();
      else this.form.controls.login.enable();
    });
  }

  senhaInvalida(): boolean {
    const senha = this.form.controls.senha;

    return senha.invalid && senha.touched;
  }

  salvar(): void {
    if (this.form.invalid) return;

    this.salvando.set(true);
    const { login, senha, ativo } = this.form.getRawValue();
    const atual = this.usuario();

    const requisicao = atual
      ? this.service.atualizar(atual.id, { senha: senha || undefined, ativo })
      : this.service.criar({ login, senha, ativo });

    requisicao.subscribe({
      next: () => {
        this.notificacao.sucesso(atual ? 'Usuário atualizado.' : 'Usuário cadastrado.');
        this.salvando.set(false);
        this.salvo.emit();
      },
      // A mensagem de erro já é exibida pelo interceptor.
      error: () => this.salvando.set(false)
    });
  }
}
