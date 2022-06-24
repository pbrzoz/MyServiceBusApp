using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;

namespace ServiceBusSender.Services
{
    public class QueueService
    {
        private readonly IConfiguration _configuration;
        static ServiceBusClient _client;
        static Azure.Messaging.ServiceBus.ServiceBusSender _sender;

        public QueueService(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new ServiceBusClient(_configuration.GetConnectionString("ServiceBusConnectionString"));
            _sender = _client.CreateSender("insertQueue");
        }

        public async Task SendMessageAsync<T>(T serviceBusMessage)
        {
            string messageBody = JsonSerializer.Serialize(serviceBusMessage);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));
            try
            {
                await _sender.SendMessageAsync(message);
                Console.WriteLine("SENDER - message sent");
            }
            finally
            {
                await _sender.DisposeAsync();
                await _client.DisposeAsync();
            }
        }
    }
}
