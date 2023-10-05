using DevExpress.Xpo.Metadata;

namespace Clockly.Models;

public sealed class TimeOnlyConverter : ValueConverter
{
    public override Type StorageType { get; } = typeof(DateTime);

    public override object ConvertFromStorageType(object value)
    {
        if (value is DateTime dateTime)
        {
            return TimeOnly.FromDateTime(dateTime);
        }

        return TimeOnly.MinValue;
    }

    public override object ConvertToStorageType(object value)
    {
        if (value is TimeOnly timeOnly)
        {
            return new DateTime(timeOnly.Ticks);
        }

        return DateTime.MinValue;
    }
}