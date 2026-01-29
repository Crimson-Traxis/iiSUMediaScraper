using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Xaml.Interactivity;

namespace iiSUMediaScraper.Behaviors;

/// <summary>
/// A behavior that creates a quick-return header effect for ScrollView controls.
/// The header slides out of view when scrolling down and slides back in when scrolling up or at the top.
/// </summary>
public class QuickReturnHeaderBehavior : Behavior<ScrollView>
{
    private double _headerHeight;
    private double _previousVerticalOffset;
    private bool _isHeaderVisible;
    private Storyboard? _showStoryboard;
    private Storyboard? _hideStoryboard;
    private TranslateTransform? _headerTransform;
    private Thickness _originalPadding;

    #region Dependency Properties

    /// <summary>
    /// Identifies the HeaderElement dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderElementProperty =
        DependencyProperty.Register(nameof(HeaderElement), typeof(FrameworkElement), typeof(QuickReturnHeaderBehavior),
            new PropertyMetadata(null));

    /// <summary>
    /// Identifies the AnimationDuration dependency property.
    /// </summary>
    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(double), typeof(QuickReturnHeaderBehavior),
            new PropertyMetadata(250.0));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the header element that will be shown/hidden during scrolling.
    /// </summary>
    public FrameworkElement? HeaderElement
    {
        get => (FrameworkElement?)GetValue(HeaderElementProperty);
        set => SetValue(HeaderElementProperty, value);
    }

    /// <summary>
    /// Gets or sets the duration of the show/hide animations in milliseconds.
    /// Default is 250 milliseconds.
    /// </summary>
    public double AnimationDuration
    {
        get => (double)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    #endregion

    /// <summary>
    /// Called when the behavior is attached to the ScrollView.
    /// Sets up event handlers for loading and unloading.
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.Unloaded += OnUnloaded;
    }

    /// <summary>
    /// Called when the behavior is being detached from the ScrollView.
    /// Removes event handlers and cleans up resources.
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Loaded -= OnLoaded;
        AssociatedObject.Unloaded -= OnUnloaded;
        Cleanup();
    }

    /// <summary>
    /// Handles the Loaded event to initialize the behavior.
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Setup();
    }

    /// <summary>
    /// Handles the Unloaded event to clean up resources.
    /// </summary>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Cleanup();
    }

    /// <summary>
    /// Initializes the behavior by setting up transforms, animations, and padding.
    /// </summary>
    private void Setup()
    {
        if (HeaderElement == null || AssociatedObject.Content == null) return;

        HeaderElement.SizeChanged += OnHeaderSizeChanged;
        AssociatedObject.ViewChanged += OnViewChanged;

        _headerHeight = HeaderElement.ActualHeight;
        _previousVerticalOffset = 0;
        _isHeaderVisible = true;

        // Setup transform for animations
        _headerTransform = new TranslateTransform();
        HeaderElement.RenderTransform = _headerTransform;

        // Add padding to content for header space
        if (AssociatedObject.Content is FrameworkElement content)
        {
            _originalPadding = content.Margin;
            content.Margin = new Thickness(
                _originalPadding.Left,
                _originalPadding.Top + _headerHeight,
                _originalPadding.Right,
                _originalPadding.Bottom);
        }

        // Create animations
        CreateAnimations();
    }

    /// <summary>
    /// Creates the show and hide animations for the header.
    /// Show animation slides the header down from above, hide animation slides it up.
    /// </summary>
    private void CreateAnimations()
    {
        if (_headerTransform == null) return;

        _showStoryboard?.Stop();
        _hideStoryboard?.Stop();

        // Show animation - slide down from -headerHeight to 0
        DoubleAnimation showAnimation = new DoubleAnimation
        {
            From = -_headerHeight,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            EnableDependentAnimation = true
        };

        _showStoryboard = new Storyboard();
        Storyboard.SetTarget(showAnimation, _headerTransform);
        Storyboard.SetTargetProperty(showAnimation, "Y");
        _showStoryboard.Children.Add(showAnimation);

        // Hide animation - slide up from 0 to -headerHeight
        DoubleAnimation hideAnimation = new DoubleAnimation
        {
            From = 0,
            To = -_headerHeight,
            Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn },
            EnableDependentAnimation = true
        };

        _hideStoryboard = new Storyboard();
        Storyboard.SetTarget(hideAnimation, _headerTransform);
        Storyboard.SetTargetProperty(hideAnimation, "Y");
        _hideStoryboard.Children.Add(hideAnimation);
    }

    /// <summary>
    /// Cleans up event handlers, restores original padding, and stops animations.
    /// </summary>
    private void Cleanup()
    {
        if (HeaderElement != null)
        {
            HeaderElement.SizeChanged -= OnHeaderSizeChanged;
        }

        if (AssociatedObject != null)
        {
            AssociatedObject.ViewChanged -= OnViewChanged;

            // Restore original padding
            if (AssociatedObject.Content is FrameworkElement content)
            {
                content.Margin = _originalPadding;
            }
        }

        _showStoryboard?.Stop();
        _hideStoryboard?.Stop();
        _showStoryboard = null;
        _hideStoryboard = null;
        _headerTransform = null;
    }

    /// <summary>
    /// Handles changes to the header's size by updating padding and recreating animations.
    /// </summary>
    private void OnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _headerHeight = e.NewSize.Height;

        // Update content padding
        if (AssociatedObject?.Content is FrameworkElement content)
        {
            content.Margin = new Thickness(
                _originalPadding.Left,
                _originalPadding.Top + _headerHeight,
                _originalPadding.Right,
                _originalPadding.Bottom);
        }

        // Recreate animations with new height
        CreateAnimations();

        // Update header position if hidden
        if (!_isHeaderVisible && _headerTransform != null)
        {
            _headerTransform.Y = -_headerHeight;
        }
    }

    /// <summary>
    /// Handles scroll position changes to determine whether to show or hide the header.
    /// Shows the header when scrolling up or at the top, hides it when scrolling down.
    /// </summary>
    private void OnViewChanged(ScrollView sender, object args)
    {
        if (HeaderElement == null || _headerHeight <= 0) return;

        double currentOffset = sender.VerticalOffset;
        double delta = currentOffset - _previousVerticalOffset;
        _previousVerticalOffset = currentOffset;

        // At top - always show header
        if (currentOffset <= 0)
        {
            if (!_isHeaderVisible)
            {
                ShowHeader();
            }
            return;
        }

        // Scrolling UP - show header
        if (delta < 0 && !_isHeaderVisible)
        {
            ShowHeader();
        }
        // Scrolling DOWN - hide header
        else if (delta > 0 && _isHeaderVisible)
        {
            HideHeader();
        }
    }

    /// <summary>
    /// Animates the header sliding down into view.
    /// </summary>
    private void ShowHeader()
    {
        _isHeaderVisible = true;
        _hideStoryboard?.Stop();
        _headerTransform!.Y = -_headerHeight; // Ensure starting position
        _showStoryboard?.Begin();
    }

    /// <summary>
    /// Animates the header sliding up out of view.
    /// </summary>
    private void HideHeader()
    {
        _isHeaderVisible = false;
        _showStoryboard?.Stop();
        _headerTransform!.Y = 0; // Ensure starting position
        _hideStoryboard?.Begin();
    }
}