using FluentValidation;
using GestaoColaboradores.Application.Dtos;

namespace GestaoColaboradores.Application.Validators;

// EXEMPLO COMPLETO — replique o padrão para os demais DTOs de entrada.
public class CriarColaboradorValidator : AbstractValidator<CriarColaboradorDto>
{
    public CriarColaboradorValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CodigoUnidade).NotEmpty();
        RuleFor(x => x.CodigoUsuario).NotEmpty();
    }
}

public class CriarUsuarioValidator : AbstractValidator<CriarUsuarioDto>
{
    public CriarUsuarioValidator()
    {
        // TODO: código/login obrigatórios; senha com tamanho mínimo (ex.: 6+)
    }
}

public class CriarUnidadeValidator : AbstractValidator<CriarUnidadeDto>
{
    public CriarUnidadeValidator()
    {
        // TODO
    }
}

// TODO: AtualizarUsuarioValidator (senha, QUANDO informada, com tamanho mínimo),
//       AtualizarColaboradorValidator, AtualizarUnidadeValidator, LoginValidator.
