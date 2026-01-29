using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace iiSUMediaScraper.Controls;

/// <summary>
/// A custom ComboBox control that provides better binding support and selection handling.
/// Fixes issues with the standard ComboBox when working with data-bound collections.
/// </summary>
public class CustomComboBox : ComboBox
{
    private const string ContentPresenterKey = "ContentPresenter";
    private const string PopupKey = "Popup";

    private ContentPresenter _contentPresenter;
    private Popup _popup;

    // Prevents recursive update loops when programmatically changing selection
    private bool _ingoreUpdate;

    // Stores the selected item when the popup is open
    private object? _popupSelectedItem;

    /// <summary>
    /// Raised when the selection changes. Shadows the base SelectionChanged event to provide better control.
    /// </summary>
    public new event EventHandler<SelectionChangedEventArgs> SelectionChanged;

    /// <summary>
    /// Initializes a new instance of the CustomComboBox.
    /// </summary>
    public CustomComboBox()
    {
        DefaultStyleKey = typeof(CustomComboBox);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _contentPresenter = GetTemplateChild(ContentPresenterKey) as ContentPresenter;
        _popup = GetTemplateChild(PopupKey) as Popup;

        base.SelectionChanged += BindableComboBox_SelectionChanged;

        Loaded += BindableComboBox_Loaded;

        Unloaded += BindableComboBox_Unloaded;

        ReDraw();
    }

