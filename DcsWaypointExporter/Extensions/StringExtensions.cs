// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

namespace DcsWaypointExporter.Extensions
{
    public static class StringExtensions
    {
        public static string ToNonNull(this string? value)
        {
            if (value is null)
            {
                return string.Empty;
            }

            return value;
        }
    }
}
