namespace LoadingViewSample.Models;

public record WeatherForecast(string Day, int TemperatureC, string Summary)
{
    public string Temperature => $"{TemperatureC}°C";
}
