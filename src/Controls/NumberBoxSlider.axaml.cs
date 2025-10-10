using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace MicMuter.Controls;

public partial class NumberBoxSlider : UserControl
{
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<NumberBoxSlider, double>(nameof(Minimum));

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<NumberBoxSlider, double>(nameof(Maximum), 100.0);

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<NumberBoxSlider, double>(
        nameof(Value),
        defaultValue: 0.0,
        defaultBindingMode: BindingMode.TwoWay,
        coerce: CoerceValue
    );

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public NumberBoxSlider()
    {
        InitializeComponent();
        ValueTextBox.AddHandler(TextInputEvent, ValueTextBox_OnTextInput, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        ValueTextBox.LostFocus += (_, _) => ValueTextBox.Text = Value.ToString("F0", CultureInfo.CurrentCulture);
        ValueTextBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Return) ValueSlider.Focus();
        };
        ValueTextBox.GotFocus += (_, _) => Dispatcher.UIThread.Post(ValueTextBox.SelectAll);
    }

    private static void ValueTextBox_OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (sender is not TextBox textBox || textBox.Text is null) return;

        var proposedText = textBox.Text.Insert(textBox.CaretIndex, e.Text ?? "");

        if (!ValidNumericInput().IsMatch(proposedText)) e.Handled = true;
    }

    private static double CoerceValue(AvaloniaObject instance, double value)
    {
        if (instance is not NumberBoxSlider control) return value;

        double min = control.Minimum;
        double max = double.Max(min, control.Maximum);
        return double.Clamp(value, min, max);
    }

    static NumberBoxSlider()
    {
        Action<AvaloniaObject, AvaloniaPropertyChangedEventArgs> reCoerce = (s, _) => s.CoerceValue(ValueProperty);
        MinimumProperty.Changed.AddClassHandler<NumberBoxSlider>(reCoerce);
        MaximumProperty.Changed.AddClassHandler<NumberBoxSlider>(reCoerce);
    }

    [GeneratedRegex(@"^-?\d*\.?\d*$")]
    private static partial Regex ValidNumericInput();
}
