using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace iiSUMediaScraper.Controls;

/// <summary>
/// A drop zone control that accepts file drops with visual feedback.
/// </summary>
public sealed class DropZone : ContentControl
{
    #region Dependency Properties

    /// <summary>
    /// The message displayed when files are being dragged over the control.
    /// </summary>
    public static readonly DependencyProperty DropMessageProperty =
        DependencyProperty.Register(
            nameof(DropMessage),
            typeof(string),
            typeof(DropZone),
            new PropertyMetadata("Drop Files"));

    /// <summary>
    /// The default border brush when not hovering.
    /// </summary>
    public static readonly DependencyProperty DefaultBorderBrushProperty =
        DependencyProperty.Register(
            nameof(DefaultBorderBrush),
            typeof(Brush),
            typeof(DropZone),
            new PropertyMetadata(null));

    /// <summary>
    /// The border brush when files are being dragged over.
    /// </summary>
    public static readonly DependencyProperty HoverBorderProperty =
        DependencyProperty.Register(
            nameof(HoverBorder),
            typeof(Brush),
            typeof(DropZone),
            new PropertyMetadata(null));

    /// <summary>
    /// The backdrop brush displayed when files are being dragged over.
    /// </summary>
    public static readonly DependencyProperty BackdropBrushProperty =
        DependencyProperty.Register(
            nameof(BackdropBrush),
            typeof(Brush),
            typeof(DropZone),
            new PropertyMetadata(null));

    /// <summary>
    /// The scale factor applied when files are dragged over (default 1.02).
    /// </summary>
    public static readonly DependencyProperty HoverScaleProperty =
        DependencyProperty.Register(
            nameof(HoverScale),
            typeof(double),
            typeof(DropZone),
            new PropertyMetadata(1.02));

    /// <summary>
    /// Indicates whether files are currently being dragged over the control.
    /// </summary>
    public static readonly DependencyProperty IsDragOverProperty =
        DependencyProperty.Register(
            nameof(IsDragOver),
            typeof(bool),
            typeof(DropZone),
            new PropertyMetadata(false, OnIsDragOverChanged));

    /// <summary>
    /// Comma-separated list of accepted file extensions (e.g., "png, jpg, gif").
    /// If empty or null, all extensions are accepted.
    /// </summary>
    public static readonly DependencyProperty AcceptedExtensionsProperty =
        DependencyProperty.Register(
            nameof(AcceptedExtensions),
            typeof(string),
            typeof(DropZone),
            new PropertyMetadata(null));

    #endregion

    #region Properties

    public string DropMessage
    {
        get => (string)GetValue(DropMessageProperty);
        set => SetValue(DropMessageProperty, value);
    }

    public Brush DefaultBorderBrush
    {
        get => (Brush)GetValue(DefaultBorderBrushProperty);
        set => SetValue(DefaultBorderBrushProperty, value);
    }

    public Brush HoverBorder
    {
        get => (Brush)GetValue(HoverBorderProperty);
        set => SetValue(HoverBorderProperty, value);
    }

    public Brush BackdropBrush
    {
        get => (Brush)GetValue(BackdropBrushProperty);
        set => SetValue(BackdropBrushProperty, value);
    }

    public double HoverScale
    {
        get => (double)GetValue(HoverScaleProperty);
        set => SetValue(HoverScaleProperty, value);
    }

    public bool IsDragOver
    {
        get => (bool)GetValue(IsDragOverProperty);
        private set => SetValue(IsDragOverProperty, value);
    }

    /// <summary>
    /// Gets or sets a comma-separated list of accepted file extensions (e.g., "png, jpg, gif").
    /// If empty or null, all extensions are accepted.
    /// </summary>
    public string AcceptedExtensions
    {
        get => (string)GetValue(AcceptedExtensionsProperty);
        set => SetValue(AcceptedExtensionsProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when files are dropped onto the control.
    /// </summary>
    public event EventHandler<FilesDroppedEventArgs> FilesDropped;

    #endregion

    #region Constructor

    public DropZone()
    {
        this.DefaultStyleKey = typeof(DropZone);
        this.AllowDrop = true;

        this.DragEnter += OnDragEnter;
        this.DragOver += OnDragOver;
        this.DragLeave += OnDragLeave;
        this.Drop += OnDrop;
    }

    #endregion

    #region Event Handlers

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            IsDragOver = true;
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = DropMessage;
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        IsDragOver = false;
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        IsDragOver = false;

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            IReadOnlyList<IStorageItem> items = await e.DataView.GetStorageItemsAsync();
            List<StorageFile> files = [];

            foreach (IStorageItem? item in items)
            {
                if (item is StorageFile file && IsAcceptedFile(file))
                {
                    files.Add(file);
                }
            }

            if (files.Count > 0)
            {
                FilesDropped?.Invoke(this, new FilesDroppedEventArgs(files));
            }
        }
    }

    private bool IsAcceptedFile(StorageFile file)
    {
        if (string.IsNullOrWhiteSpace(AcceptedExtensions))
        {
            return true;
        }

        List<string> acceptedList = AcceptedExtensions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ext => ext.StartsWith('.') ? ext : "." + ext)
            .ToList();

        return acceptedList.Any(ext =>
            file.FileType.Equals(ext, StringComparison.OrdinalIgnoreCase));
    }

    private static void OnIsDragOverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DropZone dropZone)
        {
            dropZone.UpdateVisualState();
        }
    }

    #endregion

    #region Methods

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        VisualStateManager.GoToState(this, IsDragOver ? "DragOver" : "Normal", true);
    }

    #endregion
}

/// <summary>
/// Event arguments for the FilesDropped event.
/// </summary>
public class FilesDroppedEventArgs : EventArgs
{
    public IReadOnlyList<StorageFile> Files { get; }

    public FilesDroppedEventArgs(IReadOnlyList<StorageFile> files)
    {
        Files = files;
    }
}