using HubMeteorologico.Domain.ResponseDefault.Interface;
using System.Net;
using System.Text.Json.Serialization;

namespace HubMeteorologico.Domain.ResponseDefault;

public class ReturnDefault : IReturnDefault
{
    private Exception _exception = new Exception("Default Exception");
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    private string _message = string.Empty;
    public Dictionary<string, string[]> ModelState { get; set; } = new Dictionary<string, string[]>();

    [JsonIgnore]
    public bool IsSuccessStatusCode => (int)_statusCode >= 200 && (int)_statusCode <= 299;

    [JsonIgnore]
    public bool IsNoContentStatusCode => _statusCode == HttpStatusCode.NoContent;

    public string Message
    {
        get => _message;
        set => _message = value;
    }

    [JsonIgnore]
    public HttpStatusCode StatusCode
    {
        get => _statusCode;
        set => _statusCode = value;
    }

    [JsonIgnore]
    public Exception Exception
    {
        get => _exception;
        set
        {
            _exception = value;
            _statusCode = value != null ? HttpStatusCode.InternalServerError : _statusCode;
            _message = _exception?.Message ?? _message;
        }
    }
    public ReturnDefault()
    {
    }
    public ReturnDefault(Exception ex)
    {
        Exception = ex;
    }
    public ReturnDefault(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    public ReturnDefault(HttpStatusCode statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }
}

public class ReturnDefault<TData> : ReturnDefault, IReturnDefault<TData>
{
    public TData Data { get; set; } = default!;

    public ReturnDefault() : base() { }

    public ReturnDefault(Exception ex) : base(ex)
    {
        Exception = ex;
    }

    public ReturnDefault(TData data) : base()
    {
        Data = data;
    }

    public ReturnDefault(HttpStatusCode statusCode, TData data) : base(statusCode)
    {
        StatusCode = statusCode;
        Data = data;
    }

    public ReturnDefault(HttpStatusCode statusCode, string message) : base(statusCode, message)
    {
        StatusCode = statusCode;
        Message = message;
    }

    public ReturnDefault(HttpStatusCode statusCode, string message, TData data) : base(statusCode, message)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }


    public ReturnDefault(HttpStatusCode statusCode) : base(statusCode)
    {
        StatusCode = statusCode;
    }
}
