using System.Collections.ObjectModel;

namespace LoadingViewSample;

/// <summary>
/// A deliberately plain view model — no MVUX, no INotifyPropertyChanged plumbing. The only thing
/// the LoadingViews need from it is an <c>ILoadable</c>, which each <see cref="AsyncCommand"/> is.
/// </summary>
public class MainViewModel
{
    private static readonly string[] Days =
        ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"];

    private static readonly string[] Summaries =
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering"];

    private readonly Random _random = new();

    public MainViewModel()
    {
        FetchWeatherForecasts = new AsyncCommand(LoadForecastsAsync);
        LoadContent0Command = new AsyncCommand(LoadContent0Async);
        LoadContent1Command = new AsyncCommand(LoadContent1Async);
    }

    public ObservableCollection<WeatherForecast> Forecasts { get; } = new();

    public ObservableCollection<string> Content0 { get; } = new();

    public ObservableCollection<string> Content1 { get; } = new();

    /// <summary>Drives both the Refresh button and the first LoadingView.</summary>
    public AsyncCommand FetchWeatherForecasts { get; }

    /// <summary>Fast call — finishes well before <see cref="LoadContent1Command"/>.</summary>
    public AsyncCommand LoadContent0Command { get; }

    /// <summary>Slow call — the composite source waits on this one.</summary>
    public AsyncCommand LoadContent1Command { get; }

    private async Task LoadForecastsAsync()
    {
        await Task.Delay(1500);

        Forecasts.Clear();
        foreach (var day in Days)
        {
            Forecasts.Add(new WeatherForecast(
                day,
                _random.Next(-5, 35),
                Summaries[_random.Next(Summaries.Length)]));
        }
    }

    private async Task LoadContent0Async()
    {
        await Task.Delay(800);

        Content0.Clear();
        foreach (var i in Enumerable.Range(1, 3))
        {
            Content0.Add($"Fast result #{i} — back in 800ms");
        }
    }

    private async Task LoadContent1Async()
    {
        await Task.Delay(3000);

        Content1.Clear();
        foreach (var i in Enumerable.Range(1, 3))
        {
            Content1.Add($"Slow result #{i} — back in 3000ms");
        }
    }
}
