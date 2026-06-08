namespace HubMeteorologico.Domain.Exceptions;

public class FluentValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public FluentValidationException(Dictionary<string, string[]> errors) : base("Validation errors have occurred.")
    {
        Errors = errors;
    }

}
