using Microsoft.AspNetCore.SignalR;


public class MessageHub : Hub
{
    private readonly ILogger<MessageHub> _logger;

    public MessageHub(ILogger<MessageHub> logger)
    {
        _logger = logger;
    }

    public async Task SendMessage(int id, string message, DateTime timestamp)
    {
        await Clients.All.SendAsync("ReceiveMessage", id , message, timestamp);
    }
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("A new client has connected. Connection ID: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client with ID {ConnectionId} has disconnected", Context.ConnectionId);
        if (exception != null)
        {
            _logger.LogError(exception, "Error while disconnecting the client.");
        }
        await base.OnDisconnectedAsync(exception);
    }
}