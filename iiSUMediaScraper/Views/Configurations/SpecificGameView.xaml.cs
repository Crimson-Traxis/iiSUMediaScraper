using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace iiSUMediaScraper.Views.Configurations;

/// <summary>
/// A user control for configuring individual game-specific settings.
/// Allows users to override default settings for specific games.
/// </summary>
public sealed partial class SpecificGameView : UserControl, INotifyPropertyChanged
{
    private SpecificGameViewModel? _viewModel;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the SpecificGameView.
    /// </summary>
    public SpecificGameView()
    {
        InitializeComponent();
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
    /// Gets or sets the view model for this configuration control.
    /// </summary>
    public SpecificGameViewModel? ViewModel
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
