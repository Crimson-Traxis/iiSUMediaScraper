using iiSUMediaScraper.ViewModels.Configurations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace iiSUMediaScraper.Views.Configurations;

/// <summary>
/// A user control for configuring platform-specific icon paths.
/// Allows users to select custom icon images for each platform.
/// </summary>
public sealed partial class PlatformIconConfigurationView : UserControl, INotifyPropertyChanged
{
    private PlatformIconConfigurationViewModel? _viewModel;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the PlatformIconConfigurationView.
    /// </summary>
    public PlatformIconConfigurationView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the button click to open a file picker for selecting an icon file.
    /// Disables the button during the pick operation to prevent double-clicks.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">The event arguments.</param>
    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // disable the button to avoid double-clicking
            button.IsEnabled = false;

            var picker = new FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                CommitButtonText = "Pick File",
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                ViewMode = PickerViewMode.List
            };

            // Show the picker dialog window
            PickFileResult folder = await picker.PickSingleFileAsync();

            if (folder != null && !string.IsNullOrWhiteSpace(folder.Path) && ViewModel != null)
            {
                ViewModel.Path = folder.Path;
            }

            // re-enable the button
            button.IsEnabled = true;
        }
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
    public PlatformIconConfigurationViewModel? ViewModel
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