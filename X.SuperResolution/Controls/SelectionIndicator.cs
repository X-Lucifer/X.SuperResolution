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

    private Size _arrangedSize;
    private double _itemPitch;
    private double _targetOffset = double.NaN;
    private bool _hasLayout;

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

    protected override Size MeasureOverride(Size availableSize)
    {
        Child?.Measure(availableSize);
        return default;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var width = Math.Max(0, (finalSize.Width - (ItemCount - 1) * ItemSpacing) / ItemCount);
        var pitch = width + ItemSpacing;
        var resized = !_hasLayout || finalSize != _arrangedSize || pitch != _itemPitch;
        _arrangedSize = finalSize;
        _itemPitch = pitch;
        Child?.Arrange(new Rect(0, 0, width, finalSize.Height));
        MoveIndicator(animate: !resized);
        _hasLayout = true;
        return finalSize;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ChildProperty)
        {
            _hasLayout = false;
            _targetOffset = double.NaN;
        }
        else if (change.Property == SelectedIndexProperty && _hasLayout)
        {
            MoveIndicator(animate: true);
        }
    }

    private void MoveIndicator(bool animate)
    {
        if (Child is not { } surface)
            return;

        surface.IsVisible = SelectedIndex >= 0 && SelectedIndex < ItemCount;
        var offset = Math.Clamp(SelectedIndex, 0, ItemCount - 1) * _itemPitch;
        if (animate && offset.Equals(_targetOffset))
            return;

        _targetOffset = offset;
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
