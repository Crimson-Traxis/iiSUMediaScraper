using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;

namespace iiSUMediaScraper.Controls;

/// <summary>
/// A staggered layout that forces all items in each row to have the same height based on the tallest item in that row.
/// Items are distributed sequentially across columns maintaining source order.
/// Each row has an independent height, preventing bounce effects during scrolling.
/// Properly virtualizes by only measuring items in the viewport, with height estimation for unrealized rows.
/// </summary>
public class UniformHeightStaggeredLayout : VirtualizingLayout
{
    #region Class Members

    private readonly List<double> _columnHeights = [];
    private readonly Dictionary<int, Rect> _itemBounds = [];
    private readonly Dictionary<int, double> _rowHeights = []; // Actual measured height for each row
    private readonly HashSet<int> _realizedElements = []; // Track which elements are currently realized
    private double _estimatedRowHeight = 100.0; // Default estimated height for unrealized rows
    private int _firstRealizedItemIndex;
    private int _lastRealizedItemIndex;

    #endregion

    #region Private Callbacks

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UniformHeightStaggeredLayout layout)
        {
            layout.InvalidateMeasure();
        }
    }
    
    #endregion

    #region Protected Methods

    protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        if (context.ItemCount == 0)
        {
            // Recycle all previously realized elements
            RecycleAllElements(context);
            ClearLayoutState();
            return new Size(0, 0);
        }

        var columnSpacing = ColumnSpacing;
        var rowSpacing = RowSpacing;
        var itemsStretch = ItemsStretch;
        var realizationRect = context.RealizationRect;

        // Calculate column count
        int columnCount;
        double columnWidth;

        if (ColumnCount > 0)
        {
            columnCount = ColumnCount;
            var totalSpacing = columnSpacing * (columnCount - 1);
            columnWidth = (availableSize.Width - totalSpacing) / columnCount;
        }
        else if (!double.IsNaN(DesiredColumnWidth) && DesiredColumnWidth > 0)
        {
            columnCount = Math.Max(1, (int)Math.Floor((availableSize.Width + columnSpacing) / (DesiredColumnWidth + columnSpacing)));
            var totalSpacing = columnSpacing * (columnCount - 1);
            columnWidth = (availableSize.Width - totalSpacing) / columnCount;
        }
        else
        {
            columnCount = 2;
            var totalSpacing = columnSpacing * (columnCount - 1);
            columnWidth = (availableSize.Width - totalSpacing) / columnCount;
        }

        var totalRows = (int)Math.Ceiling((double)context.ItemCount / columnCount);

        // Determine measurement size based on ItemsStretch
        var itemMeasureSize = itemsStretch switch
        {
            UniformHeightStaggeredLayoutItemsStretch.Fill => new Size(columnWidth, double.PositiveInfinity),
            UniformHeightStaggeredLayoutItemsStretch.Uniform => new Size(columnWidth, double.PositiveInfinity),
            _ => new Size(double.PositiveInfinity, double.PositiveInfinity)
        };

        // Calculate approximate row positions to determine which rows are visible
        var currentY = 0.0;
        var firstVisibleRow = 0;
        var lastVisibleRow = totalRows - 1;
        var foundFirstVisible = false;

        for (var row = 0; row < totalRows; row++)
        {
            var rowHeight = _rowHeights.TryGetValue(row, out var cached) ? cached : _estimatedRowHeight;

            if (!foundFirstVisible && currentY + rowHeight >= realizationRect.Y)
            {
                firstVisibleRow = row;
                foundFirstVisible = true;
            }

            if (foundFirstVisible && currentY > realizationRect.Y + realizationRect.Height)
            {
                lastVisibleRow = row - 1;
                break;
            }

            currentY += rowHeight;
            if (row < totalRows - 1)
            {
                currentY += rowSpacing;
            }
        }

        // Add buffer for smooth scrolling
        firstVisibleRow = Math.Max(0, firstVisibleRow - 1);
        lastVisibleRow = Math.Min(totalRows - 1, lastVisibleRow + 1);

        // Calculate realized item range
        var newFirstRealizedIndex = firstVisibleRow * columnCount;
        var newLastRealizedIndex = Math.Min((lastVisibleRow + 1) * columnCount - 1, context.ItemCount - 1);

        // Recycle elements that are no longer in the realized range
        RecycleElementsOutsideRange(context, newFirstRealizedIndex, newLastRealizedIndex);

        _firstRealizedItemIndex = newFirstRealizedIndex;
        _lastRealizedItemIndex = newLastRealizedIndex;

        // Measure only visible rows to determine their actual heights
        var measuredHeights = new List<double>();
        for (var row = firstVisibleRow; row <= lastVisibleRow; row++)
        {
            double maxHeightInRow = 0;
            var startIndex = row * columnCount;
            var endIndex = Math.Min(startIndex + columnCount, context.ItemCount);

            for (var i = startIndex; i < endIndex; i++)
            {
                var element = context.GetOrCreateElementAt(i);
                _realizedElements.Add(i);
                element.Measure(itemMeasureSize);
                maxHeightInRow = Math.Max(maxHeightInRow, element.DesiredSize.Height);
            }

            _rowHeights[row] = maxHeightInRow;
            measuredHeights.Add(maxHeightInRow);
        }

        // Update estimated height based on measured rows (use average)
        if (measuredHeights.Count > 0)
        {
            _estimatedRowHeight = measuredHeights.Average();
        }

        // Initialize column heights
        _columnHeights.Clear();
        for (var i = 0; i < columnCount; i++)
        {
            _columnHeights.Add(0);
        }

        // Calculate positions for all items using actual or estimated heights
        _itemBounds.Clear();

        for (var i = 0; i < context.ItemCount; i++)
        {
            var columnIndex = i % columnCount;
            var rowIndex = i / columnCount;

            // Use cached height if available, otherwise use estimate
            var rowHeight = _rowHeights.TryGetValue(rowIndex, out var actualHeight)
                ? actualHeight
                : _estimatedRowHeight;

            var uniformItemSize = itemsStretch switch
            {
                UniformHeightStaggeredLayoutItemsStretch.Fill => new Size(columnWidth, rowHeight),
                UniformHeightStaggeredLayoutItemsStretch.Uniform => new Size(columnWidth, rowHeight),
                _ => new Size(columnWidth, rowHeight)
            };

            var x = columnIndex * (columnWidth + columnSpacing);
            var y = _columnHeights[columnIndex];

            if (_columnHeights[columnIndex] > 0)
            {
                y += rowSpacing;
            }

            // Only actually measure and arrange visible items
            if (rowIndex >= firstVisibleRow && rowIndex <= lastVisibleRow)
            {
                var element = context.GetOrCreateElementAt(i);
                _realizedElements.Add(i);

                var itemWidth = itemsStretch == UniformHeightStaggeredLayoutItemsStretch.None && element.DesiredSize.Width < columnWidth
                    ? element.DesiredSize.Width
                    : columnWidth;

                var itemX = itemsStretch == UniformHeightStaggeredLayoutItemsStretch.None && element.DesiredSize.Width < columnWidth
                    ? x + (columnWidth - element.DesiredSize.Width) / 2
                    : x;

                _itemBounds[i] = new Rect(itemX, y, itemWidth, uniformItemSize.Height);
                element.Measure(uniformItemSize);
            }
            else
            {
                // For non-visible items, just calculate bounds (don't realize elements)
                _itemBounds[i] = new Rect(x, y, columnWidth, uniformItemSize.Height);
            }

            _columnHeights[columnIndex] = y + uniformItemSize.Height;
        }

        var maxColumnHeight = _columnHeights.Max();
        return new Size(availableSize.Width, maxColumnHeight);
    }

    protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        if (context.ItemCount == 0)
        {
            return finalSize;
        }

        // Only arrange items that were realized during MeasureOverride
        for (var i = _firstRealizedItemIndex; i <= _lastRealizedItemIndex && i < context.ItemCount; i++)
        {
            if (_itemBounds.TryGetValue(i, out var bounds))
            {
                var element = context.GetOrCreateElementAt(i);
                element.Arrange(bounds);
            }
        }

        return finalSize;
    }

    protected override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        base.InitializeForContextCore(context);
        ClearLayoutState();
        _estimatedRowHeight = 100.0;
    }

    protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        base.UninitializeForContextCore(context);
        RecycleAllElements(context);
        ClearLayoutState();
    }

    protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
    {
        // Recycle all realized elements when items change to prevent stale elements
        RecycleAllElements(context);

        // Clear all cached state when items change
        ClearLayoutState();

        // Reset estimated row height on full reset to avoid height estimation issues
        if (args.Action == NotifyCollectionChangedAction.Reset)
        {
            _estimatedRowHeight = 100.0;
        }

        // Invalidate the layout to trigger a fresh measure/arrange pass
        InvalidateMeasure();
    }

    #endregion

    #region Private Methods

    private void ClearLayoutState()
    {
        _itemBounds.Clear();
        _columnHeights.Clear();
        _rowHeights.Clear();
        _realizedElements.Clear();
        _firstRealizedItemIndex = 0;
        _lastRealizedItemIndex = 0;
    }

    private void RecycleAllElements(VirtualizingLayoutContext context)
    {
        foreach (var index in _realizedElements)
        {
            var element = context.GetOrCreateElementAt(index);
            context.RecycleElement(element);
        }
        _realizedElements.Clear();
    }

    private void RecycleElementsOutsideRange(VirtualizingLayoutContext context, int newFirstIndex, int newLastIndex)
    {
        var toRecycle = new List<int>();

        foreach (var index in _realizedElements)
        {
            if (index < newFirstIndex || index > newLastIndex)
            {
                toRecycle.Add(index);
            }
        }

        foreach (var index in toRecycle)
        {
            var element = context.GetOrCreateElementAt(index);
            context.RecycleElement(element);
            _realizedElements.Remove(index);
        }
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the explicit number of columns. If 0, columns are calculated from DesiredColumnWidth.
    /// </summary>
    public int ColumnCount
    {
        get => (int)GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    /// <summary>
    /// Gets or sets the desired width of each column. Used to calculate column count if ColumnCount is 0.
    /// </summary>
    public double DesiredColumnWidth
    {
        get => (double)GetValue(DesiredColumnWidthProperty);
        set => SetValue(DesiredColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal spacing between columns.
    /// </summary>
    public double ColumnSpacing
    {
        get => (double)GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical spacing between rows.
    /// </summary>
    public double RowSpacing
    {
        get => (double)GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets how items should be stretched to fill the available space.
    /// </summary>
    public UniformHeightStaggeredLayoutItemsStretch ItemsStretch
    {
        get => (UniformHeightStaggeredLayoutItemsStretch)GetValue(ItemsStretchProperty);
        set => SetValue(ItemsStretchProperty, value);
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty ColumnCountProperty =
        DependencyProperty.Register(
            nameof(ColumnCount),
            typeof(int),
            typeof(UniformHeightStaggeredLayout),
            new PropertyMetadata(0, OnLayoutPropertyChanged));

    public static readonly DependencyProperty DesiredColumnWidthProperty =
        DependencyProperty.Register(
            nameof(DesiredColumnWidth),
            typeof(double),
            typeof(UniformHeightStaggeredLayout),
            new PropertyMetadata(double.NaN, OnLayoutPropertyChanged));

    public static readonly DependencyProperty ColumnSpacingProperty =
        DependencyProperty.Register(
            nameof(ColumnSpacing),
            typeof(double),
            typeof(UniformHeightStaggeredLayout),
            new PropertyMetadata(8.0, OnLayoutPropertyChanged));

    public static readonly DependencyProperty RowSpacingProperty =
        DependencyProperty.Register(
            nameof(RowSpacing),
            typeof(double),
            typeof(UniformHeightStaggeredLayout),
            new PropertyMetadata(8.0, OnLayoutPropertyChanged));

    public static readonly DependencyProperty ItemsStretchProperty =
        DependencyProperty.Register(
            nameof(ItemsStretch),
            typeof(UniformHeightStaggeredLayoutItemsStretch),
            typeof(UniformHeightStaggeredLayout),
            new PropertyMetadata(UniformHeightStaggeredLayoutItemsStretch.None, OnLayoutPropertyChanged));

    #endregion
}

/// <summary>
/// Defines how items are stretched in a UniformHeightStaggeredLayout.
/// </summary>
public enum UniformHeightStaggeredLayoutItemsStretch
{
    /// <summary>
    /// Items use their natural width (centered if narrower than column), but all use the maximum height.
    /// </summary>
    None = 0,

    /// <summary>
    /// Items stretch to fill the column width and use the uniform maximum height.
    /// </summary>
    Fill = 1,

    /// <summary>
    /// Items stretch uniformly to fill the column width and use the uniform maximum height.
    /// </summary>
    Uniform = 2
}
