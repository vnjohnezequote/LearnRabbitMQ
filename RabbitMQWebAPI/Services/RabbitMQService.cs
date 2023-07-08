using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RabbitMQ.Client;

namespace RabbitMQWebAPI.Services
{
	public class RabbitMQService : IRabbitMQService
	{
		private readonly ConnectionFactory _connectionFactory;
		private readonly string? _queueName;

		public RabbitMQService(string? hostName,int port, string? userName, string? password, string? queueName)
		{
			_connectionFactory = new ConnectionFactory
			{
				HostName = hostName,
				Port = port,
				UserName = userName,
				Password = password
			};

			_queueName = queueName;
		}

		public void SendMessage(string message)
		{
			using var connection = _connectionFactory.CreateConnection();
			using var channel = connection.CreateModel();

			channel.QueueDeclare(queue: _queueName,
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null);

			var body = Encoding.UTF8.GetBytes(message);

			channel.BasicPublish(exchange: "",
				routingKey: _queueName,
				basicProperties: null,
				body: body);
		}

		public bool DeleteMessage(string id)
		{
			try
			{
				using var client = new HttpClient();

				var baseUrl = $"http://{_connectionFactory.HostName}:{_connectionFactory.Port}/api/";
				var queueUrl = $"queues/%2F/{_queueName}";

				var getMessagesUrl = $"{baseUrl}{queueUrl}/get";
				var response = client.GetStringAsync(getMessagesUrl).Result;

				var messages = JsonSerializer.Deserialize<List<Message>>(response);

				var message = messages.FirstOrDefault(m => m.Payload.Properties.MessageId == id);
				if (message != null)
				{
					var deleteMessageUrl = $"{baseUrl}{queueUrl}/contents/{Uri.EscapeDataString(message.Payload.Properties.MessageId)}";
					var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, deleteMessageUrl);
					deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_connectionFactory.UserName}:{_connectionFactory.Password}")));

					var deleteResponse = client.SendAsync(deleteRequest).Result;
					deleteResponse.EnsureSuccessStatusCode();

					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}

	public class Message
	{
		[JsonPropertyName("payload")]
		public Payload Payload { get; set; }
	}

	public class Payload
	{
		[JsonPropertyName("properties")]
		public Properties Properties { get; set; }
	}

	public class Properties
	{
		[JsonPropertyName("message_id")]
		public string MessageId { get; set; }
	}
}
