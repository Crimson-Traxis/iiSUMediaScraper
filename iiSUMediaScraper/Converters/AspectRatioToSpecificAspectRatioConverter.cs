using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

/// <summary>
/// Converts a decimal aspect ratio to a known aspect ratio type if it matches within tolerance.
/// Returns the ratio value if recognized, null otherwise.
/// </summary>
public class AspectRatioToSpecificAspectRatioConverter : IValueConverter
{
    /// <summary>
    /// Converts a decimal aspect ratio to itself if it matches a known type, null otherwise.
    /// </summary>
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            if (GetAspectRatioTypeWithTolerance(d) != AspectRatioType.Unknown)
            {
                return d;
            }
        }

        return null;
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Known aspect ratio types.
    /// </summary>
    public enum AspectRatioType
    {
        /// <summary>
        /// Unknown aspect ratio.
        /// </summary>
        Unknown,

        /// <summary>
        /// 1:1 aspect ratio (square).
        /// </summary>
        OneToOne,

        /// <summary>
        /// 19:9 aspect ratio.
        /// </summary>
        NineteenToNine,

        /// <summary>
        /// 9:16 or 16:9 aspect ratio.
        /// </summary>
        NineToSixteen,

        /// <summary>
        /// 4:3 or 3:4 aspect ratio.
        /// </summary>
        FourToThree,

        /// <summary>
        /// 3:2 or 2:3 aspect ratio.
        /// </summary>
        ThreeToTwo
    }

    /// <summary>
    /// Gets the aspect ratio type for a given ratio value within tolerance.
    /// </summary>
    /// <param name="ratio">The decimal aspect ratio value.</param>
    /// <param name="tolerance">The tolerance for matching (default 0.01).</param>
    /// <returns>The matched AspectRatioType or Unknown.</returns>
    public static AspectRatioType GetAspectRatioTypeWithTolerance(double ratio, double tolerance = 0.01)
    {

        var ratioValues = new Dictionary<double, AspectRatioType>
        {
            { 1.0, AspectRatioType.OneToOne },           // 1:1
            { 19.0 / 9.0, AspectRatioType.NineteenToNine }, // 19:9
            { 9.0 / 16.0, AspectRatioType.NineToSixteen },  // 9:16
            { 16.0 / 9.0, AspectRatioType.NineToSixteen },  // 16:9 (inverse)
            { 4.0 / 3.0, AspectRatioType.FourToThree },     // 4:3
            { 3.0 / 4.0, AspectRatioType.FourToThree },     // 3:4 (inverse)
            { 3.0 / 2.0, AspectRatioType.ThreeToTwo },      // 3:2
            { 2.0 / 3.0, AspectRatioType.ThreeToTwo }       // 2:3 (inverse)
        };

        foreach (KeyValuePair<double, AspectRatioType> kvp in ratioValues)
        {
            if (Math.Abs(ratio - kvp.Key) < tolerance)
            {
                return kvp.Value;
            }
        }

        return AspectRatioType.Unknown;
    }
}