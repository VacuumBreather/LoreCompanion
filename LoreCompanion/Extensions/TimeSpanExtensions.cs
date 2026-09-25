namespace LoreCompanion.Extensions
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Clamp(this TimeSpan time, TimeSpan min, TimeSpan max)
        {
            if (time < min)
            {
                return min;
            }

            if (time > max)
            {
                return max;
            }

            return time;
        }
    }
}