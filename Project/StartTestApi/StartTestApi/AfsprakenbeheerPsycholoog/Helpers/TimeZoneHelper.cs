using System;

namespace AfsprakenbeheerPsycholoog.Helpers
{
    /// <summary>
    /// Helper voor het uniform ophalen van de Nederlandse tijdzone (Europe/Amsterdam of W. Europe Standard Time).
    /// </summary>
    public static class TimeZoneHelper
    {
        private static readonly Lazy<TimeZoneInfo> _dutchTimeZone = new Lazy<TimeZoneInfo>(() =>
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                }
                catch
                {
                    return TimeZoneInfo.Local;
                }
            }
            catch
            {
                return TimeZoneInfo.Local;
            }
        });

        public static TimeZoneInfo DutchTimeZone => _dutchTimeZone.Value;
    }
}
