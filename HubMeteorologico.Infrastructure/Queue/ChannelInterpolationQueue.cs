using System.Threading.Channels;
using HubMeteorologico.Domain.DTOs.Ingestion;
using HubMeteorologico.Domain.Interfaces;

namespace HubMeteorologico.Infrastructure.Queue;

public class ChannelInterpolationQueue : IInterpolationQueue
{
    private readonly Channel<InterpolationRequestedMessage> _channel;

    public ChannelInterpolationQueue()
    {
        _channel = Channel.CreateUnbounded<InterpolationRequestedMessage>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
    }

    public ValueTask EnqueueAsync(InterpolationRequestedMessage message, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(message, cancellationToken);

    public IAsyncEnumerable<InterpolationRequestedMessage> DequeueAllAsync(CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}
