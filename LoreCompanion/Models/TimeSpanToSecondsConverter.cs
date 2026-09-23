using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LoreCompanion.Models
{
    public class TimeSpanToSecondsConverter() : ValueConverter<TimeSpan, long>(
        v => (long)v.TotalSeconds,
        v => TimeSpan.FromSeconds(v));
}