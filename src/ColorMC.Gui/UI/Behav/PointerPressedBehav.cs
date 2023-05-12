using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ColorMC.Gui.UI.Behav;

/// <summary>
/// Container class for attached properties. Must inherit from <see cref="AvaloniaObject"/>.
/// </summary>
public class PointerPressedBehav : AvaloniaObject
{
    static PointerPressedBehav()
    {
        CommandProperty.Changed.Subscribe(x => HandleCommandChanged(x.Sender, x.NewValue.GetValueOrDefault<ICommand>()));
    }

    /// <summary>
    /// Identifies the <seealso cref="CommandProperty"/> avalonia attached property.
    /// </summary>
    /// <value>Provide an <see cref="ICommand"/> derived object or binding.</value>
    public static readonly AttachedProperty<ICommand> CommandProperty = AvaloniaProperty.RegisterAttached<PointerPressedBehav, Interactive, ICommand>(
        "Command", defaultBindingMode: BindingMode.OneTime);


    /// <summary>
    /// <see cref="CommandProperty"/> changed event handler.
    /// </summary>
    private static void HandleCommandChanged(AvaloniaObject element, ICommand? commandValue)
    {
        if (element is Interactive interactElem)
        {
            if (commandValue != null)
            {
                // Add non-null value
                interactElem.AddHandler(InputElement.PointerPressedEvent, Handler);
            }
            else
            {
                // remove prev value
                interactElem.RemoveHandler(InputElement.PointerPressedEvent, Handler);
            }
        }

        // local handler fcn
        static void Handler(object? s, PointerPressedEventArgs e)
        {
            if (s is Interactive interactElem)
            {
                // This is how we get the parameter off of the gui element.
                object commandParameter = e.GetCurrentPoint(s as Visual);
                ICommand commandValue = interactElem.GetValue(CommandProperty);
                if (commandValue?.CanExecute(commandParameter) == true)
                {
                    commandValue.Execute(commandParameter);
                }
            }
        }
    }


    /// <summary>
    /// Accessor for Attached property <see cref="CommandProperty"/>.
    /// </summary>
    public static void SetCommand(AvaloniaObject element, ICommand commandValue)
    {
        element.SetValue(CommandProperty, commandValue);
    }

    /// <summary>
    /// Accessor for Attached property <see cref="CommandProperty"/>.
    /// </summary>
    public static ICommand GetCommand(AvaloniaObject element)
    {
        return element.GetValue(CommandProperty);
    }
}