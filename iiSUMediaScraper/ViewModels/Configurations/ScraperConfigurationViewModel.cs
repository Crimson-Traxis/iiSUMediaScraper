using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.ObservableModels.Configurations;
using System.Collections.ObjectModel;

namespace iiSUMediaScraper.ViewModels.Configurations;

/// <summary>
/// Represents the view model for scraper configuration settings.
/// </summary>
public partial class ScraperConfigurationViewModel : ObservableScraperConfiguration
{
    /// <summary>
    /// Gets the configuration view model.
    /// </summary>
    protected ConfigurationViewModel ConfigurationViewModel { get; private set; }

    /// <summary>
    /// Raised when removal is requested for the configuration.
    /// </summary>
    public event EventHandler? RemoveRequested;

    /// <summary>
    /// Gets or sets the comma-delimited string of icon styles to scrape.
    /// </summary>
    [ObservableProperty]
    private string iconStyles;

    /// <summary>
    /// Gets or sets the comma-delimited string of logo styles to scrape.
    /// </summary>
    [ObservableProperty]
    private string logoStyles;

    /// <summary>
    /// Gets or sets the comma-delimited string of title styles to scrape.
    /// </summary>
    [ObservableProperty]
    private string titleStyles;

    /// <summary>
    /// Gets or sets the comma-delimited string of hero styles to scrape.
    /// </summary>
    [ObservableProperty]
    private string heroStyles;

    /// <summary>
    /// Gets or sets the comma-delimited string of slide styles to scrape.
    /// </summary>
    [ObservableProperty]
    private string slideStyles;

    /// <summary>
    /// Gets or sets the icon fetch limit.
    /// </summary>
    [ObservableProperty]
    private double iconFetchLimit = double.NaN;

    /// <summary>
    /// Gets or sets the title fetch limit.
    /// </summary>
    [ObservableProperty]
    private double titleFetchLimit = double.NaN;

    /// <summary>
    /// Gets or sets the logo fetch limit.
    /// </summary>
    [ObservableProperty]
    private double logoFetchLimit = double.NaN;

    /// <summary>
    /// Gets or sets the hero fetch limit.
    /// </summary>
    [ObservableProperty]
    private double heroFetchLimit = double.NaN;

    /// <summary>
    /// Gets or sets the slide fetch limit.
    /// </summary>
    [ObservableProperty]
    private double slideFetchLimit = double.NaN;

    /// <summary>
    /// Gets or sets the collection of platform translation configurations.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PlatformTranslationConfigurationViewModel> platformTranslationConfigurations;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScraperConfigurationViewModel"/> class.
    /// </summary>
    /// <param name="baseModel">The underlying scraper configuration model.</param>
    /// <param name="configurationViewModel">The configuration view model.</param>
    public ScraperConfigurationViewModel(ScraperConfiguration baseModel, ConfigurationViewModel configurationViewModel) : base(baseModel)
    {
        ConfigurationViewModel = configurationViewModel;

        iconStyles = string.Join(", ", baseModel.IconStyles);

        logoStyles = string.Join(", ", baseModel.LogoStyles);

        titleStyles = string.Join(", ", baseModel.TitleStyles);

        heroStyles = string.Join(", ", baseModel.HeroStyles);

        slideStyles = string.Join(", ", baseModel.SlideStyles);

        platformTranslationConfigurations = [];

        RegisterBaseModelObservableCollection(
            nameof(PlatformTranslationConfigurations),
            baseModel.PlatformTranslationConfigurations,
            platformTranslationConfigurations,
            CreatePlatformTranslationConfiguration,
            InitializePlatformTranslationConfiguration);

        if (base.IconFetchLimit != null)
        {
            iconFetchLimit = (double)base.IconFetchLimit;
        }

        if (base.TitleFetchLimit != null)
        {
            titleFetchLimit = (double)base.TitleFetchLimit;
        }

        if (base.LogoFetchLimit != null)
        {
            logoFetchLimit = (double)base.LogoFetchLimit;
        }

        if (base.HeroFetchLimit != null)
        {
            heroFetchLimit = (double)base.HeroFetchLimit;
        }

        if (base.SlideFetchLimit != null)
        {
            slideFetchLimit = (double)base.SlideFetchLimit;
        }
    }

