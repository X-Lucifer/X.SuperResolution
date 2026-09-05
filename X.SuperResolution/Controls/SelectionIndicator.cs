using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Transformation;

namespace X.SuperResolution.Controls;

/// <summary>
/// A shared selection surface for equally sized segments. Selection only changes the render transform.
/// </summary>
public sealed class SelectionIndicator : Decorator
{
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<SelectionIndicator, int>(nameof(SelectedIndex));

    public static readonly StyledProperty<int> ItemCountProperty =
        AvaloniaProperty.Register<SelectionIndicator, int>(nameof(ItemCount), 1, validate: value => value > 0);

    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<SelectionIndicator, double>(nameof(ItemSpacing), validate: value => double.IsFinite(value) && value >= 0);

    private Size _arranged_size;
    private double _item_pitch;
    private double _target_offset = double.NaN;
    private bool _has_layout;

    static SelectionIndicator()
    {
        AffectsArrange<SelectionIndicator>(ItemCountProperty, ItemSpacingProperty);
    }

    public SelectionIndicator()
    {
        IsHitTestVisible = false;
        Focusable = false;
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public int ItemCount
    {
        get => GetValue(ItemCountProperty);
        set => SetValue(ItemCountProperty, value);
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size available_size)
    {
        Child?.Measure(available_size);
        return default;
    }

    protected override Size ArrangeOverride(Size final_size)
    {
        var width = Math.Max(0, (final_size.Width - (ItemCount - 1) * ItemSpacing) / ItemCount);
        var pitch = width + ItemSpacing;
        var resized = !_has_layout || final_size != _arranged_size || pitch != _item_pitch;
        _arranged_size = final_size;
        _item_pitch = pitch;
        Child?.Arrange(new Rect(0, 0, width, final_size.Height));
        MoveIndicator(animate: !resized);
        _has_layout = true;
        return final_size;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ChildProperty)
        {
            _has_layout = false;
            _target_offset = double.NaN;
        }
        else if (change.Property == SelectedIndexProperty && _has_layout)
        {
            MoveIndicator(animate: true);
        }
    }

    private void MoveIndicator(bool animate)
    {
        if (Child is not { } surface)
            return;

        surface.IsVisible = SelectedIndex >= 0 && SelectedIndex < ItemCount;
        var offset = Math.Clamp(SelectedIndex, 0, ItemCount - 1) * _item_pitch;
        if (animate && offset.Equals(_target_offset))
            return;

        _target_offset = offset;
        var transform = TransformOperations.Parse($"translateX({offset.ToString(CultureInfo.InvariantCulture)}px)");
        if (animate)
        {
            // Avalonia transitions retarget from the current painted value when the user clicks again.
            surface.RenderTransform = transform;
        }
        else
        {
            var transitions = surface.Transitions;
            surface.Transitions = null;
            surface.RenderTransform = transform;
            surface.Transitions = transitions;
        }
    }
}
