using RabbitMQWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRabbitMQService>(provider =>
{
	var configuration = provider.GetRequiredService<IConfiguration>();
	var rabbitMqConfig = configuration.GetSection("RabbitMQ");
	return new RabbitMQService(
		rabbitMqConfig["HostName"],
		int.Parse(rabbitMqConfig["Port"] ?? string.Empty),
		rabbitMqConfig["UserName"],
		rabbitMqConfig["Password"],
		rabbitMqConfig["QueueName"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
