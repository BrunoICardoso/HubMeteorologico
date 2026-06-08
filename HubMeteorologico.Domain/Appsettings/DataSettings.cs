using HubMeteorologico.Domain.Appsettings.Interface;

namespace HubMeteorologico.Domain.Appsettings;

public class DataSettings : IDataSettings
{
    public required string ContextBase { get; set; }
}
