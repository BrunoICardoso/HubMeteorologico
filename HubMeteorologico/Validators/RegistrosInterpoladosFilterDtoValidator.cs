using FluentValidation;
using HubMeteorologico.API.ConfigController;
using HubMeteorologico.Domain.DTOs.RegistrosInterpolados;

namespace HubMeteorologico.API.Validators;

public class RegistrosInterpoladosFilterDtoValidator : AbstractValidatorCustom<RegistrosInterpoladosFilterDto>
{
    public RegistrosInterpoladosFilterDtoValidator()
    {
        RuleFor(x => x.FazendaId)
            .GreaterThan(0).WithMessage("FazendaId deve ser maior que zero.");

        RuleFor(x => x.CodigoLavoura)
            .MaximumLength(100).WithMessage("Codigo da Lavoura deve ter no máximo 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoLavoura));


        RuleFor(x => x.DataHora)
            .NotEmpty().WithMessage("Data Hora é obrigatória.")
            .Must(BeFullHour).WithMessage("Data Hora deve ser uma hora cheia (ex: 2024-01-15T12:00:00Z).");
    }

    private static bool BeFullHour(DateTime dataHora)
        => dataHora.Minute == 0 && dataHora.Second == 0 && dataHora.Millisecond == 0;
}