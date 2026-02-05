using iiSUMediaScraper.ViewModels;
using iiSUMediaScraper.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace iiSUMediaScraper.DataTemplateSelectors;

public class MeadiaTemplateSelector : DataTemplateSelector
{
    protected override DataTemplate SelectTemplateCore(object item)
    {
        return SelectTemplate(item);
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplate(item);
    }

    private new DataTemplate SelectTemplate(object item)
    {
        if (item is ImageViewModel imageViewModel)
        {
            if (new string[] { "webp", "gif" }.Contains(imageViewModel.Extension?.ToLower().Replace(".", "")))
            {
                // Fall back to ImageTemplate if AnimatedImageTemplate is not set
                return AnimatedImageTemplate ?? ImageTemplate!;
            }

            return ImageTemplate!;
        }
        else if (item is VideoViewModel)
        {
            return VideoTemplate!;
        }

        return MediaTemplate ?? base.SelectTemplateCore(item);
    }

    public DataTemplate? ImageTemplate { get; set; }

    public DataTemplate? AnimatedImageTemplate { get; set; }

    public DataTemplate? VideoTemplate { get; set; }

    public DataTemplate? MediaTemplate { get; set; }
}
