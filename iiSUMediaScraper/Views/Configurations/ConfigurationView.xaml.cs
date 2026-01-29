using CommunityToolkit.WinUI.Collections;
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
/// A comprehensive configuration view for managing application settings.
/// Includes settings for paths, platforms, scrapers, upscalers, and media overlays.
/// Features searchable/filterable configuration lists for easy management.
/// </summary>
public sealed partial class ConfigurationView : UserControl, INotifyPropertyChanged
{
    private ConfigurationViewModel? _viewModel;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the ConfigurationView.
    /// </summary>
    public ConfigurationView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the games folder picker button click.
    /// Opens a folder picker to select the root directory containing game files.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">The event arguments.</param>
    private async void PickGamesFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // disable the button to avoid double-clicking
            button.IsEnabled = false;

            FolderPicker picker = new FolderPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                CommitButtonText = "Pick Folder",
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                ViewMode = PickerViewMode.List
            };

            // Show the picker dialog window
            PickFolderResult folder = await picker.PickSingleFolderAsync();

            if (folder != null && !string.IsNullOrWhiteSpace(folder.Path) && ViewModel != null)
            {
                ViewModel.GamesPath = folder.Path;
            }

            // re-enable the button
            button.IsEnabled = true;
        }
    }

    /// <summary>
    /// Handles the unfound games folder picker button click.
    /// Opens a folder picker to select where games without media should be moved.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">The event arguments.</param>
    private async void PickUnfoundGamesFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // disable the button to avoid double-clicking
            button.IsEnabled = false;

            FolderPicker picker = new FolderPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                CommitButtonText = "Pick Folder",
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                ViewMode = PickerViewMode.List
            };

            // Show the picker dialog window
            PickFolderResult folder = await picker.PickSingleFolderAsync();

            if (folder != null && !string.IsNullOrWhiteSpace(folder.Path) && ViewModel != null)
            {
                ViewModel.UnfoundMediaMovePath = folder.Path;
            }

            // re-enable the button
            button.IsEnabled = true;
        }
    }

    /// <summary>
    /// Handles the assets folder picker button click.
    /// Opens a folder picker to select where processed/formatted assets should be saved.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">The event arguments.</param>
    private async void PickAssetsFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // disable the button to avoid double-clicking
            button.IsEnabled = false;

            FolderPicker picker = new FolderPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                CommitButtonText = "Pick Folder",
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                ViewMode = PickerViewMode.List
            };

            // Show the picker dialog window
            PickFolderResult folder = await picker.PickSingleFolderAsync();

            if (folder != null && !string.IsNullOrWhiteSpace(folder.Path) && ViewModel != null)
            {
                ViewModel.ApplyAssetPath = folder.Path;
            }

            // re-enable the button
            button.IsEnabled = true;
        }
    }

    #region Search Filters

    /// <summary>
    /// Filters specific games based on the search text.
    /// Matches against platform name and game name.
    /// </summary>
    /// <param name="item">The item to filter.</param>
    /// <returns>True if the item matches the filter criteria; otherwise, false.</returns>
    private bool SpecificGamesSearchFilter(object? item)
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.SpecificGamesSearch))
        {
            return true;
        }

        if (ViewModel != null && item is SpecificGameViewModel specificGameView)
        {
            var search = ViewModel.SpecificGamesSearch?.ToLower() ?? "";

            if (specificGameView.Platform?.Name?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
            if (specificGameView.Name?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Filters platform definition configurations based on the search text.
    /// Matches against platform code and name.
    /// </summary>
    /// <param name="item">The item to filter.</param>
    /// <returns>True if the item matches the filter criteria; otherwise, false.</returns>
    private bool PlatformDefinitionConfigurationsSearchFilter(object? item)
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.PlatformDefinitionConfigurationsSearch))
        {
            return true;
        }

        if (ViewModel != null && item is PlatformConfigurationViewModel platformConfigView)
        {
            var search = ViewModel.PlatformDefinitionConfigurationsSearch?.ToLower() ?? "";

            if (platformConfigView.Code?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
            if (platformConfigView.Name?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Filters specific platforms based on the search text.
    /// Matches against platform configuration name.
    /// </summary>
    /// <param name="item">The item to filter.</param>
    /// <returns>True if the item matches the filter criteria; otherwise, false.</returns>
    private bool SpecificPlatformsSearchFilter(object? item)
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.SpecificPlatformsSearch))
        {
            return true;
        }

        if (ViewModel != null && item is SpecificPlatformViewModel platform)
        {
            var search = ViewModel.SpecificPlatformsSearch?.ToLower() ?? "";

            if (platform.PlatformConfiguration.Name.ToLower().Contains(search))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Filters image upscaler configurations based on the search text.
    /// Matches against upscaler name.
    /// </summary>
    /// <param name="item">The item to filter.</param>
    /// <returns>True if the item matches the filter criteria; otherwise, false.</returns>
    private bool ImageUpscalerConfigurationsSearchFilter(object? item)
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.ImageUpscalerConfigurationsSearch))
        {
            return true;
        }

        if (ViewModel != null && item is UpscalerConfigurationViewModel upscalerConfigView)
        {
            var search = ViewModel.ImageUpscalerConfigurationsSearch?.ToLower() ?? "";

            if (upscalerConfigView.Name?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Filters image reconstructor configurations based on the search text.
    /// Matches against reconstructor name.
    /// </summary>
    /// <param name="item">The item to filter.</param>
    /// <returns>True if the item matches the filter criteria; otherwise, false.</returns>
    private bool ImageReconstructorConfigurationsSearchFilter(object? item)
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.ImageReconstructorConfigurationsSearch))
        {
            return true;
        }

        if (ViewModel != null && item is UpscalerConfigurationViewModel reconstructorConfigView)
        {
            var search = ViewModel.ImageReconstructorConfigurationsSearch?.ToLower() ?? "";

            if (reconstructorConfigView.Name?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Filters folder configurations based on the search text.
    /// Matches against platform name and folder name.
    /// </summary>
    /// <param name="item">The item to filter.</param>
    /// <returns>True if the item matches the filter criteria; otherwise, false.</returns>
    private bool FolderConfigurationsSearchFilter(object? item)
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.FolderConfigurationsSearch))
        {
            return true;
        }

        if (ViewModel != null && item is FolderNameConfigurationViewModel folderConfigView)
        {
            var search = ViewModel.FolderConfigurationsSearch?.ToLower() ?? "";

            if (folderConfigView.SelectedPlatform?.Name?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
            if (folderConfigView.Name?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Filters game icon overlay configurations based on the search text.
    /// Matches against platform name and overlay file path.
    /// </summary>
    /// <param name="item">The item to filter.</param>
    /// <returns>True if the item matches the filter criteria; otherwise, false.</returns>
    private bool GameIconOverlayConfigurationsSearchFilter(object? item)
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.GameIconOverlayConfigurationsSearch))
        {
            return true;
        }

        if (ViewModel != null && item is GameIconOverlayConfigurationViewModel gameIconOverlayConfigView)
        {
            var search = ViewModel.GameIconOverlayConfigurationsSearch?.ToLower() ?? "";

            if (gameIconOverlayConfigView.SelectedPlatform?.Name?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
            if (gameIconOverlayConfigView.Path?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Filters platform icon configurations based on the search text.
    /// Matches against platform name and icon file path.
    /// </summary>
    /// <param name="item">The item to filter.</param>
    /// <returns>True if the item matches the filter criteria; otherwise, false.</returns>
    private bool PlatformIconConfigurationsSearchFilter(object? item)
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.PlatformIconConfigurationsSearch))
        {
            return true;
        }

        if (ViewModel != null && item is PlatformIconConfigurationViewModel platformIconConfigView)
        {
            var search = ViewModel.PlatformIconConfigurationsSearch?.ToLower() ?? "";

            if (platformIconConfigView.SelectedPlatform?.Name?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
            if (platformIconConfigView.Path?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Filters file extension configurations based on the search text.
    /// Matches against platform name and file extensions.
    /// </summary>
    /// <param name="item">The item to filter.</param>
    /// <returns>True if the item matches the filter criteria; otherwise, false.</returns>
    private bool ExtensionConfigurationsSearchFilter(object? item)
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.ExtensionConfigurationsSearch))
        {
            return true;
        }

        if (ViewModel != null && item is ExtensionConfigurationViewModel extensionConfigView)
        {
            var search = ViewModel.ExtensionConfigurationsSearch?.ToLower() ?? "";

            if (extensionConfigView.SelectedPlatform?.Name?.ToLower().Contains(search) ?? false)
            {
                return true;
            }
            if (extensionConfigView.Extensions?.Any(e => e.ToLower().Contains(search)) ?? false)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Search Changed Event Handlers

    /// <summary>
    /// Handles specific games search text changes and refreshes the filter.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void SpecificGamesSearchChanged(object? sender, EventArgs e)
    {
        SpecificGamesSearch?.RefreshFilter();
    }

    /// <summary>
    /// Handles platform definition configurations search text changes and refreshes the filter.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void PlatformDefinitionConfigurationsSearchChanged(object? sender, EventArgs e)
    {
        PlatformDefinitionConfigurationsSearch?.RefreshFilter();
    }

    /// <summary>
    /// Handles specific platforms search text changes and refreshes the filter.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void SpecificPlatformsSearchChanged(object? sender, EventArgs e)
    {
        SpecificPlatformsSearch?.RefreshFilter();
    }

    /// <summary>
    /// Handles image upscaler configurations search text changes and refreshes the filter.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void ImageUpscalerConfigurationsSearchChanged(object? sender, EventArgs e)
    {
        ImageUpscalerConfigurationsSearch?.RefreshFilter();
    }

    /// <summary>
    /// Handles image reconstructor configurations search text changes and refreshes the filter.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void ImageReconstructorConfigurationsSearchChanged(object? sender, EventArgs e)
    {
        ImageReconstructorConfigurationsSearch?.RefreshFilter();
    }

    /// <summary>
    /// Handles folder configurations search text changes and refreshes the filter.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void FolderConfigurationsSearchChanged(object? sender, EventArgs e)
    {
        FolderConfigurationsSearch?.RefreshFilter();
    }

    /// <summary>
    /// Handles game icon overlay configurations search text changes and refreshes the filter.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void GameIconOverlayConfigurationsSearchChanged(object? sender, EventArgs e)
    {
        GameIconOverlayConfigurationsSearch?.RefreshFilter();
    }

    /// <summary>
    /// Handles platform icon configurations search text changes and refreshes the filter.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void PlatformIconConfigurationsSearchChanged(object? sender, EventArgs e)
    {
        PlatformIconConfigurationsSearch?.RefreshFilter();
    }

    /// <summary>
    /// Handles file extension configurations search text changes and refreshes the filter.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void ExtensionConfigurationsSearchChanged(object? sender, EventArgs e)
    {
        ExtensionConfigurationsSearch?.RefreshFilter();
    }

    #endregion

    /// <summary>
    /// Raises the PropertyChanged event for data binding updates.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Initializes the configuration view by setting up search filters for all configuration collections.
    /// Wires up event handlers for search text changes.
    /// </summary>
    /// <param name="viewModel">The configuration view model to initialize.</param>
    protected void InitializeConfiguration(ConfigurationViewModel viewModel)
    {
        if (viewModel != null)
        {
            // SpecificGames
            SpecificGamesSearch = new AdvancedCollectionView(viewModel.SpecificGames)
            {
                Filter = SpecificGamesSearchFilter
            };
            viewModel.SpecificGamesSearchChanged += SpecificGamesSearchChanged;

            // PlatformDefinitionConfigurations
            PlatformDefinitionConfigurationsSearch = new AdvancedCollectionView(viewModel.PlatformConfigurations)
            {
                Filter = PlatformDefinitionConfigurationsSearchFilter
            };
            viewModel.PlatformDefinitionConfigurationsSearchChanged += PlatformDefinitionConfigurationsSearchChanged;

            // SpecificPlatforms
            SpecificPlatformsSearch = new AdvancedCollectionView(viewModel.SpecificPlatforms)
            {
                Filter = SpecificPlatformsSearchFilter
            };
            viewModel.SpecificPlatformsSearchChanged += SpecificPlatformsSearchChanged;

            // ImageUpscalerConfigurations
            ImageUpscalerConfigurationsSearch = new AdvancedCollectionView(viewModel.UpscalerConfigurations)
            {
                Filter = ImageUpscalerConfigurationsSearchFilter
            };
            viewModel.ImageUpscalerConfigurationsSearchChanged += ImageUpscalerConfigurationsSearchChanged;

            // ImageReconstructorConfigurations
            ImageReconstructorConfigurationsSearch = new AdvancedCollectionView(viewModel.ReconstructorConfigurations)
            {
                Filter = ImageReconstructorConfigurationsSearchFilter
            };
            viewModel.ImageReconstructorConfigurationsSearchChanged += ImageReconstructorConfigurationsSearchChanged;

            // FolderConfigurations
            FolderConfigurationsSearch = new AdvancedCollectionView(viewModel.FolderConfigurations)
            {
                Filter = FolderConfigurationsSearchFilter
            };
            viewModel.FolderConfigurationsSearchChanged += FolderConfigurationsSearchChanged;

            // GameIconOverlayConfigurations
            GameIconOverlayConfigurationsSearch = new AdvancedCollectionView(viewModel.GameIconOverlayConfigurations)
            {
                Filter = GameIconOverlayConfigurationsSearchFilter
            };
            viewModel.GameIconOverlayConfigurationsSearchChanged += GameIconOverlayConfigurationsSearchChanged;

            // PlatformIconConfigurations
            PlatformIconConfigurationsSearch = new AdvancedCollectionView(viewModel.PlatformIconConfigurations)
            {
                Filter = PlatformIconConfigurationsSearchFilter
            };
            viewModel.PlatformIconConfigurationsSearchChanged += PlatformIconConfigurationsSearchChanged;

            // ExtensionConfigurations
            ExtensionConfigurationsSearch = new AdvancedCollectionView(viewModel.ExtensionConfigurations)
            {
                Filter = ExtensionConfigurationsSearchFilter
            };
            viewModel.ExtensionConfigurationsSearchChanged += ExtensionConfigurationsSearchChanged;

            OnPropertyChanged(nameof(SpecificGamesSearch));
            OnPropertyChanged(nameof(PlatformDefinitionConfigurationsSearch));
            OnPropertyChanged(nameof(SpecificPlatformsSearch));
            OnPropertyChanged(nameof(ImageUpscalerConfigurationsSearch));
            OnPropertyChanged(nameof(ImageReconstructorConfigurationsSearch));
            OnPropertyChanged(nameof(FolderConfigurationsSearch));
            OnPropertyChanged(nameof(GameIconOverlayConfigurationsSearch));
            OnPropertyChanged(nameof(PlatformIconConfigurationsSearch));
            OnPropertyChanged(nameof(ExtensionConfigurationsSearch));
        }
    }

    /// <summary>
    /// De-initializes the configuration view by clearing search filters and unwiring event handlers.
    /// Called when the view model is being replaced.
    /// </summary>
    /// <param name="viewModel">The configuration view model to de-initialize.</param>
    protected void DeInitializeConfiguration(ConfigurationViewModel viewModel)
    {
        if (viewModel != null)
        {
            // SpecificGames
            SpecificGamesSearch = [];
            viewModel.SpecificGamesSearchChanged -= SpecificGamesSearchChanged;

            // PlatformDefinitionConfigurations
            PlatformDefinitionConfigurationsSearch = [];
            viewModel.PlatformDefinitionConfigurationsSearchChanged -= PlatformDefinitionConfigurationsSearchChanged;

            // SpecificPlatforms
            SpecificPlatformsSearch = [];
            viewModel.SpecificPlatformsSearchChanged -= SpecificPlatformsSearchChanged;

            // ImageUpscalerConfigurations
            ImageUpscalerConfigurationsSearch = [];
            viewModel.ImageUpscalerConfigurationsSearchChanged -= ImageUpscalerConfigurationsSearchChanged;

            // ImageReconstructorConfigurations
            ImageReconstructorConfigurationsSearch = [];
            viewModel.ImageReconstructorConfigurationsSearchChanged -= ImageReconstructorConfigurationsSearchChanged;

            // FolderConfigurations
            FolderConfigurationsSearch = [];
            viewModel.FolderConfigurationsSearchChanged -= FolderConfigurationsSearchChanged;

            // GameIconOverlayConfigurations
            GameIconOverlayConfigurationsSearch = [];
            viewModel.GameIconOverlayConfigurationsSearchChanged -= GameIconOverlayConfigurationsSearchChanged;

            // PlatformIconConfigurations
            PlatformIconConfigurationsSearch = [];
            viewModel.PlatformIconConfigurationsSearchChanged -= PlatformIconConfigurationsSearchChanged;

            // ExtensionConfigurations
            ExtensionConfigurationsSearch = [];
            viewModel.ExtensionConfigurationsSearchChanged -= ExtensionConfigurationsSearchChanged;
        }
    }

    /// <summary>
    /// Gets or sets the view model for this configuration control.
    /// Automatically initializes/de-initializes search filters when changed.
    /// </summary>
    public ConfigurationViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel != value)
            {
                if (_viewModel != null)
                {
                    DeInitializeConfiguration(_viewModel);
                }

                _viewModel = value;

                if (_viewModel != null)
                {
                    InitializeConfiguration(_viewModel);
                }

                OnPropertyChanged(nameof(ViewModel));
            }
        }
    }

    /// <summary>
    /// Gets the filterable collection view for specific games.
    /// </summary>
    public AdvancedCollectionView? SpecificGamesSearch { get; private set; }

    /// <summary>
    /// Gets the filterable collection view for platform definition configurations.
    /// </summary>
    public AdvancedCollectionView? PlatformDefinitionConfigurationsSearch { get; private set; }

    /// <summary>
    /// Gets the filterable collection view for specific platforms.
    /// </summary>
    public AdvancedCollectionView? SpecificPlatformsSearch { get; private set; }

    /// <summary>
    /// Gets the filterable collection view for image upscaler configurations.
    /// </summary>
    public AdvancedCollectionView? ImageUpscalerConfigurationsSearch { get; private set; }

    /// <summary>
    /// Gets the filterable collection view for image reconstructor configurations.
    /// </summary>
    public AdvancedCollectionView? ImageReconstructorConfigurationsSearch { get; private set; }

    /// <summary>
    /// Gets the filterable collection view for folder configurations.
    /// </summary>
    public AdvancedCollectionView? FolderConfigurationsSearch { get; private set; }

    /// <summary>
    /// Gets the filterable collection view for game icon overlay configurations.
    /// </summary>
    public AdvancedCollectionView? GameIconOverlayConfigurationsSearch { get; private set; }

    /// <summary>
    /// Gets the filterable collection view for platform icon configurations.
    /// </summary>
    public AdvancedCollectionView? PlatformIconConfigurationsSearch { get; private set; }

    /// <summary>
    /// Gets the filterable collection view for file extension configurations.
    /// </summary>
    public AdvancedCollectionView? ExtensionConfigurationsSearch { get; private set; }
}