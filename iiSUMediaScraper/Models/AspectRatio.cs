namespace iiSUMediaScraper.Models;

public readonly struct AspectRatio
{
    public int X { get; }
    public int Y { get; }
    public double Value => (double)X / Y;

    public AspectRatio(int width, int height)
    {
        int gcd = GCD(width, height);
        X = width / gcd;
        Y = height / gcd;
    }

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

    public override string ToString() => $"{X}:{Y}";
}
