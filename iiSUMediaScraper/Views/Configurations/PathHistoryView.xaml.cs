using iiSUMediaScraper.ViewModels.Configurations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace iiSUMediaScraper.Views.Configurations;

/// <summary>
/// A user control for displaying a path history entry with remove support.
/// Uses AddHandler with handledEventsToo to bypass the AutoSuggestBox's
/// internal ListView swallowing pointer events.
/// </summary>
public sealed partial class PathHistoryView : UserControl, INotifyPropertyChanged
{
    private PathHistoryViewModel? _viewModel;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the PathHistoryView.
    /// </summary>
    public PathHistoryView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DeleteButton.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler((s, args) =>
        {
            args.Handled = true;
            if (ViewModel?.RequestRemoveCommand.CanExecute(null) == true)
            {
                ViewModel.RequestRemoveCommand.Execute(null);
            }
        }), true);
    }

    /// <summary>
    /// Raises the PropertyChanged event for data binding updates.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets or sets the view model for this control.
    /// </summary>
    public PathHistoryViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel != value)
            {
                _viewModel = value;
                OnPropertyChanged(nameof(ViewModel));
            }
        }
    }
}
