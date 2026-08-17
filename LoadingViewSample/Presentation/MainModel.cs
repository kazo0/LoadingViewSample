using Uno.Extensions.Reactive;

namespace LoadingViewSample.Presentation;

/// <summary>
/// A plain MVUX model. Note what's *not* here: no INotifyPropertyChanged, no ICommand
/// implementations, no IsBusy flags.
/// </summary>
/// <remarks>
/// Every public method below is turned into an <c>IAsyncCommand</c> on the generated bindable
/// model, and <c>IAsyncCommand</c> derives from <c>ILoadable</c>. That's the whole trick: the same
/// generated command can drive a <c>Button.Command</c> and a <c>LoadingView.Source</c> at once,
/// with nothing extra to write.
/// </remarks>
public partial record MainModel
{
    private static readonly string[] Days =
        ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"];

    private static readonly string[] Summaries =
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering"];

    private readonly Random _random = new();

    public IListState<WeatherForecast> Forecasts => ListState<WeatherForecast>.Empty(this);

    public IListState<string> FastResults => ListState<string>.Empty(this);

    public IListState<string> SlowResults => ListState<string>.Empty(this);

    /// <summary>Drives both the Refresh button and the first LoadingView.</summary>
    public async ValueTask FetchWeatherForecasts(CancellationToken ct)
    {
        await Task.Delay(1500, ct);

        // Built outside the updater so the updater itself stays pure.
        var forecasts = Days
            .Select(day => new WeatherForecast(day, _random.Next(-5, 35), Summaries[_random.Next(Summaries.Length)]))
            .ToImmutableList();

        await Forecasts.Update(_ => forecasts, ct);
    }

    /// <summary>Fast call — finishes well before <see cref="LoadSlowContent"/>.</summary>
    public async ValueTask LoadFastContent(CancellationToken ct)
    {
        await Task.Delay(800, ct);

        var results = Enumerable
            .Range(1, 3)
            .Select(i => $"Fast result #{i} — back in 800ms")
            .ToImmutableList();

        await FastResults.Update(_ => results, ct);
    }

    /// <summary>Slow call — the composite source waits on this one.</summary>
    public async ValueTask LoadSlowContent(CancellationToken ct)
    {
        await Task.Delay(3000, ct);

        var results = Enumerable
            .Range(1, 3)
            .Select(i => $"Slow result #{i} — back in 3000ms")
            .ToImmutableList();

        await SlowResults.Update(_ => results, ct);
    }
}
