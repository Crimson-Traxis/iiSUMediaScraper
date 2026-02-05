using iiSUMediaScraper;
using iiSUMediaScraper.Models;
using iiSUMediaScraper.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
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
    private MediaViewModel? _currentHero;
    private MediaViewModel? _currentSlide;
    private bool _isLoading;
    private int _currentHeroIndex;
    private int _currentSlideIndex;
    private int _heroCount;
    private int _slideCount;
    private readonly DispatcherTimer _timer;
    private bool? _previousIsInDemoMode;

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

        PointerEntered += GameView_PointerEntered;
        PointerExited += GameView_PointerExited;
    }

    /// <summary>
    /// Handles the pointer exited event.
    /// Sets the IsHover on the viewmodel
    /// </summary>
    private void GameView_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if(ViewModel != null)
        {
            ViewModel.IsHover = false;
        }
    }

    /// <summary>
    /// Handles the pointer exited event.
    /// Sets the IsHover on the viewmodel
    /// </summary>
    private void GameView_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.IsHover = true;
        }
    }

    /// <summary>
    /// Handles the menu item click.
    /// Closes the flyout.
    /// </summary>
    private void MenuFlyoutItem_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        AddButton.Flyout?.Hide();
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
    /// Handles the Add Music menu item click.
    /// Opens a file picker and adds selected files as music media.
    /// </summary>
    private async void MenuFlyoutItem_AddMusicClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement)
        {
            IEnumerable<string> files = await PickFiles(frameworkElement);

            await AddMusic(files);
        }
    }

    /// <summary>
    /// Handles the Add video menu item click.
    /// Opens a file picker and adds selected files as music media.
    /// </summary>
    private async void MenuFlyoutItem_AddVideosClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement)
        {
            IEnumerable<string> files = await PickFiles(frameworkElement);

            await AddVideos(files);
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
    /// Handles files dropped on the music drop zone.
    /// </summary>
    private async void DropZone_MusicFilesDropped(object sender, Controls.FilesDroppedEventArgs e)
    {
        await AddMusic(e.Files.Select(f => f.Path));
    }

    /// <summary>
    /// Handles files dropped on the video drop zone.
    /// </summary>
    private async void DropZone_VideoFilesDropped(object sender, Controls.FilesDroppedEventArgs e)
    {
        await AddVideos(e.Files.Select(f => f.Path));
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
    /// Stops the slideshow timer and all media playback.
    /// </summary>
    private void Game_Unloaded(object sender, RoutedEventArgs e)
    {
        StopAllMedia();

        CurrentHero = null;
        CurrentSlide = null;

        _timer.Stop();
    }

    /// <summary>
    /// Stops all media controls including music players, video previews, and the WebView2.
    /// </summary>
    private void StopAllMedia()
    {
        DemoModeMusicPlayer.MediaPlayer?.Pause();
        EditModeMusicPlayer.MediaPlayer?.Pause();

        if (HeroPreviewContentControl.ContentTemplateRoot is MediaPlayerElement heroPlayer)
        {
            heroPlayer.MediaPlayer?.Pause();
        }

        if (SlidePreviewContentControl.ContentTemplateRoot is MediaPlayerElement slidePlayer)
        {
            slidePlayer.MediaPlayer?.Pause();
        }

        try { PlayingVideoWebView.CoreWebView2?.Navigate("about:blank"); } catch { }
    }

    /// <summary>
    /// Handles demo mode slides collection changes.
    /// Updates the current slide display.
    /// </summary>
    private void DemoModeSlides_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        CurrentHero = null;
        CurrentSlide = null;

        UpdateHeroAndSlide();
    }

    /// <summary>
    /// Handles demo mode heroes collection changes.
    /// Updates the current hero display.
    /// </summary>
    private void DemoModeHeros_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        CurrentHero = null;
        CurrentSlide = null;

        UpdateHeroAndSlide();
    }

    /// <summary>
    /// Handles view model property changes.
    /// Updates the loading state and triggers flip animation on the UI thread.
    /// </summary>
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            IsLoading = ViewModel?.IsLoading ?? false;

            if (e.PropertyName == nameof(GameViewModel.IsInDemoMode))
            {
                PlayFlipAnimation();
            }
        });
    }

    /// <summary>
    /// Plays the card flip animation when toggling between demo mode and edit mode.
    /// </summary>
    private void PlayFlipAnimation()
    {
        if (ViewModel == null) return;

        bool isInDemoMode = ViewModel.IsInDemoMode;

        // Only play animation if the state actually changed
        if (_previousIsInDemoMode.HasValue && _previousIsInDemoMode.Value != isInDemoMode)
        {
            if (isInDemoMode)
            {
                // Flip to demo mode
                if (Resources.TryGetValue("FlipToDemoMode", out var flipToDemoStoryboard) && flipToDemoStoryboard is Storyboard storyboard)
                {
                    storyboard.Begin();
                }
            }
            else
            {
                // Flip to edit mode
                if (Resources.TryGetValue("FlipToEditMode", out var flipToEditStoryboard) && flipToEditStoryboard is Storyboard storyboard)
                {
                    storyboard.Begin();
                }
            }
        }

        _previousIsInDemoMode = isInDemoMode;
    }

    private void ViewModel_DemoMediaChanged(object? sender, GameViewModel e)
    {
        UpdateHeroAndSlide();
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
                    Extension = Path.GetExtension(file),
                    LocalPath = file,
                    Source = SourceFlag.Local
                });

                await ViewModel.MediaContext.AddIcon(image, true);
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
                    Extension = Path.GetExtension(file),
                    LocalPath = file,
                    Source = SourceFlag.Local
                });

                await ViewModel.MediaContext.AddLogo(image, true);
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
                    Extension = Path.GetExtension(file),
                    LocalPath = file,
                    Source = SourceFlag.Local
                });

                await ViewModel.MediaContext.AddTitle(image, true);
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
                ImageViewModel image = ViewModel.MediaContext.CreateHero(new Models.Image()
                {
                    Extension = Path.GetExtension(file),
                    LocalPath = file,
                    Source = SourceFlag.Local
                });

                await ViewModel.MediaContext.AddHero(image, true);
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
                ImageViewModel image = ViewModel.MediaContext.CreateSlide(new Models.Image()
                {
                    Extension = Path.GetExtension(file),
                    LocalPath = file,
                    Source = SourceFlag.Local
                });

                await ViewModel.MediaContext.AddSlide(image, true);
            }
        }
    }

    /// <summary>
    /// Adds local files as music media to the game's media context.
    /// Reads file bytes and creates music view models.
    /// </summary>
    /// <param name="files">File paths to add as musics.</param>
    private async Task AddMusic(IEnumerable<string> files)
    {
        if (ViewModel != null && ViewModel.MediaContext != null)
        {
            foreach (string file in files)
            {
                MusicViewModel music = ViewModel.MediaContext.CreateMusic(new Models.Music()
                {
                    Extension = Path.GetExtension(file),
                    LocalPath = file,
                    Url = file,
                    Title = Path.GetFileNameWithoutExtension(file),
                    Source = SourceFlag.Local
                });

                await ViewModel.MediaContext.AddMusic(music, true);
            }
        }
    }

    /// <summary>
    /// Adds local files as video media to the game's media context.
    /// Reads file bytes and creates video view models.
    /// </summary>
    /// <param name="files">File paths to add as videos.</param>
    private async Task AddVideos(IEnumerable<string> files)
    {
        if (ViewModel != null && ViewModel.MediaContext != null)
        {
            foreach (string file in files)
            {
                VideoViewModel video = ViewModel.MediaContext.CreateVideo(new Models.Video()
                {
                    Extension = Path.GetExtension(file),
                    LocalPath = file,
                    Url = file,
                    Title = Path.GetFileNameWithoutExtension(file),
                    Source = SourceFlag.Local,
                    ApplyMediaType = MediaType.Slide
                });
                await ViewModel.MediaContext.AddVideo(video, true);
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
        var picker = new FileOpenPicker(sender.XamlRoot.ContentIslandEnvironment.AppWindowId)
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

                if (ViewModel.DemoModeSlides.Count > 0)
                {
                    _currentSlideIndex = (_currentSlideIndex + 1) % ViewModel.DemoModeSlides.Count;
                    CurrentSlide = ViewModel.DemoModeSlides[_currentSlideIndex];
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
        if (isLoading || image == null || (image != null && string.IsNullOrWhiteSpace(image.LocalPath)))
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
                    StopAllMedia();

                    _viewModel.DemoModeHeros.CollectionChanged -= DemoModeHeros_CollectionChanged;
                    _viewModel.DemoModeSlides.CollectionChanged -= DemoModeSlides_CollectionChanged;
                    _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                    _viewModel.DemoMediaChanged -= ViewModel_DemoMediaChanged;
                }

                _viewModel = value;

                IsLoading = _viewModel?.IsLoading ?? false;

                if (_viewModel != null)
                {
                    _viewModel.DemoModeHeros.CollectionChanged += DemoModeHeros_CollectionChanged;
                    _viewModel.DemoModeSlides.CollectionChanged += DemoModeSlides_CollectionChanged;
                    _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                    _viewModel.DemoMediaChanged += ViewModel_DemoMediaChanged;

                    _currentHeroIndex = -1; // Start at -1 so first update goes to index 0
                    _currentSlideIndex = -1;

                    CurrentHero = null;
                    CurrentSlide = null;

                    // Initialize the flip state without animation
                    _previousIsInDemoMode = _viewModel.IsInDemoMode;
                    if (_viewModel.IsInDemoMode)
                    {
                        DemoModeGrid.Visibility = Visibility.Visible;
                        EditModeGrid.Visibility = Visibility.Collapsed;
                        DemoModeProjection.RotationY = 0;
                        EditModeProjection.RotationY = -90;
                    }
                    else
                    {
                        DemoModeGrid.Visibility = Visibility.Collapsed;
                        EditModeGrid.Visibility = Visibility.Visible;
                        DemoModeProjection.RotationY = 90;
                        EditModeProjection.RotationY = 0;
                    }

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
    public MediaViewModel? CurrentHero
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
    public MediaViewModel? CurrentSlide
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