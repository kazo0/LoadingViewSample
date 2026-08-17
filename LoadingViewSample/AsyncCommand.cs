using System.Windows.Input;
using Uno.Toolkit;

namespace LoadingViewSample;

/// <summary>
/// An <see cref="ICommand"/> that is also an <see cref="ILoadable"/>, so a single instance can
/// drive both a <c>Button.Command</c> and a <c>LoadingView.Source</c>.
/// </summary>
/// <remarks>
/// <see cref="ILoadable"/> lives in the <c>Uno.Toolkit</c> namespace — not <c>Uno.Toolkit.UI</c>,
/// which is where the controls live.
/// </remarks>
public class AsyncCommand : ICommand, ILoadable
{
    public event EventHandler? CanExecuteChanged;
    public event EventHandler? IsExecutingChanged;

    private readonly Func<Task> _executeAsync;
    private bool _isExecuting;

    public AsyncCommand(Func<Task> executeAsync) => _executeAsync = executeAsync;

    // Disables the bound button for the duration of the work.
    public bool CanExecute(object? parameter) => !IsExecuting;

    public bool IsExecuting
    {
        get => _isExecuting;
        set
        {
            if (_isExecuting != value)
            {
                _isExecuting = value;
                IsExecutingChanged?.Invoke(this, new());
                CanExecuteChanged?.Invoke(this, new());
            }
        }
    }

    // async void is intentional here: ICommand.Execute returns void. The try/finally is what
    // guarantees the LoadingView leaves its Loading state even if the work throws.
    public async void Execute(object? parameter)
    {
        try
        {
            IsExecuting = true;
            await _executeAsync();
        }
        finally
        {
            IsExecuting = false;
        }
    }
}
