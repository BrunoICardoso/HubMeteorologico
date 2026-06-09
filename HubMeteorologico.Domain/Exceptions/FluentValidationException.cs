namespace HubMeteorologico.Domain.Exceptions;

public class FluentValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public FluentValidationException(Dictionary<string, string[]> errors) : base("Ocorreram erros de validação.")
    {
        Errors = errors;
    }

}
