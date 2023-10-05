using DevExpress.Xpo.Metadata;

namespace Clockly.Models;

public sealed class DateOnlyConverter : ValueConverter
{
    public override Type StorageType { get; } = typeof(DateTime);

    public override object ConvertFromStorageType(object value)
    {
        if (value is DateTime dt)
        {
            return DateOnly.FromDateTime(dt);
        }

        return DateOnly.MinValue;
    }

    public override object ConvertToStorageType(object value)
    {
        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToDateTime(TimeOnly.MinValue);
        }

        return DateTime.MinValue;
    }
}
