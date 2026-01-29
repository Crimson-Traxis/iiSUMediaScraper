using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace iiSUMediaScraper.Views;

/// <summary>
/// A user control that displays a single game with its media and metadata.
/// Features automatic slideshow rotation for heroes and slides, drag-and-drop support,
/// and file picker integration for adding media manually.
/// </summary>
public sealed partial class GameView : UserControl, INotifyPropertyChanged
{
    private GameViewModel? _viewModel;
    private iiSUMediaScraper.Models.Image? _currentHero;
    private Models.Image? _currentSlide;
    private bool _isLoading;
    private int _currentHeroIndex;
    private int _currentSlideIndex;
    private int _heroCount;
    private int _slideCount;
    private readonly DispatcherTimer _timer;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the GameView.
    /// Sets up a timer for automatic hero/slide rotation every 5 seconds.
    /// </summary>
    public GameView()
    {
        InitializeComponent();
        Loaded += Game_Loaded;
        Unloaded += Game_Unloaded;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };

        _timer.Tick += Timer_Tick;
    }

    /// <summary>
    /// Handles the Add Icons menu item click.
    /// Opens a file picker and adds selected files as icon media.
    /// </summary>
    private async void MenuFlyoutItem_AddIconsClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement)
        {
            IEnumerable<string> files = await PickFiles(frameworkElement);

            await AddIcons(files);
        }
    }

    /// <summary>
    /// Handles the Add Titles menu item click.
    /// Opens a file picker and adds selected files as title media.
    /// </summary>
    private async void MenuFlyoutItem_AddTitlesClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement)
        {
            IEnumerable<string> files = await PickFiles(frameworkElement);

            await AddTitles(files);
        }
    }

    /// <summary>
    /// Handles the Add Slides menu item click.
    /// Opens a file picker and adds selected files as slide/screenshot media.
    /// </summary>
    private async void MenuFlyoutItem_AddSlidesClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement)
        {
            IEnumerable<string> files = await PickFiles(frameworkElement);

            await AddSlides(files);
        }
    }

    /// <summary>
    /// Handles the Add Logos menu item click.
    /// Opens a file picker and adds selected files as logo media.
    /// </summary>
    private async void MenuFlyoutItem_AddLogosClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement)
        {
            IEnumerable<string> files = await PickFiles(frameworkElement);

            await AddLogos(files);
        }
    }

    /// <summary>
    /// Handles the Add Heros menu item click.
    /// Opens a file picker and adds selected files as hero/banner media.
    /// </summary>
    private async void MenuFlyoutItem_AddHerosClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement)
        {
            IEnumerable<string> files = await PickFiles(frameworkElement);

            await AddHeros(files);
        }
    }

    /// <summary>
    /// Handles files dropped on the icon drop zone.
    /// </summary>
    private async void DropZone_IconFilesDropped(object sender, Controls.FilesDroppedEventArgs e)
    {
        await AddIcons(e.Files.Select(f => f.Path));
    }

    /// <summary>
    /// Handles files dropped on the logo drop zone.
    /// </summary>
    private async void DropZone_LogoFilesDropped(object sender, Controls.FilesDroppedEventArgs e)
    {
        await AddLogos(e.Files.Select(f => f.Path));
    }

    /// <summary>
    /// Handles files dropped on the title drop zone.
    /// </summary>
    private async void DropZone_TitleFilesDropped(object sender, Controls.FilesDroppedEventArgs e)
    {
        await AddTitles(e.Files.Select(f => f.Path));
    }

    /// <summary>
    /// Handles files dropped on the hero drop zone.
    /// </summary>
    private async void DropZone_HeroFilesDropped(object sender, Controls.FilesDroppedEventArgs e)
    {
        await AddHeros(e.Files.Select(f => f.Path));
    }

    /// <summary>
    /// Handles files dropped on the slide drop zone.
    /// </summary>
    private async void DropZone_SlideFilesDropped(object sender, Controls.FilesDroppedEventArgs e)
    {
        await AddSlides(e.Files.Select(f => f.Path));
    }

    /// <summary>
    /// Handles timer tick to rotate through heroes and slides in demo mode.
    /// </summary>
    private void Timer_Tick(object? sender, object e)
    {
        UpdateHeroAndSlide();
    }

    /// <summary>
    /// Handles control loaded event.
    /// Starts the slideshow timer and displays initial media.
    /// </summary>
    private void Game_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateHeroAndSlide();

        _timer.Start();
    }

    /// <summary>
    /// Handles control unloaded event.
    /// Stops the slideshow timer.
    /// </summary>
    private void Game_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }

    /// <summary>
    /// Handles demo mode slides collection changes.
    /// Updates the current slide display.
    /// </summary>
    private void DemoModeSlides_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateHeroAndSlide();
    }

    /// <summary>
    /// Handles demo mode heroes collection changes.
    /// Updates the current hero display.
    /// </summary>
    private void DemoModeHeros_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateHeroAndSlide();
    }

    /// <summary>
    /// Handles view model property changes.
    /// Updates the loading state on the UI thread.
    /// </summary>
    private void _viewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            IsLoading = ViewModel?.IsLoading ?? false;
        });
    }

    /// <summary>
    /// Adds local files as icon media to the game's media context.
    /// Reads file bytes and creates icon view models.
    /// </summary>
    /// <param name="files">File paths to add as icons.</param>
    private async Task AddIcons(IEnumerable<string> files)
    {
        if (ViewModel != null && ViewModel.MediaContext != null)
        {
            foreach (string file in files)
            {
                ImageViewModel image = ViewModel.MediaContext.CreateIcon(new Models.Image()
                {
                    Bytes = await File.ReadAllBytesAsync(file),
                    Extension = Path.GetExtension(file),
                    Url = file,
                    Source = Models.SourceFlag.Local
                });

                await ViewModel.MediaContext.AddIcon(image);
            }
        }
    }

    /// <summary>
    /// Adds local files as logo media to the game's media context.
    /// Reads file bytes and creates logo view models.
    /// </summary>
    /// <param name="files">File paths to add as logos.</param>
    private async Task AddLogos(IEnumerable<string> files)
    {
        if (ViewModel != null && ViewModel.MediaContext != null)
        {
            foreach (string file in files)
            {
                ImageViewModel image = ViewModel.MediaContext.CreateLogo(new Models.Image()
                {
                    Bytes = await File.ReadAllBytesAsync(file),
                    Extension = Path.GetExtension(file),
                    Url = file,
                    Source = Models.SourceFlag.Local
                });

                await ViewModel.MediaContext.AddLogo(image);
            }
        }
    }

    /// <summary>
    /// Adds local files as title media to the game's media context.
    /// Reads file bytes and creates title view models.
    /// </summary>
    /// <param name="files">File paths to add as titles.</param>
    private async Task AddTitles(IEnumerable<string> files)
    {
        if (ViewModel != null && ViewModel.MediaContext != null)
        {
            foreach (string file in files)
            {
                ImageViewModel image = ViewModel.MediaContext.CreateTitle(new Models.Image()
                {
                    Bytes = await File.ReadAllBytesAsync(file),
                    Extension = Path.GetExtension(file),
                    Url = file,
                    Source = Models.SourceFlag.Local
                });

                await ViewModel.MediaContext.AddTitle(image);
            }
        }
    }

    /// <summary>
    /// Adds local files as hero/banner media to the game's media context.
    /// Reads file bytes and creates hero view models.
    /// </summary>
    /// <param name="files">File paths to add as heroes.</param>
    private async Task AddHeros(IEnumerable<string> files)
    {
        if (ViewModel != null && ViewModel.MediaContext != null)
        {
            foreach (string file in files)
            {
                MediaViewModel image = ViewModel.MediaContext.CreateHero(new Models.Image()
                {
                    Bytes = await File.ReadAllBytesAsync(file),
                    Extension = Path.GetExtension(file),
                    Url = file,
                    Source = Models.SourceFlag.Local
                });

                await ViewModel.MediaContext.AddHero(image);
            }
        }
    }

    /// <summary>
    /// Adds local files as slide/screenshot media to the game's media context.
    /// Reads file bytes and creates slide view models.
    /// </summary>
    /// <param name="files">File paths to add as slides.</param>
    private async Task AddSlides(IEnumerable<string> files)
    {
        if (ViewModel != null && ViewModel.MediaContext != null)
        {
            foreach (string file in files)
            {
                MediaViewModel image = ViewModel.MediaContext.CreateSlide(new Models.Image()
                {
                    Bytes = await File.ReadAllBytesAsync(file),
                    Extension = Path.GetExtension(file),
                    Url = file,
                    Source = Models.SourceFlag.Local
                });

                await ViewModel.MediaContext.AddSlide(image);
            }
        }
    }

    /// <summary>
    /// Opens a file picker dialog to select multiple image files.
    /// </summary>
    /// <param name="sender">The UI element to anchor the picker to.</param>
    /// <returns>Collection of selected file paths.</returns>
    private async Task<IEnumerable<string>> PickFiles(FrameworkElement sender)
    {
        FileOpenPicker picker = new FileOpenPicker(sender.XamlRoot.ContentIslandEnvironment.AppWindowId)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,

            ViewMode = PickerViewMode.List
        };

        // Show the picker dialog window
        IReadOnlyList<PickFileResult> files = await picker.PickMultipleFilesAsync();

        if (files.Count > 0)
        {
            return files.Select(f => f.Path);
        }

        return [];
    }

    /// <summary>
    /// Updates the currently displayed hero and slide images in demo mode.
    /// Cycles through available media in a round-robin fashion.
    /// </summary>
    private void UpdateHeroAndSlide()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (ViewModel != null)
            {
                if (ViewModel.DemoModeHeros.Count > 0)
                {
                    _currentHeroIndex = (_currentHeroIndex + 1) % ViewModel.DemoModeHeros.Count;
                    CurrentHero = ViewModel.DemoModeHeros[_currentHeroIndex];
                }
                else
                {
                    CurrentHero = null;
                }

                if (ViewModel.DemoModeSlides.Count > 0)
                {
                    _currentSlideIndex = (_currentSlideIndex + 1) % ViewModel.DemoModeSlides.Count;
                    CurrentSlide = ViewModel.DemoModeSlides[_currentSlideIndex];
                }
                else
                {
                    CurrentSlide = null;
                }

                HeroCount = ViewModel.DemoModeHeros.Count;

                SlideCount = ViewModel.DemoModeSlides.Count;
            }
            else
            {
                _currentHeroIndex = 0;
                _currentSlideIndex = 0;
                CurrentHero = null;
                CurrentSlide = null;
            }
        });
    }

    /// <summary>
    /// Raises the PropertyChanged event for data binding updates.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Determines whether an image should be visible based on loading state and image data.
    /// </summary>
    /// <param name="isLoading">Whether the image is currently loading.</param>
    /// <param name="image">The image to check.</param>
    /// <returns>Visibility.Visible if image has data and is not loading, otherwise Collapsed.</returns>
    public Visibility CheckImageVisibility(bool isLoading, Models.Image image)
    {
        if (isLoading || image == null || (image != null && image.Bytes.Length == 0))
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    /// <summary>
    /// Gets or sets the view model for this game control.
    /// Automatically wires up/unwires event handlers for media collection changes.
    /// </summary>
    public GameViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel != value)
            {
                if (_viewModel != null)
                {
                    _viewModel.DemoModeHeros.CollectionChanged -= DemoModeHeros_CollectionChanged;
                    _viewModel.DemoModeSlides.CollectionChanged -= DemoModeSlides_CollectionChanged;
                    _viewModel.PropertyChanged -= _viewModel_PropertyChanged;
                }

                _viewModel = value;

                IsLoading = _viewModel?.IsLoading ?? false;

                if (_viewModel != null)
                {
                    _viewModel.DemoModeHeros.CollectionChanged += DemoModeHeros_CollectionChanged;
                    _viewModel.DemoModeSlides.CollectionChanged += DemoModeSlides_CollectionChanged;
                    _viewModel.PropertyChanged += _viewModel_PropertyChanged;

                    _currentHeroIndex = -1; // Start at -1 so first update goes to index 0
                    _currentSlideIndex = -1;

                    UpdateHeroAndSlide();
                }
                else
                {
                    CurrentHero = null;
                    CurrentSlide = null;
                }

                OnPropertyChanged(nameof(ViewModel));
            }
        }
    }

    /// <summary>
    /// Gets or sets the currently displayed hero image in demo mode.
    /// </summary>
    public Models.Image? CurrentHero
    {
        get => _currentHero;
        set
        {
            if (_currentHero != value)
            {
                _currentHero = value;
                OnPropertyChanged(nameof(CurrentHero));
            }
        }
    }

    /// <summary>
    /// Gets or sets the currently displayed slide image in demo mode.
    /// </summary>
    public Models.Image? CurrentSlide
    {
        get => _currentSlide;
        set
        {
            if (_currentSlide != value)
            {
                _currentSlide = value;
                OnPropertyChanged(nameof(CurrentSlide));
            }
        }
    }

    /// <summary>
    /// Gets or sets the total number of hero images available.
    /// </summary>
    public int HeroCount
    {
        get => _heroCount;
        set
        {
            if (_heroCount != value)
            {
                _heroCount = value;
                OnPropertyChanged(nameof(HeroCount));
            }
        }
    }

    /// <summary>
    /// Gets or sets the total number of slide images available.
    /// </summary>
    public int SlideCount
    {
        get => _slideCount;
        set
        {
            if (_slideCount != value)
            {
                _slideCount = value;
                OnPropertyChanged(nameof(SlideCount));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the game view is currently loading.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }
}