using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.IO;

namespace iiSUMediaScraper.Controls;

/// <summary>
/// A control that displays animated images (WebP, GIF) using WebView2.
/// </summary>
public class AnimatedImageWebView : Control
{
    private static readonly string TempFolder = Path.Combine(Path.GetTempPath(), "iiSUMediaScraper_AnimatedImages");
    private Grid? _rootGrid;
    private WebView2? _webView;
    private bool _isInitialized;
    private string? _pendingImagePath;
    private string? _tempFilePath;

    public AnimatedImageWebView()
    {
        DefaultStyleKey = typeof(AnimatedImageWebView);
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
        RegisterPropertyChangedCallback(VisibilityProperty, OnVisibilityChanged);
    }

    private async void OnVisibilityChanged(DependencyObject sender, DependencyProperty dp)
    {
        var isVisible = Visibility == Visibility.Visible;

        // When becoming visible, ensure WebView2 is initialized and load image
        if (isVisible && _webView != null && !_isInitialized)
        {
            try
            {
                await _webView.EnsureCoreWebView2Async();

                _webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                _webView.DefaultBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);

                _isInitialized = true;

                // Load pending image or current ImagePath
                var pathToLoad = _pendingImagePath ?? ImagePath;
                if (!string.IsNullOrWhiteSpace(pathToLoad))
                {
                    LoadImage(pathToLoad);
                    _pendingImagePath = null;
                }
            }
            catch
            {
                // WebView2 runtime not available
            }
        }
        else if (isVisible && _isInitialized)
        {
            // Already initialized, load pending image or current ImagePath
            var pathToLoad = _pendingImagePath ?? ImagePath;
            if (!string.IsNullOrWhiteSpace(pathToLoad))
            {
                LoadImage(pathToLoad);
                _pendingImagePath = null;
            }
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_webView != null && e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            _webView.Width = e.NewSize.Width;
            _webView.Height = e.NewSize.Height;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        CleanupTempFile();
    }

    private void CleanupTempFile()
    {
        if (!string.IsNullOrEmpty(_tempFilePath) && File.Exists(_tempFilePath))
        {
            try { File.Delete(_tempFilePath); } catch { }
            _tempFilePath = null;
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _rootGrid = GetTemplateChild("PART_RootGrid") as Grid;
        _webView = GetTemplateChild("PART_WebView") as WebView2;

        if (_rootGrid != null)
        {
            _rootGrid.SizeChanged += RootGrid_SizeChanged;
        }

        if (_webView != null)
        {
            _webView.Loaded += WebView_Loaded;
        }
    }

    private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_webView != null && e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            _webView.Width = e.NewSize.Width;
            _webView.Height = e.NewSize.Height;
        }
    }

    private async void WebView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_webView != null && !_isInitialized)
        {
            try
            {
                await _webView.EnsureCoreWebView2Async();

                _webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;

                // Make background transparent
                _webView.DefaultBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);

                _isInitialized = true;

                // Load pending image if any
                if (!string.IsNullOrWhiteSpace(_pendingImagePath))
                {
                    LoadImage(_pendingImagePath);
                }
            }
            catch
            {
                // WebView2 runtime not available
            }
        }
    }

    public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register(
        nameof(ImagePath),
        typeof(string),
        typeof(AnimatedImageWebView),
        new PropertyMetadata(null, OnImagePathChanged));

    public string? ImagePath
    {
        get => (string?)GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }

    private static void OnImagePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AnimatedImageWebView control && e.NewValue is string path)
        {
            control.LoadImage(path);
        }
    }

    private void LoadImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        // Strip file:/// prefix if present
        if (path.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
        {
            path = path.Substring(8).Replace('/', '\\');
        }

        if (!_isInitialized || _webView == null)
        {
            _pendingImagePath = path;
            return;
        }

        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            CleanupTempFile();

            // Ensure temp folder exists
            Directory.CreateDirectory(TempFolder);

            // Read file to detect type from magic bytes
            var bytes = File.ReadAllBytes(path);
            var extension = DetectExtension(bytes, path);

            // Copy to temp folder with correct extension so WebView2 serves correct MIME type
            var tempFileName = $"{Guid.NewGuid()}{extension}";
            _tempFilePath = Path.Combine(TempFolder, tempFileName);
            File.WriteAllBytes(_tempFilePath, bytes);

            // Map the temp folder to virtual host
            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "localimages",
                TempFolder,
                CoreWebView2HostResourceAccessKind.Allow);

            // Create HTML that loads via virtual host
            var html = $@"<!DOCTYPE html>
<html>
<head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        html, body {{ width: 100vw; height: 100vh; overflow: hidden; background: transparent; }}
        img {{ position: absolute; top: 0; left: 0; width: 100vw; height: 100vh; object-fit: cover; }}
    </style>
</head>
<body>
    <img src=""https://localimages/{tempFileName}"" />
</body>
</html>";

            _webView.NavigateToString(html);
        }
        catch
        {
            // Failed to load image
        }
    }

    private static string DetectExtension(byte[] bytes, string path)
    {
        // Check magic bytes first
        if (bytes.Length >= 4)
        {
            // WebP: RIFF....WEBP
            if (bytes.Length >= 12 && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
            {
                return ".webp";
            }

            // GIF: GIF87a or GIF89a
            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
            {
                return ".gif";
            }

            // PNG: 89 50 4E 47
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return ".png";
            }

            // JPEG: FF D8 FF
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            {
                return ".jpg";
            }
        }

        // Fall back to original extension if valid
        var ext = Path.GetExtension(path).ToLower();
        return ext switch
        {
            ".webp" or ".gif" or ".png" or ".jpg" or ".jpeg" => ext,
            _ => ".webp"
        };
    }
}
