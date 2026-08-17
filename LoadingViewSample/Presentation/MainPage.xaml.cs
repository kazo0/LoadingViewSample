namespace LoadingViewSample.Presentation;

public sealed partial class MainPage : Page
{
    private bool _started;

    public MainPage()
    {
        this.InitializeComponent();

        // Navigation assigns the generated MainViewModel, so wait for it rather than Loaded.
        DataContextChanged += (_, _) => StartInitialLoad();
    }

    private void StartInitialLoad()
    {
        if (_started || DataContext is not MainViewModel model)
        {
            return;
        }

        _started = true;

        // Kick off the initial loads so the LoadingViews start out in their Loading state.
        // Without this they'd resolve straight to Loaded: a non-null Source reporting
        // IsExecuting == false means "done", not "not started".
        model.FetchWeatherForecasts.Execute(null);
        ReloadBoth(model);
    }

    private void OnReloadBoth(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel model)
        {
            ReloadBoth(model);
        }
    }

    // Note we execute the *commands*, not the model methods directly. Only the commands report
    // IsExecuting, and that's what CompositeLoadableSource is watching.
    private static void ReloadBoth(MainViewModel model)
    {
        model.LoadFastContent.Execute(null);
        model.LoadSlowContent.Execute(null);
    }

    // Demo 3: hand the stuck LoadingView a real ILoadable. Because that command reports
    // IsExecuting == false, the control leaves Loading immediately and fades its Content in.
    private void OnFixStuckSource(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel model)
        {
            StuckLoadingView.Source = model.FetchWeatherForecasts;
        }
    }
}
