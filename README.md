# LoadingViewSample

A runnable Uno Platform sample for the
[Toolkit Tuesdays: LoadingView](https://kazo0.dev/toolkit-tuesday/2026/08/18/toolkit-tuesday-loadingview.html)
post on [kazo0.dev](https://kazo0.dev).

Scaffolded with the recommended preset, which brings in MVUX, Uno Extensions and Uno Toolkit:

```bash
dotnet new unoapp -preset recommended -o LoadingViewSample -n LoadingViewSample
```

Note that `-preset` defaults to `blank`, so the `-preset recommended` part matters — a bare
`dotnet new unoapp` gives you the minimal template with none of the above.

## Running it

```bash
dotnet run --project LoadingViewSample/LoadingViewSample.csproj -f net10.0-desktop
```

Other heads: `net10.0-android`, `net10.0-ios`, `net10.0-browserwasm`.

## The MVUX angle

There are no hand-written view models or `ICommand` implementations anywhere in here, and that's
the point. [`MainModel`](LoadingViewSample/Presentation/MainModel.cs) is a plain `partial record`.
Each public method on it that returns `void`, `Task`, or `ValueTask` becomes an `IAsyncCommand`
on the generated `MainViewModel`, and:

```csharp
public interface IAsyncCommand : ICommand, INotifyPropertyChanged, ILoadable
```

`IAsyncCommand` **already derives from `ILoadable`**. That's the whole trick. A generated MVUX
command can be handed straight to a `LoadingView.Source` with nothing extra to write, which is why
the same binding appears on both the `Button.Command` and the `LoadingView.Source` below.

| File | Purpose |
| --- | --- |
| `Presentation/MainModel.cs` | The MVUX model: three `IListState`s and three async methods that become commands. |
| `Presentation/MainPage.xaml` | All three demos. |
| `Presentation/MainPage.xaml.cs` | A little UI-only glue to kick off the initial loads. |
| `Models/WeatherForecast.cs` | Trivial record used by the first demo's list. |

## The three demos

### 1. Basic usage + a busy-aware command

`FetchWeatherForecasts` is bound to both the button's `Command` and the `LoadingView`'s `Source`.
One generated command drives both sides: it reports `IsExecuting`, the `LoadingView` swaps in the
spinner, and the button disables itself for the duration.

### 2. Waiting on multiple sources

`CompositeLoadableSource` aggregates two `LoadableSource`s (an 800ms command and a 3000ms one) and
reports itself as executing while *any* of them is. The spinner stays up until the slower command
returns, then both lists appear together.

This demo deliberately omits `IsActive="True"` on its `ProgressRing`. It still spins, because the
`LoadingView` template toggles the `utu:ProgressExtensions.IsActive` attached property on whatever
you put in `LoadingContent` as it moves between states.

One subtlety worth knowing: the "Reload both" button executes the two *commands*, not the two model
methods. Only the commands report `IsExecuting`, and that's what `CompositeLoadableSource` watches.

### 3. The gotcha — a null `Source` spins forever

The third `LoadingView` is given no `Source` at all. It sits in its `Loading` state indefinitely
and its `Content` never fades in. This isn't a quirk of the sample; it falls out of the control's
own state resolution:

```csharp
var targetState = Source?.IsExecuting ?? true
    ? VisualStateNames.Loading
    : VisualStateNames.Loaded;
```

A null `Source` takes the `?? true` branch — "still loading" — every time.

Recent Toolkit builds help you catch this. Five seconds after the template is applied with a null
`Source`, you'll see this in the debug output:

```text
warn: Uno.Toolkit.UI.LoadingView[0]
      Source is still null 5 seconds after the template was applied. The view will remain in
      'Loading' state indefinitely. Ensure that the Source property is set to an ILoadable
      instance (e.g., via navigation extensions).
```

Click **Set the Source** to hand it a real `ILoadable`. Because that command reports
`IsExecuting == false`, the control leaves `Loading` immediately and fades its content in.

## Note on namespaces

`ILoadable` lives in `Uno.Toolkit` — not `Uno.Toolkit.UI`, which is where the controls
(`LoadingView`, `LoadableSource`, `CompositeLoadableSource`) live. If you implement it by hand
you'll want `using Uno.Toolkit;`, though with MVUX you get it for free via `IAsyncCommand`.
