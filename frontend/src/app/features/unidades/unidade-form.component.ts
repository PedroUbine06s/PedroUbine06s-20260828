import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'carbon-components-angular/button';
import { CheckboxModule } from 'carbon-components-angular/checkbox';
import { InputModule } from 'carbon-components-angular/input';
import { ModalModule } from 'carbon-components-angular/modal';
import { Unidade } from '../../core/models/modelos';
import { NotificacaoService } from '../../core/services/notificacao.service';
import { UnidadesService } from './unidades.service';

/**
 * Formulário de unidade em modal, servindo criação e edição.
 *
 * Na criação a API aceita apenas o nome — o código é gerado — então o campo
 * de status só aparece na edição, onde o PUT exige nome e ativo.
 */
@Component({
  selector: 'app-unidade-form',
  imports: [ReactiveFormsModule, ModalModule, ButtonModule, InputModule, CheckboxModule],
  template: `
    <cds-modal [open]="aberto()" size="sm" (close)="cancelado.emit()">
      <cds-modal-header (closeSelect)="cancelado.emit()">
        <h3 cdsModalHeaderHeading>{{ unidade() ? 'Editar unidade' : 'Nova unidade' }}</h3>
      </cds-modal-header>

      <section cdsModalContent>
        <form [formGroup]="form" class="campos">
          <cds-label
            [invalid]="nomeInvalido()"
            invalidText="Informe o nome da unidade."
            [helperText]="unidade() ? '' : 'O código é gerado pelo sistema (UNI000001).'">
            Nome
            <input cdsText formControlName="nome" autocomplete="off" />
          </cds-label>

          @if (unidade()) {
            <cds-checkbox formControlName="ativo">Ativa</cds-checkbox>
            <p class="aviso">
              Uma unidade inativa deixa de aceitar novos colaboradores.
            </p>
          }
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
    .aviso { margin: 0; font-size: .75rem; color: var(--cds-text-secondary, #525252); }
  `
})
export class UnidadeFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(UnidadesService);
  private readonly notificacao = inject(NotificacaoService);

  /** `null` significa criação. */
  readonly unidade = input<Unidade | null>(null);
  readonly aberto = input.required<boolean>();

  readonly salvo = output<void>();
  readonly cancelado = output<void>();

  readonly salvando = signal(false);

  readonly form = this.fb.nonNullable.group({
    nome: ['', Validators.required],
    ativo: [true]
  });

  constructor() {
    effect(() => {
      const atual = this.unidade();
      this.aberto();

      this.form.reset({ nome: atual?.nome ?? '', ativo: atual?.ativo ?? true });
    });
  }

  nomeInvalido(): boolean {
    const nome = this.form.controls.nome;

    return nome.invalid && nome.touched;
  }

  salvar(): void {
    if (this.form.invalid) return;

    this.salvando.set(true);
    const { nome, ativo } = this.form.getRawValue();
    const atual = this.unidade();

    const requisicao = atual
      ? this.service.atualizar(atual.id, { nome, ativo })
      : this.service.criar({ nome });

    requisicao.subscribe({
      next: () => {
        this.notificacao.sucesso(atual ? 'Unidade atualizada.' : 'Unidade cadastrada.');
        this.salvando.set(false);
        this.salvo.emit();
      },
      error: () => this.salvando.set(false)
    });
  }
}
