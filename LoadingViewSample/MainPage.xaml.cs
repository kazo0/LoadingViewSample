namespace LoadingViewSample;

public sealed partial class MainPage : Page
{
    private readonly MainViewModel _viewModel = new();

    public MainPage()
    {
        this.InitializeComponent();

        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Kick off the initial loads so both LoadingViews start out in their Loading state.
        // Without this they'd resolve straight to Loaded: a non-null Source reporting
        // IsExecuting == false means "done", not "not started".
        _viewModel.FetchWeatherForecasts.Execute(null);
        ReloadBoth();
    }

    private void OnReloadBoth(object sender, RoutedEventArgs e) => ReloadBoth();

    private void ReloadBoth()
    {
        _viewModel.LoadContent0Command.Execute(null);
        _viewModel.LoadContent1Command.Execute(null);
    }

    // Demo 3: hand the stuck LoadingView a real ILoadable. Because that command reports
    // IsExecuting == false, the control leaves Loading immediately and fades its Content in.
    private void OnFixStuckSource(object sender, RoutedEventArgs e)
        => StuckLoadingView.Source = _viewModel.FetchWeatherForecasts;
}
