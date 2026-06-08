namespace HubMeteorologico.Domain.ResponseDefault.Interface;

public interface IReturnDefaultDataTable<T>
{
    int TotalRecords { get; set; }
    int TotalPages { get; set; }
    T Data { get; set; }

}