    private void BindableComboBox_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_popup != null)
        {
            _popup.Closed -= Popup_Closed;
            _popup.Opened -= Popup_Opened;
        }

        RemoveBindings();
    }

    private void BindableComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (_popup != null)
        {
            _popup.Closed += Popup_Closed;
            _popup.Opened += Popup_Opened;
        }

        AddBindings();

        ReDraw();
    }

    private void Popup_Opened(object sender, object e)
    {
        _popupSelectedItem = SelectedItem;
    }

    private void Popup_Closed(object sender, object e)
    {
        ReDraw();

        SelectedItem = _popupSelectedItem;
    }

    private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        ReDraw();
    }

    private void BindableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_popup != null)
        {
            if (!_popup.IsOpen)
            {
                if (!_ingoreUpdate)
                {
                    if (e.AddedItems?.FirstOrDefault() is object obj)
                    {
                        SelectedItem = obj;
                    }
                }
            }
            else
            {
                if (e.AddedItems?.FirstOrDefault() is object obj)
                {
                    _popupSelectedItem = obj;
                }
            }
        }
    }

    private void AddBindings()
    {
        if (ItemsSource is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += Collection_CollectionChanged;
        }

        if (ItemsSource is ObservableCollection<object> observableCollection)
        {
            observableCollection.CollectionChanged += Collection_CollectionChanged;
        }
    }

    private void RemoveBindings()
    {
        if (ItemsSource is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= Collection_CollectionChanged;
        }

        if (ItemsSource is ObservableCollection<object> observableCollection)
        {
            observableCollection.CollectionChanged -= Collection_CollectionChanged;
        }
    }

    private async void RefreshAndRedraw()
    {
        await Task.Delay(50);

        _ = DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                base.ItemsSource = null;

                base.ItemsSource = ItemsSource;

                UpdateLayout();

                ReDraw();
            }
            catch (Exception)
            {

            }
        });
    }

    private void ReDraw()
    {
        try
        {
            _ingoreUpdate = true;

            if (_contentPresenter != null)
            {
                if (SelectedItem != null)
                {
                    if (!string.IsNullOrWhiteSpace(DisplayMemberPath))
                    {
                        _contentPresenter.ContentTemplate = null;
                        _contentPresenter.ContentTemplateSelector = null;
                        _contentPresenter.Content = SelectedItem?.GetType().GetProperty(DisplayMemberPath).GetValue(SelectedItem)?.ToString();
                    }
                    else if (ItemTemplate != null)
                    {
                        _contentPresenter.ContentTemplate = ItemTemplate;
                        _contentPresenter.ContentTemplateSelector = ItemTemplateSelector;
                        _contentPresenter.Content = SelectedItem;
                    }
                    else
                    {
                        _contentPresenter.ContentTemplate = null;
                        _contentPresenter.ContentTemplateSelector = null;
                        _contentPresenter.Content = SelectedItem?.ToString();
                    }
                }
                else
                {
                    _contentPresenter.ContentTemplate = null;
                    _contentPresenter.ContentTemplateSelector = null;
                    _contentPresenter.Content = null;
                }
            }

            if (SelectedItem != null)
            {
                object foundObj = null;

                if (ItemsSource is IEnumerable enumerable)
                {
                    foreach (object? obj in enumerable)
                    {
                        if (obj?.Equals(SelectedItem) ?? false)
                        {
                            foundObj = obj;

                            break;
                        }
                    }
                }

                bool isNeedCreateCollection = false;

                if (base.ItemsSource == null)
                {
                    isNeedCreateCollection = true;
                }
                else
                {
                    if (base.ItemsSource is IEnumerable enumerable1)
                    {
                        foreach (object? obj in enumerable1)
                        {
                            isNeedCreateCollection = false;

                            break;
                        }
                    }
                }

                if (isNeedCreateCollection)
                {
                    base.ItemsSource = new List<object>() { SelectedItem };
                }

                if (foundObj != null)
                {
                    try
                    {
                        base.SelectedItem = null;

                        base.ItemsSource = null;

                        base.ItemsSource = ItemsSource;

                        base.SelectedItem = foundObj;

                        UpdateLayout();
                    }
                    catch
                    {

                    }
                }
                else
                {
                    base.SelectedItem = SelectedItem;
                }
            }

            _ingoreUpdate = false;
        }
        catch
        {

        }
    }

    private static void OnItemsSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        CustomComboBox? target = obj as CustomComboBox;

        object oldValue = args.OldValue;
        object newValue = args.NewValue;

        if (oldValue != newValue)
        {
            target.OnItemsSourcePropertyChanged(oldValue, newValue);
        }
    }

    private static void OnSelectedItemPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        CustomComboBox? target = obj as CustomComboBox;

        object oldValue = args.OldValue;
        object newValue = args.NewValue;

        if (oldValue != newValue)
        {
            target.OnSelectedItemPropertyChanged(oldValue, newValue);
        }
    }

    protected virtual void OnItemsSourcePropertyChanged(object oldValue, object newValue)
    {
        _ingoreUpdate = true;

        RemoveBindings();
        AddBindings();

        try
        {
            base.ItemsSource = newValue;
            base.SelectedItem = SelectedItem;
        }
        catch
        {
        }

        RefreshAndRedraw();
    }

    protected virtual void OnSelectedItemPropertyChanged(object oldValue, object newValue)
    {
        _ingoreUpdate = true;

        try
        {
            base.ItemsSource = ItemsSource;
            base.SelectedItem = SelectedItem;
        }
        catch
        {
        }

        SelectionChanged?.Invoke(this, new SelectionChangedEventArgs([], SelectedItem != null ? [SelectedItem] : []));

        ReDraw();
    }

    /// <summary>
    /// Gets or sets the source of items for the ComboBox.
    /// Shadows the base property to provide better binding support.
    /// </summary>
    public new object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected item.
    /// Shadows the base property to provide better binding support.
    /// </summary>
    public new object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the path to the property used for searching within items.
    /// </summary>
    public string SearchMemberPath
    {
        get => (string)GetValue(SearchMemberPathProperty);
        set => SetValue(SearchMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum number of characters required to trigger a search.
    /// Default is 2.
    /// </summary>
    public int SearchContainsMinimumCharacterCount
    {
        get => (int)GetValue(SearchContainsMinimumCharacterCountProperty);
        set => SetValue(SearchContainsMinimumCharacterCountProperty, value);
    }

    /// <summary>
    /// Identifies the ItemsSource dependency property.
    /// </summary>
    public static new readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(CustomComboBox), new PropertyMetadata(null, OnItemsSourcePropertyChanged));

    /// <summary>
    /// Identifies the SelectedItem dependency property.
    /// </summary>
    public static new readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(CustomComboBox), new PropertyMetadata(null, OnSelectedItemPropertyChanged));

    /// <summary>
    /// Identifies the SearchMemberPath dependency property.
    /// </summary>
    public static readonly DependencyProperty SearchMemberPathProperty =
        DependencyProperty.Register(nameof(SearchMemberPath), typeof(string), typeof(CustomComboBox), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the SearchContainsMinimumCharacterCount dependency property.
    /// </summary>
    public static readonly DependencyProperty SearchContainsMinimumCharacterCountProperty =
        DependencyProperty.Register(nameof(SearchContainsMinimumCharacterCount), typeof(int), typeof(CustomComboBox), new PropertyMetadata(2));
}