    /// <summary>
    /// Called when the icon styles string changes to update the base model.
    /// </summary>
    /// <param name="value">The new comma-delimited icon styles string.</param>
    partial void OnIconStylesChanged(string? value)
    {
        if (value != null)
        {
            BaseModel.IconStyles = [.. value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
        }
    }

    /// <summary>
    /// Called when the logo styles string changes to update the base model.
    /// </summary>
    /// <param name="value">The new comma-delimited logo styles string.</param>
    partial void OnLogoStylesChanged(string? value)
    {
        if (value != null)
        {
            BaseModel.LogoStyles = [.. value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
        }
    }

    /// <summary>
    /// Called when the title styles string changes to update the base model.
    /// </summary>
    /// <param name="value">The new comma-delimited title styles string.</param>
    partial void OnTitleStylesChanged(string? value)
    {
        if (value != null)
        {
            BaseModel.TitleStyles = [.. value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
        }
    }

    /// <summary>
    /// Called when the hero styles string changes to update the base model.
    /// </summary>
    /// <param name="value">The new comma-delimited hero styles string.</param>
    partial void OnHeroStylesChanged(string? value)
    {
        if (value != null)
        {
            BaseModel.HeroStyles = [.. value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
        }
    }

    /// <summary>
    /// Called when the slide styles string changes to update the base model.
    /// </summary>
    /// <param name="value">The new comma-delimited slide styles string.</param>
    partial void OnSlideStylesChanged(string? value)
    {
        if (value != null)
        {
            BaseModel.SlideStyles = [.. value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
        }
    }

    /// <summary>
    /// Called when the icon fetch count changes.
    /// </summary>
    /// <param name="value">The double value of the new fetch limit.</param>
    partial void OnIconFetchLimitChanged(double value)
    {
        base.IconFetchLimit = double.IsNaN(value) ? null : (int)value;
    }

    /// <summary>
    /// Called when the tile fetch count changes.
    /// </summary>
    /// <param name="value">The double value of the new fetch limit.</param>
    partial void OnTitleFetchLimitChanged(double value)
    {
        base.TitleFetchLimit = double.IsNaN(value) ? null : (int)value;
    }

    /// <summary>
    /// Called when the logo fetch count changes.
    /// </summary>
    /// <param name="value">The double value of the new fetch limit.</param>
    partial void OnLogoFetchLimitChanged(double value)
    {
        base.LogoFetchLimit = double.IsNaN(value) ? null : (int)value;
    }

    /// <summary>
    /// Called when the hero fetch count changes.
    /// </summary>
    /// <param name="value">The double value of the new fetch limit.</param>
    partial void OnHeroFetchLimitChanged(double value)
    {
        base.HeroFetchLimit = double.IsNaN(value) ? null : (int)value;
    }

    /// <summary>
    /// Called when the slide fetch count changes.
    /// </summary>
    /// <param name="value">The double value of the new fetch limit.</param>
    partial void OnSlideFetchLimitChanged(double value)
    {
        base.SlideFetchLimit = double.IsNaN(value) ? null : (int)value;
    }

    /// <summary>
    /// Handles the remove requested event from a platform translation configuration.
    /// </summary>
    /// <param name="sender">The platform translation configuration requesting removal.</param>
    /// <param name="e">The event arguments.</param>
    private void OnPlatformTranslationConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is PlatformTranslationConfigurationViewModel item)
        {
            RemovePlatformTranslationConfiguration(item);
        }
    }

    /// <summary>
    /// Initializes event handlers for a platform translation configuration.
    /// </summary>
    /// <param name="item">The platform translation configuration to initialize.</param>
    protected void InitializePlatformTranslationConfiguration(PlatformTranslationConfigurationViewModel item)
    {
        item.RemoveRequested += OnPlatformTranslationConfigurationRemoveRequested;
    }

    /// <summary>
    /// Removes event handlers from a platform translation configuration.
    /// </summary>
    /// <param name="item">The platform translation configuration to de-initialize.</param>
    protected void DeInitializePlatformTranslationConfiguration(PlatformTranslationConfigurationViewModel item)
    {
        item.RemoveRequested -= OnPlatformTranslationConfigurationRemoveRequested;
    }

    /// <summary>
    /// Creates a new platform translation configuration view model from a base model.
    /// </summary>
    /// <param name="baseModel">The underlying platform translation configuration model.</param>
    /// <returns>A new platform translation configuration view model.</returns>
    public PlatformTranslationConfigurationViewModel CreatePlatformTranslationConfiguration(PlatformTranslationConfiguration baseModel)
    {
        return new PlatformTranslationConfigurationViewModel(baseModel, ConfigurationViewModel);
    }


    /// <summary>
    /// Creates a new platform translation configuration and adds it to the collection.
    /// </summary>
    [RelayCommand]
    public void CreateNewPlatformTranslationConfiguration()
    {
        InsertPlatformTranslationConfiguration(0, CreatePlatformTranslationConfiguration(new PlatformTranslationConfiguration()));
    }

    /// <summary>
    /// Adds a platform translation configuration to the collection.
    /// </summary>
    /// <param name="item">The platform translation configuration to add.</param>
    public void AddPlatformTranslationConfiguration(PlatformTranslationConfigurationViewModel item)
    {
        InitializePlatformTranslationConfiguration(item);
        PlatformTranslationConfigurations.Add(item);
    }

    /// <summary>
    /// Inserts a platform translation configuration at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert the configuration.</param>
    /// <param name="item">The platform translation configuration to insert.</param>
    public void InsertPlatformTranslationConfiguration(int index, PlatformTranslationConfigurationViewModel item)
    {
        InitializePlatformTranslationConfiguration(item);
        PlatformTranslationConfigurations.Insert(index, item);
    }

    /// <summary>
    /// Removes a platform translation configuration from the collection.
    /// </summary>
    /// <param name="item">The platform translation configuration to remove.</param>
    public void RemovePlatformTranslationConfiguration(PlatformTranslationConfigurationViewModel item)
    {
        DeInitializePlatformTranslationConfiguration(item);
        PlatformTranslationConfigurations.Remove(item);
    }

    /// <summary>
    /// Clears all platform translation configurations from the collection.
    /// </summary>
    public void ClearPlatformTranslationConfigurations()
    {
        foreach (PlatformTranslationConfigurationViewModel? item in PlatformTranslationConfigurations.ToList())
        {
            RemovePlatformTranslationConfiguration(item);
        }
    }

    /// <summary>
    /// Requests removal of the scraper configuration.
    /// </summary>
    [RelayCommand]
    public void RequestRemove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }
}
