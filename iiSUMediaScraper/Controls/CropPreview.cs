using ImageEx;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace iiSUMediaScraper.Controls;

/// <summary>
/// A control that displays an image with a darkened overlay outside the specified crop region.
/// </summary>
public sealed class CropPreview : Control
{
    private ImageEx.ImageEx? _sourceImage;
    private AnimatedImageWebView? _animatedImage;
    private Path? _overlayPath;

    #region Dependency Properties

    public static readonly DependencyProperty HasCropProperty =
        DependencyProperty.Register(
            nameof(HasCrop),
            typeof(bool),
            typeof(CropPreview),
            new PropertyMetadata(false, OnHasCropChanged));

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(ImageSource),
            typeof(CropPreview),
            new PropertyMetadata(null, OnSourceChanged));

    public static readonly DependencyProperty CropLeftProperty =
        DependencyProperty.Register(
            nameof(CropLeft),
            typeof(double),
            typeof(CropPreview),
            new PropertyMetadata(0.0, OnCropRegionChanged));

    public static readonly DependencyProperty CropTopProperty =
        DependencyProperty.Register(
            nameof(CropTop),
            typeof(double),
            typeof(CropPreview),
            new PropertyMetadata(0.0, OnCropRegionChanged));

    public static readonly DependencyProperty CropWidthProperty =
        DependencyProperty.Register(
            nameof(CropWidth),
            typeof(double),
            typeof(CropPreview),
            new PropertyMetadata(0.0, OnCropRegionChanged));

    public static readonly DependencyProperty CropHeightProperty =
        DependencyProperty.Register(
            nameof(CropHeight),
            typeof(double),
            typeof(CropPreview),
            new PropertyMetadata(0.0, OnCropRegionChanged));

    public static readonly DependencyProperty OverlayBrushProperty =
        DependencyProperty.Register(
            nameof(OverlayBrush),
            typeof(Brush),
            typeof(CropPreview),
            new PropertyMetadata(null, OnOverlayBrushChanged));

    public static readonly DependencyProperty OverlayOpacityProperty =
        DependencyProperty.Register(
            nameof(OverlayOpacity),
            typeof(double),
            typeof(CropPreview),
            new PropertyMetadata(0.6, OnOverlayOpacityChanged));

    public static readonly DependencyProperty StretchProperty =
        DependencyProperty.Register(
            nameof(Stretch),
            typeof(Stretch),
            typeof(CropPreview),
            new PropertyMetadata(Stretch.Uniform, OnStretchChanged));

    public static readonly DependencyProperty ImagePathProperty =
        DependencyProperty.Register(
            nameof(ImagePath),
            typeof(string),
            typeof(CropPreview),
            new PropertyMetadata(null, OnImagePathChanged));

    public static readonly DependencyProperty IsAnimatedProperty =
        DependencyProperty.Register(
            nameof(IsAnimated),
            typeof(bool),
            typeof(CropPreview),
            new PropertyMetadata(false, OnIsAnimatedChanged));

    public static readonly DependencyProperty ExtensionProperty =
        DependencyProperty.Register(
            nameof(Extension),
            typeof(string),
            typeof(CropPreview),
            new PropertyMetadata(null, OnExtensionChanged));

    #endregion

    #region Properties

    public bool HasCrop
    {
        get => (bool)GetValue(HasCropProperty);
        set => SetValue(HasCropProperty, value);
    }

    public ImageSource Source
    {
        get => (ImageSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public double CropLeft
    {
        get => (double)GetValue(CropLeftProperty);
        set => SetValue(CropLeftProperty, value);
    }

    public double CropTop
    {
        get => (double)GetValue(CropTopProperty);
        set => SetValue(CropTopProperty, value);
    }

    public double CropWidth
    {
        get => (double)GetValue(CropWidthProperty);
        set => SetValue(CropWidthProperty, value);
    }

    public double CropHeight
    {
        get => (double)GetValue(CropHeightProperty);
        set => SetValue(CropHeightProperty, value);
    }

    public Brush OverlayBrush
    {
        get => (Brush)GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    public double OverlayOpacity
    {
        get => (double)GetValue(OverlayOpacityProperty);
        set => SetValue(OverlayOpacityProperty, value);
    }

    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public string? ImagePath
    {
        get => (string?)GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }

    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    public string? Extension
    {
        get => (string?)GetValue(ExtensionProperty);
        set => SetValue(ExtensionProperty, value);
    }

    #endregion

    public CropPreview()
    {
        DefaultStyleKey = typeof(CropPreview);
        SizeChanged += OnSizeChanged;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _sourceImage = GetTemplateChild("SourceImage") as ImageEx.ImageEx;
        _animatedImage = GetTemplateChild("AnimatedImage") as AnimatedImageWebView;
        _overlayPath = GetTemplateChild("OverlayPath") as Path;

        if (_sourceImage != null)
        {
            _sourceImage.Source = Source;
            _sourceImage.Stretch = Stretch;
        }

        if (_animatedImage != null)
        {
            _animatedImage.ImagePath = ImagePath;
        }

        if (_overlayPath != null)
        {
            _overlayPath.Fill = OverlayBrush ?? new SolidColorBrush(Microsoft.UI.Colors.Black);
            _overlayPath.Opacity = OverlayOpacity;
        }

        UpdateVisibility();
        UpdateAnimatedImageSize();
        UpdateOverlay();
    }

    private void UpdateVisibility()
    {
        if (_sourceImage != null)
        {
            _sourceImage.Visibility = IsAnimated ? Visibility.Collapsed : Visibility.Visible;
        }

        if (_animatedImage != null)
        {
            _animatedImage.Visibility = IsAnimated ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateOverlay();
        UpdateAnimatedImageSize();
    }

    private void UpdateAnimatedImageSize()
    {
        if (_animatedImage != null && ActualWidth > 0 && ActualHeight > 0)
        {
            _animatedImage.Width = ActualWidth;
            _animatedImage.Height = ActualHeight;
        }
    }

    private static void OnHasCropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CropPreview control)
        {
            control.UpdateOverlay();
        }
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CropPreview control)
        {
            if (control._sourceImage != null)
            {
                control._sourceImage.Source = e.NewValue as ImageSource;
            }

            if (e.NewValue is BitmapImage bitmapImage)
            {
                bitmapImage.ImageOpened += (s, args) => control.UpdateOverlay();
            }

            control.UpdateOverlay();
        }
    }

    private static void OnCropRegionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CropPreview control)
        {
            control.UpdateOverlay();
        }
    }

    private static void OnOverlayBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CropPreview control && control._overlayPath != null)
        {
            control._overlayPath.Fill = e.NewValue as Brush ?? new SolidColorBrush(Microsoft.UI.Colors.Black);
        }
    }

    private static void OnOverlayOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CropPreview control && control._overlayPath != null)
        {
            control._overlayPath.Opacity = (double)e.NewValue;
        }
    }

    private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CropPreview control)
        {
            if (control._sourceImage != null)
            {
                control._sourceImage.Stretch = (Stretch)e.NewValue;
            }
            control.UpdateOverlay();
        }
    }

    private static void OnImagePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CropPreview control && control._animatedImage != null)
        {
            control._animatedImage.ImagePath = e.NewValue as string;
        }
    }

    private static void OnExtensionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CropPreview control)
        {
            var extension = (e.NewValue as string)?.ToLowerInvariant();
            // Treat .gif and .webp as animated formats
            control.IsAnimated = extension == ".gif" || extension == ".webp";
        }
    }

    private static void OnIsAnimatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CropPreview control)
        {
            control.UpdateVisibility();
        }
    }

    private void UpdateOverlay()
    {
        if (_overlayPath == null || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        if (!HasCrop)
        {
            _overlayPath.Data = null;
            return;
        }

        double sourceWidth = GetSourceWidth();
        double sourceHeight = GetSourceHeight();

        if (sourceWidth <= 0 || sourceHeight <= 0 || (CropWidth <= 0 && CropHeight <= 0))
        {
            _overlayPath.Data = null;
            return;
        }

        Rect imageRect = GetImageRect(sourceWidth, sourceHeight);
        if (imageRect.Width <= 0 || imageRect.Height <= 0)
        {
            return;
        }

        double scaleX = imageRect.Width / sourceWidth;
        double scaleY = imageRect.Height / sourceHeight;

        double cropLeft = imageRect.X + (CropLeft * scaleX);
        double cropTop = imageRect.Y + (CropTop * scaleY);
        double cropWidth = CropWidth * scaleX;
        double cropHeight = CropHeight * scaleY;

        // Clamp crop region to image bounds
        double cropRight = Math.Min(cropLeft + cropWidth, imageRect.X + imageRect.Width);
        double cropBottom = Math.Min(cropTop + cropHeight, imageRect.Y + imageRect.Height);
        cropLeft = Math.Max(cropLeft, imageRect.X);
        cropTop = Math.Max(cropTop, imageRect.Y);
        cropWidth = Math.Max(0, cropRight - cropLeft);
        cropHeight = Math.Max(0, cropBottom - cropTop);

        // Create 4 rectangles around the crop region instead of using EvenOdd
        var geometryGroup = new GeometryGroup
        {
            FillRule = FillRule.Nonzero
        };

        // Top rectangle
        if (cropTop > imageRect.Y)
        {
            geometryGroup.Children.Add(new RectangleGeometry
            {
                Rect = new Rect(imageRect.X, imageRect.Y, imageRect.Width, cropTop - imageRect.Y)
            });
        }

        // Bottom rectangle
        double cropBottomEdge = cropTop + cropHeight;
        double imageBottomEdge = imageRect.Y + imageRect.Height;
        if (cropBottomEdge < imageBottomEdge)
        {
            geometryGroup.Children.Add(new RectangleGeometry
            {
                Rect = new Rect(imageRect.X, cropBottomEdge, imageRect.Width, imageBottomEdge - cropBottomEdge)
            });
        }

        // Left rectangle (between top and bottom)
        if (cropLeft > imageRect.X)
        {
            geometryGroup.Children.Add(new RectangleGeometry
            {
                Rect = new Rect(imageRect.X, cropTop, cropLeft - imageRect.X, cropHeight)
            });
        }

        // Right rectangle (between top and bottom)
        double cropRightEdge = cropLeft + cropWidth;
        double imageRightEdge = imageRect.X + imageRect.Width;
        if (cropRightEdge < imageRightEdge)
        {
            geometryGroup.Children.Add(new RectangleGeometry
            {
                Rect = new Rect(cropRightEdge, cropTop, imageRightEdge - cropRightEdge, cropHeight)
            });
        }

        _overlayPath.Data = geometryGroup;
    }

    private Rect GetImageRect(double sourceWidth, double sourceHeight)
    {
        double controlWidth = ActualWidth;
        double controlHeight = ActualHeight;

        double renderWidth, renderHeight, offsetX, offsetY;

        switch (Stretch)
        {
            case Stretch.None:
                renderWidth = sourceWidth;
                renderHeight = sourceHeight;
                offsetX = (controlWidth - renderWidth) / 2;
                offsetY = (controlHeight - renderHeight) / 2;
                break;

            case Stretch.Fill:
                renderWidth = controlWidth;
                renderHeight = controlHeight;
                offsetX = 0;
                offsetY = 0;
                break;

            case Stretch.Uniform:
                double scale = Math.Min(controlWidth / sourceWidth, controlHeight / sourceHeight);
                renderWidth = sourceWidth * scale;
                renderHeight = sourceHeight * scale;
                offsetX = (controlWidth - renderWidth) / 2;
                offsetY = (controlHeight - renderHeight) / 2;
                break;

            case Stretch.UniformToFill:
                double scaleFill = Math.Max(controlWidth / sourceWidth, controlHeight / sourceHeight);
                renderWidth = sourceWidth * scaleFill;
                renderHeight = sourceHeight * scaleFill;
                offsetX = (controlWidth - renderWidth) / 2;
                offsetY = (controlHeight - renderHeight) / 2;
                break;

            default:
                return Rect.Empty;
        }

        return new Rect(offsetX, offsetY, renderWidth, renderHeight);
    }

    private double GetSourceWidth()
    {
        if (Source is BitmapSource bitmapSource)
        {
            return bitmapSource.PixelWidth;
        }
        return ActualWidth;
    }

    private double GetSourceHeight()
    {
        if (Source is BitmapSource bitmapSource)
        {
            return bitmapSource.PixelHeight;
        }
        return ActualHeight;
    }
}