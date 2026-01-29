namespace iiSUMediaScraper.Helpers;

/// <summary>
/// Provides utilities for determining and checking aspect ratios of images.
/// </summary>
public static class AspectRatioHelper
{
    /// <summary>
    /// Defines common aspect ratio types.
    /// </summary>
    public enum AspectRatioType
    {
        Unknown,
        OneToOne,      // 1:1 (Square)
        NineteenToNine, // 19:9 (Ultra-wide)
        NineToSixteen, // 9:16 (Portrait/Landscape)
        FourToThree,   // 4:3 (Classic)
        ThreeToTwo     // 3:2 (Standard photo)
    }

    /// <summary>
    /// Maps reduced ratio tuples to their aspect ratio types.
    /// Includes both orientations for non-square ratios.
    /// </summary>
    private static readonly Dictionary<(int, int), AspectRatioType> KnownRatios = new()
    {
        { (1, 1), AspectRatioType.OneToOne },
        { (19, 9), AspectRatioType.NineteenToNine },
        { (9, 16), AspectRatioType.NineToSixteen },
        { (16, 9), AspectRatioType.NineToSixteen }, // Also check inverse
        { (4, 3), AspectRatioType.FourToThree },
        { (3, 4), AspectRatioType.FourToThree },    // Also check inverse
        { (3, 2), AspectRatioType.ThreeToTwo },
        { (2, 3), AspectRatioType.ThreeToTwo }      // Also check inverse
    };

    /// <summary>
    /// Determines the aspect ratio type of an image based on its dimensions.
    /// Reduces the dimensions by their GCD before checking.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <returns>The aspect ratio type, or Unknown if not recognized.</returns>
    public static AspectRatioType GetAspectRatioType(int width, int height)
    {
        int gcd = GCD(width, height);
        int ratioX = width / gcd;
        int ratioY = height / gcd;

        if (KnownRatios.TryGetValue((ratioX, ratioY), out AspectRatioType ratioType))
        {
            return ratioType;
        }

        return AspectRatioType.Unknown;
    }

    /// <summary>
    /// Checks whether the dimensions correspond to a known aspect ratio.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <returns>True if the aspect ratio is recognized, false otherwise.</returns>
    public static bool IsKnownAspectRatio(int width, int height)
    {
        return GetAspectRatioType(width, height) != AspectRatioType.Unknown;
    }

    /// <summary>
    /// Checks whether the dimensions match a specific aspect ratio type.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="type">The aspect ratio type to check against.</param>
    /// <returns>True if the dimensions match the specified aspect ratio type.</returns>
    public static bool IsAspectRatio(int width, int height, AspectRatioType type)
    {
        return GetAspectRatioType(width, height) == type;
    }

    /// <summary>
    /// Calculates the greatest common divisor of two numbers using Euclid's algorithm.
    /// </summary>
    /// <param name="a">First number.</param>
    /// <param name="b">Second number.</param>
    /// <returns>The greatest common divisor.</returns>
    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}