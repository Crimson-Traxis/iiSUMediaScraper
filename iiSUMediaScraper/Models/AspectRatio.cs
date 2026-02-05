namespace iiSUMediaScraper.Models;

/// <summary>
/// Represents an aspect ratio as a reduced fraction (e.g., 16:9).
/// </summary>
public readonly struct AspectRatio
{
    /// <summary>
    /// Gets the horizontal component of the aspect ratio.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the vertical component of the aspect ratio.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Gets the decimal value of the aspect ratio (width / height).
    /// </summary>
    public double Value => (double)X / Y;

    /// <summary>
    /// Initializes a new instance of the AspectRatio struct from width and height values.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    public AspectRatio(int width, int height)
    {
        int gcd = GCD(width, height);
        X = width / gcd;
        Y = height / gcd;
    }

    /// <summary>
    /// Calculates the greatest common divisor of two integers.
    /// </summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
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

    /// <summary>
    /// Returns a string representation of the aspect ratio (e.g., "16:9").
    /// </summary>
    public override string ToString() => $"{X}:{Y}";
}
