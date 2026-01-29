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
    }

    partial void OnIconStylesChanged(string? value)
    {
        if (value != null)
        {
            BaseModel.HeroStyles = [.. value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
        }
    }

    partial void OnLogoStylesChanged(string? value)
    {
        if (value != null)
        {
            BaseModel.LogoStyles = [.. value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
        }
    }

    partial void OnTitleStylesChanged(string? value)
    {
        if (value != null)
        {
            BaseModel.TitleStyles = [.. value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
        }
    }

    partial void OnHeroStylesChanged(string? value)
    {
        if (value != null)
        {
            BaseModel.HeroStyles = [.. value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
        }
    }

    partial void OnSlideStylesChanged(string? value)
    {
        if (value != null)
        {
            BaseModel.SlideStyles = [.. value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
        }
    }

    private void OnPlatformTranslationConfigurationRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is PlatformTranslationConfigurationViewModel item)
        {
            RemovePlatformTranslationConfiguration(item);
        }
    }

    protected void InitializePlatformTranslationConfiguration(PlatformTranslationConfigurationViewModel item)
    {
        item.RemoveRequested += OnPlatformTranslationConfigurationRemoveRequested;
    }

    protected void DeInitializePlatformTranslationConfiguration(PlatformTranslationConfigurationViewModel item)
    {
        item.RemoveRequested -= OnPlatformTranslationConfigurationRemoveRequested;
    }

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

    public void AddPlatformTranslationConfiguration(PlatformTranslationConfigurationViewModel item)
    {
        InitializePlatformTranslationConfiguration(item);
        PlatformTranslationConfigurations.Add(item);
    }

    public void InsertPlatformTranslationConfiguration(int index, PlatformTranslationConfigurationViewModel item)
    {
        InitializePlatformTranslationConfiguration(item);
        PlatformTranslationConfigurations.Insert(index, item);
    }

    public void RemovePlatformTranslationConfiguration(PlatformTranslationConfigurationViewModel item)
    {
        DeInitializePlatformTranslationConfiguration(item);
        PlatformTranslationConfigurations.Remove(item);
    }

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
