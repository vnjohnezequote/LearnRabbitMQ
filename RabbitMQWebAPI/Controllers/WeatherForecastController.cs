using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RabbitMQWebAPI.Services;

namespace RabbitMQWebAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private static readonly string[] Summaries =
			{ 
				"Freezing", "Bracing", "Chilly", "Cool",
				"Mild", "Warm", "Balmy", "Hot",
				"Sweltering", "Scorching"
			};

		private readonly ILogger<WeatherForecastController> _logger;
		private readonly IRabbitMQService _rabbitMqService;

		public WeatherForecastController(ILogger<WeatherForecastController> logger, IRabbitMQService rabbitMqService)
		{
			_logger = logger;
			_rabbitMqService = rabbitMqService;
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public IEnumerable<WeatherForecast> Get()
		{
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		[HttpPost(Name = "PostWeatherForecast")]
		public IActionResult Post([FromBody] WeatherForecast weatherForecast)
		{
			_rabbitMqService.SendMessage(JsonSerializer.Serialize(weatherForecast));
			return Ok();
		}

		[HttpDelete("{id}", Name = "DeleteWeatherForecast")]
		public IActionResult Delete(string id)
		{
			bool result = _rabbitMqService.DeleteMessage(id);

			if (result)
			{
				return Ok();
			}
			else
			{
				return NotFound();
			}
		}
	}
}