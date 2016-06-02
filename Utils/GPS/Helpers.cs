using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Utils.GPS.SerialGPS;

/* Collection of methods to deal with GPS data and converstions. */

namespace Utils.GPS
{
    public class Helpers
    {
        public static IStopwatch stopwatch = new StopwatchWrapper();

        /* 
            Converts DateTime to Unix time stamp
            http://stackoverflow.com/questions/249760/how-to-convert-a-unix-timestamp-to-datetime-and-vice-versa
        */
        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /* Regexp that parses GPRMC messages */
        const string gprmc = @"\$GPRMC,(?<Hour>\d{2})(?<Minute>\d{2})" +
            @"(?<Second>\d{2}),(?<Active>[A,V]),(?<Latitude>\d+\.\d+)," +
            @"(?<NS>[NS]),(?<Longitude>\d+\.\d+),(?<EW>[EW])," +
            @"(?<Speed>\d+\.\d+),(?<Angle>\d+\.\d+)," +
            @"(?<Day>\d{2})(?<Month>\d{2})(?<Year>\d{2}),.*";

        /* Checks if the message is a GPSRMC message */
        public static bool isGpsrmc(string msg)
        {
            Regex rgx = new Regex(gprmc);
            Match match = rgx.Match(msg);
            return match.Success;
        }

        /* Parses GPSRMC message and returns a DateTime object. Throws an 
        ArgumentException if the message can't be parsed. */
        // TODO -- make it check the checksum
        public static GpsData ParseGpsrmc(string msg)
        {
            var ticks = stopwatch.GetTimestamp();
            Regex rgx = new Regex(gprmc);
            Match match = rgx.Match(msg);
            if (!match.Success)
            {
                throw new ArgumentException(msg + " is not a valid GPSRMC message");
            }

            var year = int.Parse(match.Groups["Year"].Value) +
                (DateTime.Now.Year / 100) * 100;
            var month = int.Parse(match.Groups["Month"].Value);
            var day = int.Parse(match.Groups["Day"].Value);
            var hour = int.Parse(match.Groups["Hour"].Value);
            var minute = int.Parse(match.Groups["Minute"].Value);
            var second = int.Parse(match.Groups["Second"].Value);

            return new GpsData()
            {
                ticks = ticks,
                timestamp = new DateTime(year, month, day, hour, minute, second,
                    DateTimeKind.Local),
                valid = match.Groups["Active"].Value == "A",
                longitude = double.Parse(match.Groups["Longitude"].Value),
                latitude = double.Parse(match.Groups["Latitude"].Value),
                speedKnots = double.Parse(match.Groups["Speed"].Value),
                angleDegrees = double.Parse(match.Groups["Angle"].Value),
                active = match.Groups["Active"].Value,
                ns = match.Groups["NS"].Value,
                ew = match.Groups["EW"].Value
            };                
        }   
    }
}
