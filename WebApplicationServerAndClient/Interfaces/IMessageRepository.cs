using WebApplicationServerAndClient.Models;

namespace WebApplicationServerAndClient.Interfaces
{
    public interface IMessageRepository
    {
            Task InitializeDatabaseAsync();
            Task SaveMessageAsync(Message message);
            Task<List<Message>> GetMessagesAsync(DateTime from, DateTime to);
        
    }
}
