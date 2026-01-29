namespace iiSUMediaScraper.Models;

public class Media
{
    public string? Url { get; set; }

    public string? Extension { get; set; }

    public byte[] Bytes { get; set; } = [];

    public SourceFlag Source { get; set; }
}
