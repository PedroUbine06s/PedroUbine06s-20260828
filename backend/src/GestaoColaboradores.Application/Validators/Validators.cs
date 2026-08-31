using FluentValidation;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Domain.Common;
using GestaoColaboradores.Domain.Entidades;

namespace GestaoColaboradores.Application.Validators;

// Os limites de tamanho vêm das constantes do domínio: validação de entrada, invariante da
// entidade e schema do banco leem da mesma fonte.
//
// O que NÃO vem de lá são as regras sobre a senha em texto puro. Elas só podem existir aqui,
// porque uma camada abaixo a senha já virou hash e o texto original deixou de existir.

/// <summary>Regras da senha digitada, aplicáveis apenas na borda.</summary>
internal static class RegrasSenha
{
    public const int TamanhoMinimo = 8;

    /// <summary>O BCrypt ignora o que passa de 72 bytes, em silêncio. Recusar é melhor que truncar.</summary>
    public const int TamanhoMaximo = 72;
}

public class CriarUsuarioValidator : AbstractValidator<CriarUsuarioDto>
{
    public CriarUsuarioValidator()
    {
        RuleFor(x => x.Login).NotEmpty().MaximumLength(Usuario.TamanhoMaximoLogin);
        RuleFor(x => x.Senha)
            .NotEmpty()
            .MinimumLength(RegrasSenha.TamanhoMinimo)
            .MaximumLength(RegrasSenha.TamanhoMaximo);
    }
}

public class AtualizarUsuarioValidator : AbstractValidator<AtualizarUsuarioDto>
{
    public AtualizarUsuarioValidator()
    {
        RuleFor(x => x.Senha)
            .MinimumLength(RegrasSenha.TamanhoMinimo)
            .MaximumLength(RegrasSenha.TamanhoMaximo)
            .When(x => !string.IsNullOrWhiteSpace(x.Senha));
    }
}

public class CriarUnidadeValidator : AbstractValidator<CriarUnidadeDto>
{
    public CriarUnidadeValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(Unidade.TamanhoMaximoNome);
    }
}

public class AtualizarUnidadeValidator : AbstractValidator<AtualizarUnidadeDto>
{
    public AtualizarUnidadeValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(Unidade.TamanhoMaximoNome);
    }
}

public class CriarColaboradorValidator : AbstractValidator<CriarColaboradorDto>
{
    public CriarColaboradorValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(Colaborador.TamanhoMaximoNome);
        RuleFor(x => x.UnidadeId).NotEmpty().WithMessage("Informe a unidade.");
        RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("Informe o usuário.");
    }
}

public class AtualizarColaboradorValidator : AbstractValidator<AtualizarColaboradorDto>
{
    public AtualizarColaboradorValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(Colaborador.TamanhoMaximoNome);
        RuleFor(x => x.UnidadeId).NotEmpty().WithMessage("Informe a unidade.");
    }
}

// --- PATCH ---------------------------------------------------------------------------
// Cada campo é opcional, mas um PATCH sem nenhum campo é engano do cliente, não um pedido

public class AtualizarParcialUsuarioValidator : AbstractValidator<AtualizarParcialUsuarioDto>
{
    public AtualizarParcialUsuarioValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Senha is not null || x.Ativo is not null)
            .WithMessage("Informe ao menos um campo para atualizar.");

        RuleFor(x => x.Senha)
            .MinimumLength(RegrasSenha.TamanhoMinimo)
            .MaximumLength(RegrasSenha.TamanhoMaximo)
            .When(x => !string.IsNullOrWhiteSpace(x.Senha));
    }
}

public class AtualizarParcialUnidadeValidator : AbstractValidator<AtualizarParcialUnidadeDto>
{
    public AtualizarParcialUnidadeValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Nome is not null || x.Ativo is not null)
            .WithMessage("Informe ao menos um campo para atualizar.");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .MaximumLength(Unidade.TamanhoMaximoNome)
            .When(x => x.Nome is not null);
    }
}

public class AtualizarParcialColaboradorValidator : AbstractValidator<AtualizarParcialColaboradorDto>
{
    public AtualizarParcialColaboradorValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Nome is not null || x.UnidadeId is not null)
            .WithMessage("Informe ao menos um campo para atualizar.");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .MaximumLength(Colaborador.TamanhoMaximoNome)
            .When(x => x.Nome is not null);

        RuleFor(x => x.UnidadeId)
            .NotEmpty().WithMessage("Informe a unidade.")
            .When(x => x.UnidadeId is not null);
    }
}

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Login).NotEmpty();
        RuleFor(x => x.Senha).NotEmpty();
    }
}
