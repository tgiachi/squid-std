using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Input;
using Terminal.Gui.Views;

namespace SquidStd.Tui.Binding;

/// <summary>Convenience overloads that wire the core binder to concrete Terminal.Gui widgets.</summary>
public sealed partial class ViewBinder
{
    /// <summary>One-way bind a source string property to a <see cref="Label" />'s text.</summary>
    public void OneWayText<TSource>(TSource source, Expression<Func<TSource, string>> property, Label label)
        where TSource : INotifyPropertyChanged
        => OneWay(source, property, label, l => l.Text);

    /// <summary>One-way bind a source string property to a <see cref="Window" />'s title.</summary>
    public void OneWayTitle<TSource>(TSource source, Expression<Func<TSource, string>> property, Window window)
        where TSource : INotifyPropertyChanged
        => OneWay(source, property, window, w => w.Title);

    /// <summary>Two-way bind a source string property to a <see cref="TextField" />.</summary>
    public void TwoWay<TSource>(TSource source, Expression<Func<TSource, string>> property, TextField field)
        where TSource : INotifyPropertyChanged

    // Terminal.Gui 2.4.16 exposes ValueChanged (EventHandler<ValueChangedEventArgs<string?>>)
    // rather than a TextChanged event.
        => TwoWay(source, property, field, f => f.Text, callback => field.ValueChanged += (_, _) => callback());

    /// <summary>Bind a command to a <see cref="Button" />: Accepted triggers it, CanExecute drives Enabled.</summary>
    public void Command(Button button, ICommand command)

        // Use Accepted (post-accept, non-cancellable) rather than Accepting for this side-effect-only
        // handler; Terminal.Gui 2.4.16 docs mark subscribing side effects to the cancellable Accepting
        // phase as incorrect.
        => Command(command, enabled => button.Enabled = enabled, callback => button.Accepted += (_, _) => callback());
}
