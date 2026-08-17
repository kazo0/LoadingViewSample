# LoadingViewSample

A runnable Uno Platform sample for the
[Toolkit Tuesdays: LoadingView](https://kazo0.dev/toolkit-tuesday/2026/08/18/toolkit-tuesday-loadingview.html)
post on [kazo0.dev](https://kazo0.dev).

Scaffolded with:

```bash
dotnet new unoapp -preset blank -toolkit -o LoadingViewSample -n LoadingViewSample
```

The `-toolkit` flag is what wires up Uno Toolkit: it adds `Toolkit` to `<UnoFeatures>` in the
csproj, merges `<ToolkitResources />` into `App.xaml`, and puts the
`xmlns:utu="using:Uno.Toolkit.UI"` namespace on `MainPage.xaml`. Nothing else was needed to start
using `LoadingView`.

## Running it

```bash
dotnet run --project LoadingViewSample/LoadingViewSample.csproj -f net10.0-desktop
```

Other heads: `net10.0-android`, `net10.0-ios`, `net10.0-browserwasm`.

## What's in here

| File | Purpose |
| --- | --- |
| `AsyncCommand.cs` | An `ICommand` that is *also* an `ILoadable` — the busy-aware command from the post. |
| `MainViewModel.cs` | Plain view model exposing three `AsyncCommand`s and their result collections. |
| `WeatherForecast.cs` | Trivial record used by the first demo's list. |
| `MainPage.xaml` | All three demos. |

## The three demos

### 1. Basic usage + a busy-aware command

The same `AsyncCommand` instance is bound to both the button's `Command` and the `LoadingView`'s
`Source`. One object drives both sides: it flips `IsExecuting`, the `LoadingView` swaps in the
spinner, and because `CanExecute` returns `!IsExecuting` the button disables itself for the
duration.

### 2. Waiting on multiple sources

`CompositeLoadableSource` aggregates two `LoadableSource`s (an 800ms call and a 3000ms call) and
reports itself as executing while *any* of them is. The spinner stays up until the slower call
returns, then both lists appear together.

This demo deliberately omits `IsActive="True"` on its `ProgressRing`. It still spins, because the
`LoadingView` template toggles the `utu:ProgressExtensions.IsActive` attached property on whatever
you put in `LoadingContent` as it moves between states.

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

## If you're using MVUX

This sample deliberately sticks to classic MVVM — that's the territory where `LoadingView` earns
its keep, since MVUX apps usually reach for `FeedView` instead (the post covers the trade-off).
But the hand-rolled `AsyncCommand` isn't needed there at all: any public method on an MVUX model
that returns `void`, `Task`, or `ValueTask` is generated into an `IAsyncCommand`, and that
interface already derives from `ILoadable`:

```csharp
public interface IAsyncCommand : ICommand, INotifyPropertyChanged, ILoadable
```

So a generated MVUX command can be bound straight to a `LoadingView.Source` with nothing extra
to write.

## Note on namespaces

`ILoadable` lives in `Uno.Toolkit` — not `Uno.Toolkit.UI`, which is where the controls
(`LoadingView`, `LoadableSource`, `CompositeLoadableSource`) live. Implementing it needs
`using Uno.Toolkit;`.
