using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HomeApplianceRepairCenter.Utilities
{
    /// <summary>
    /// Static helper class that checks every value entered into the system.
    /// It has no state, so it is declared static and cannot be instantiated.
    /// </summary>
    public static class Validator
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const double MaxLaborHours = 200;
        private const int MinYear = 2000;

        // Serial number format: 3-letter type prefix, a dash, 4 digits. Example: REF-1001
        private static readonly Regex SerialPattern = new Regex(@"^[A-Z]{3}-\d{4}$");

        /// <summary>Checks the serial format and that it starts with the prefix of its appliance type.</summary>
        public static bool IsValidSerialNumber(string serialNumber, string expectedPrefix)
        {
            return !string.IsNullOrWhiteSpace(serialNumber)
                && SerialPattern.IsMatch(serialNumber)
                && serialNumber.StartsWith(expectedPrefix + "-", StringComparison.Ordinal);
        }

        /// <summary>A name (owner or brand) must contain 2 to 50 characters and at least one letter.</summary>
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            string trimmed = name.Trim();
            bool hasLetter = false;
            foreach (char c in trimmed)
            {
                if (char.IsLetter(c))
                {
                    hasLetter = true;
                    break;
                }
            }
            return hasLetter && trimmed.Length >= 2 && trimmed.Length <= 50;
        }

        /// <summary>An appliance cannot be received in the future or before the year 2000.</summary>
        public static bool IsValidReceivedDate(DateTime date)
        {
            return date.Date <= DateTime.Today && date.Year >= MinYear;
        }

        /// <summary>The promised delivery date cannot be before the day the appliance was received.</summary>
        public static bool IsValidExpectedDeliveryDate(DateTime receivedDate, DateTime expectedDate)
        {
            return expectedDate.Date >= receivedDate.Date;
        }

        public static bool IsValidLaborHours(double hours)
        {
            return hours >= 0 && hours <= MaxLaborHours;
        }

        public static bool IsNonNegative(decimal amount)
        {
            return amount >= 0;
        }

        public static bool IsPositive(double value)
        {
            return value > 0;
        }

        /// <summary>Parses a date written exactly as yyyy-MM-dd (example: 2026-11-12).</summary>
        public static bool TryParseDate(string input, out DateTime date)
        {
            return DateTime.TryParseExact(input?.Trim(), DateFormat, CultureInfo.InvariantCulture,
                                          DateTimeStyles.None, out date);
        }
    }
}
