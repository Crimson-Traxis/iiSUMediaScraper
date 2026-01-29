using Microsoft.UI.Xaml.Data;

namespace iiSUMediaScraper.Converters;

public class AspectRatioToSpecificAspectRatioConverter : IValueConverter
{
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

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    public enum AspectRatioType
    {
        Unknown,
        OneToOne,      // 1:1
        NineteenToNine, // 19:9
        NineToSixteen, // 9:16
        FourToThree,   // 4:3
        ThreeToTwo     // 3:2
    }

    public static AspectRatioType GetAspectRatioTypeWithTolerance(double ratio, double tolerance = 0.01)
    {

        Dictionary<double, AspectRatioType> ratioValues = new Dictionary<double, AspectRatioType>
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