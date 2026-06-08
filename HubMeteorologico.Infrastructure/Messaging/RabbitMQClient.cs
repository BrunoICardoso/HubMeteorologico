using System.Text;
using HubMeteorologico.Domain.Enums;
using HubMeteorologico.Infrastructure.Helps;
using RabbitMQ.Client;


namespace HubMeteorologico.Infrastructure.Messaging;

public class RabbitMQClient
{
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMQClient(string hostName, string userName, string password)
    {
        _connectionFactory = new ConnectionFactory() { HostName = hostName, UserName = userName, Password = password };
    }

    public async Task PublishMessage(QueueNamesEnum queueNameEnum, string message)
    {
        string queueName = queueNameEnum.GetDescription();

        var properties = new BasicProperties
        {
            Persistent = true
        };

        using (var connection = await _connectionFactory.CreateConnectionAsync())
        using (var channel = await connection.CreateChannelAsync())
        {
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: string.Empty,
                                            routingKey: queueName,
                                            mandatory: true,
                                            basicProperties: properties,
                                            body: body);
        }
    }
}
