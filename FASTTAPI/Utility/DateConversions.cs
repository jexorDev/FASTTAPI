namespace FASTTAPI.Utility
{
    public class DateConversions
    {
        public static string GetFormattedISODateTime(DateTime dateTime)
        {

            return $"{dateTime.Year}-{dateTime.Month.ToString().PadLeft(2, '0')}-{dateTime.Day.ToString().PadLeft(2, '0')}T{dateTime.Hour.ToString().PadLeft(2, '0')}:{dateTime.Minute.ToString().PadLeft(2, '0')}:{dateTime.Second.ToString().PadLeft(2, '0')}Z";
        }
    }
}
