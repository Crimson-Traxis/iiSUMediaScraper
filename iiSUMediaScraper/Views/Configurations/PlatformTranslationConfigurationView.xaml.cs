using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels.Configurations;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace iiSUMediaScraper.Views.Configurations;

/// <summary>
/// A user control for configuring platform name translations.
/// Allows users to map alternative platform names to standard platform definitions.
/// </summary>
public sealed partial class PlatformTranslationConfigurationView : UserControl, INotifyPropertyChanged
{
    private PlatformTranslationConfigurationViewModel? _viewModel;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the PlatformTranslationConfigurationView.
    /// </summary>
    public PlatformTranslationConfigurationView()
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
    public PlatformTranslationConfigurationViewModel? ViewModel
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
