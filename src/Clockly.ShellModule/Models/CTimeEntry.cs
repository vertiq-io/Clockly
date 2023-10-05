using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;

namespace Clockly.Models;

[Persistent("CTimeEntry")]
public class CTimeEntry : XPObject
{
    public CTimeEntry(Session session) : base(session) { }

 
    private string userId = "";
    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    public string UserId
    {
        get => userId;
        set => SetPropertyValue(nameof(UserId), ref userId, value);
    }

    private string? description;
    [Size(SizeAttribute.Unlimited)]
    public string? Description
    {
        get => description;
        set => SetPropertyValue(nameof(Description), ref description, value);
    }

    private DateOnly date;
    [Persistent]
    [ValueConverter(typeof(DateOnlyConverter))]
    public DateOnly Date
    {
        get => date;
        set => SetPropertyValue(nameof(Date), ref date, value);
    }

    private TimeSpan? from;
    [Persistent]
    [ValueConverter(typeof(TimeOnlyConverter))]
    public TimeSpan? TimeFrom
    {
        get => from;
        set => SetPropertyValue(nameof(TimeFrom), ref from, value);
    }
        
    private TimeSpan? timeTo;
    [Persistent]
    [ValueConverter(typeof(TimeOnlyConverter))]
    public TimeSpan? TimeTo
    {
        get => timeTo;
        set => SetPropertyValue(nameof(TimeTo), ref timeTo, value);
    }
}
