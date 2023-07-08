namespace RabbitMQWebAPI.Services
{
	public interface IRabbitMQService
	{
		void SendMessage(string message);
		bool DeleteMessage(string id);
	}
}
