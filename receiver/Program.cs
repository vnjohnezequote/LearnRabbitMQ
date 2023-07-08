// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("Hello, World!");

var factory = new ConnectionFactory
{
	HostName = "localhost",
	Port = 5672,
	UserName = "guest",
	Password = "guest"
};

var queueName = "myqueue";
var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare(queue: queueName,
	durable: false,
	exclusive: false,
	autoDelete: false,
	arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, eventArgs) =>
{
	var body = eventArgs.Body.ToArray();
	var message = Encoding.UTF8.GetString(body);
	Console.WriteLine(message);

	Console.WriteLine("Press any key to exit");
	Console.ReadKey();
};

channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
Console.ReadKey();

Console.WriteLine("Press any key to exit");
Console.ReadKey();


