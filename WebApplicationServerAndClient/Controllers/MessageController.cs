using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApplicationServerAndClient.Interfaces;
using WebApplicationServerAndClient.Models;

namespace WebApplicationServerAndClient.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IMessageRepository messageRepository, IHubContext<MessageHub> hubContext, ILogger<MessageController> logger)
        {
            _messageRepository = messageRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            _logger.LogInformation("Received request to send a message with content: {MessageContent}", message?.Content);

            if (message == null || string.IsNullOrWhiteSpace(message.Content))
            {
                _logger.LogWarning("Received an empty or null message content.");
                return BadRequest("Message content cannot be empty.");
            }

            try
            {
                message.Timestamp = DateTime.UtcNow;

                await _messageRepository.SaveMessageAsync(message);
                _logger.LogInformation("Message saved to database. Content: {MessageContent}, Timestamp: {Timestamp}", message.Content, message.Timestamp);

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.Content, message.Timestamp, message.Id);
                _logger.LogInformation("Message sent to all clients: {MessageContent}, Timestamp: {Timestamp}, MessageId: {MessageId}", message.Content, message.Timestamp, message.Id);

                return Ok(new { message = "Message stored and sent to clients" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Message length validation failed: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while processing the message: {ErrorMessage}", ex.Message);
                return StatusCode(500, "An error occurred while processing the message.");
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetMessages([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                _logger.LogWarning("Invalid date range: startDate ({StartDate}) is greater than endDate ({EndDate})", startDate, endDate);
                return BadRequest("Start date cannot be greater than end date.");
            }

            try
            {
                var messages = await _messageRepository.GetMessagesAsync(startDate, endDate);

                _logger.LogInformation("Retrieved messages between {StartDate} and {EndDate}. Number of messages: {MessageCount}", startDate, startDate, messages.Count);

                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving messages: {ErrorMessage}", ex.Message);
                return StatusCode(500, "Error retrieving messages: " + ex.Message);
            }
        }
    }
}
