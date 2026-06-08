using System.Net;

namespace HubMeteorologico.Domain.ResponseDefault.Interface;

public interface IReturnDefault
{
    bool IsSuccessStatusCode { get; }
    bool IsNoContentStatusCode { get; }
    HttpStatusCode StatusCode { get; }
    string Message { get; }
    Exception Exception { get; }
    Dictionary<string, string[]> ModelState { get; }
}
public interface IReturnDefault<TData> : IReturnDefault
{
    TData Data { get; }
}
