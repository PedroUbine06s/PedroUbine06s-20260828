import {
  Component,
  Injector,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  output,
  signal
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ButtonModule } from 'carbon-components-angular/button';
import { InputModule } from 'carbon-components-angular/input';
import { ModalModule } from 'carbon-components-angular/modal';
import { SelectModule } from 'carbon-components-angular/select';
import { Colaborador, Unidade, Usuario } from '../../core/models/modelos';
import { NotificacaoService } from '../../core/services/notificacao.service';
import { UnidadesService } from '../unidades/unidades.service';
import { UsuariosService } from '../usuarios/usuarios.service';
import { ColaboradoresService } from './colaboradores.service';

/** Teto de itens por página da API; basta para as listas de apoio deste portal. */
const TAMANHO_MAXIMO = 100;

/**
 * Formulário de colaborador em modal, servindo criação e edição.
 *
 * O select de unidades lista apenas as ativas, porque a API recusa com 422 um
 * colaborador em unidade inativa — a tela evita o erro em vez de esperar por ele.
 * A exceção é a unidade atual de quem está sendo editado: se ela foi inativada
 * depois do cadastro, precisa continuar visível, senão editar só o nome moveria
 * o colaborador de unidade sem querer.
 */
@Component({
  selector: 'app-colaborador-form',
  imports: [ReactiveFormsModule, ModalModule, ButtonModule, InputModule, SelectModule],
  template: `
    <cds-modal [open]="aberto()" size="sm" (close)="cancelado.emit()">
      <cds-modal-header closeLabel="Fechar" (closeSelect)="cancelado.emit()">
        <h3 cdsModalHeaderHeading>
          {{ colaborador() ? 'Editar colaborador' : 'Novo colaborador' }}
        </h3>
      </cds-modal-header>

      <section cdsModalContent>
        <form [formGroup]="form" class="campos">
          <cds-label
            [invalid]="nomeInvalido()"
            invalidText="Informe o nome do colaborador."
            [helperText]="colaborador() ? '' : 'O código é gerado pelo sistema (COL000001).'">
            Nome
            <input cdsText formControlName="nome" autocomplete="off" />
          </cds-label>

          <cds-select
            formControlName="unidadeId"
            label="Unidade"
            [helperText]="
              semUnidadeAtiva()
                ? 'Nenhuma unidade ativa. Ative uma unidade antes de cadastrar colaboradores.'
                : ''
            ">
            <option value="">Selecione uma unidade</option>
            @for (u of unidadesSelecionaveis(); track u.id) {
              <option [value]="u.id">
                {{ u.nome }}{{ u.ativo ? '' : ' (inativa)' }}
              </option>
            }
          </cds-select>

          @if (!colaborador()) {
            <cds-select
              formControlName="usuarioId"
              label="Usuário"
              helperText="Cada usuário pertence a um único colaborador.">
              <option value="">Selecione um usuário</option>
              @for (u of usuarios(); track u.id) {
                <option [value]="u.id">{{ u.login }} ({{ u.codigo }})</option>
              }
            </cds-select>
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
  `
})
export class ColaboradorFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ColaboradoresService);
  private readonly unidadesService = inject(UnidadesService);
  private readonly usuariosService = inject(UsuariosService);
  private readonly notificacao = inject(NotificacaoService);
  private readonly injector = inject(Injector);

  /** `null` significa criação. */
  readonly colaborador = input<Colaborador | null>(null);
  readonly aberto = input.required<boolean>();

  readonly salvo = output<void>();
  readonly cancelado = output<void>();

  readonly salvando = signal(false);
  readonly unidades = signal<Unidade[]>([]);
  readonly usuarios = signal<Usuario[]>([]);

  /**
   * Unidades ativas, mais a unidade atual do colaborador em edição caso ela
   * esteja inativa — do contrário o select perderia o valor já selecionado.
   */
  readonly unidadesSelecionaveis = computed(() => {
    const atual = this.colaborador();
    const ativas = this.unidades().filter(u => u.ativo);

    if (!atual) return ativas;
    if (ativas.some(u => u.id === atual.unidadeId)) return ativas;

    const inativa = this.unidades().find(u => u.id === atual.unidadeId);

    return inativa ? [inativa, ...ativas] : ativas;
  });

  /** Só bloqueia a criação: na edição a unidade atual continua disponível. */
  readonly semUnidadeAtiva = computed(
    () => !this.colaborador() && this.unidadesSelecionaveis().length === 0
  );

  readonly form = this.fb.nonNullable.group({
    nome: ['', Validators.required],
    unidadeId: ['', Validators.required],
    usuarioId: ['', Validators.required]
  });

  constructor() {
    effect(() => {
      const atual = this.colaborador();
      this.aberto();

      this.form.reset({
        nome: atual?.nome ?? '',
        unidadeId: atual?.unidadeId ?? '',
        usuarioId: ''
      });

      // O usuário só é informado na criação: o PUT aceita apenas nome e unidade.
      const usuario = this.form.controls.usuarioId;
      usuario.setValidators(atual ? [] : [Validators.required]);
      usuario.updateValueAndValidity();
    });


    this.carregarOpcoes();
  }

  nomeInvalido(): boolean {
    const nome = this.form.controls.nome;

    return nome.invalid && nome.touched;
  }

  salvar(): void {
    if (this.form.invalid) return;

    this.salvando.set(true);
    const { nome, unidadeId, usuarioId } = this.form.getRawValue();
    const atual = this.colaborador();

    const requisicao = atual
      ? this.service.atualizar(atual.id, { nome, unidadeId })
      : this.service.criar({ nome, unidadeId, usuarioId });

    requisicao.subscribe({
      next: () => {
        this.notificacao.sucesso(atual ? 'Colaborador atualizado.' : 'Colaborador cadastrado.');
        this.salvando.set(false);
        this.salvo.emit();
      },
      error: () => this.salvando.set(false)
    });
  }

  /** Só reaplica se a pessoa ainda não tiver escolhido outra unidade na tela. */
  private sincronizarUnidadeSelecionada(): void {
    const atual = this.colaborador();
    const unidade = this.form.controls.unidadeId;

    if (atual && unidade.pristine) unidade.setValue(atual.unidadeId);
  }

  private carregarOpcoes(): void {
    forkJoin({
      unidades: this.unidadesService.listar({ tamanho: TAMANHO_MAXIMO }),
      usuarios: this.usuariosService.listar({ ativo: true, tamanho: TAMANHO_MAXIMO })
    }).subscribe({
      next: ({ unidades, usuarios }) => {
        this.unidades.set(unidades.itens);
        this.usuarios.set(usuarios.itens);

        // O accessor do <select> escreve o valor no DOM antes de as <option>
        // existirem, e o navegador descarta um valor sem option correspondente:
        // a tela mostraria "Selecione uma unidade" com o formulário já apontando
        // para a unidade certa. Reaplicar depois da renderização realinha os dois.
        afterNextRender(() => this.sincronizarUnidadeSelecionada(), {
          injector: this.injector
        });
      },
      error: () => {
        /* O interceptor já notificou; os selects ficam vazios. */
      }
    });
  }
}
