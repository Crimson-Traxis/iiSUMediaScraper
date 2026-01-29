namespace iiSUMediaScraper.Models;

public class Image : Media
{
    public int Width { get; set; }

    public int Height { get; set; }

    public Crop? Crop { get; set; }
}
