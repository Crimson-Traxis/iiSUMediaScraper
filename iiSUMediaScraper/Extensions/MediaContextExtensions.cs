using iiSUMediaScraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iiSUMediaScraper.Extensions;

public static class MediaContextExtensions
{
    public static void AddFrom(this MediaContext mediaContext, MediaContext mediaContext2)
    {
        mediaContext.Icons.AddRange(mediaContext2.Icons);
        mediaContext.Logos.AddRange(mediaContext2.Logos);
        mediaContext.Titles.AddRange(mediaContext2.Titles);
        mediaContext.Heros.AddRange(mediaContext2.Heros);
        mediaContext.Slides.AddRange(mediaContext2.Slides);
        mediaContext.Music.AddRange(mediaContext2.Music);
        mediaContext.Videos.AddRange(mediaContext2.Videos);
    }
}
