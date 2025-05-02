using System.Globalization;

namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    public class ThreePartDate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="title">Title to be shown at the top of the control</param>
        /// <param name="firstHint">First hint to shown below the title</param>
        /// <param name="secondHint">Second hint to be shown below the first hint</param>
        /// <param name="titleToBeUsedInErrorMessage">Alternative title to be used in any error messages</param>
        /// <param name="showTheHighlightBar">Is the red bar on the left hand side of the control required</param>
        public ThreePartDate(DateTime source, string title, string firstHint, string secondHint, string titleToBeUsedInErrorMessage, bool showTheHighlightBar = true)
        {
            Day = source.Day;
            Month = source.Month;
            Year = source.Year;

            Title = title;

            TitleToBeUsedInErrorMessage
                = string.IsNullOrEmpty(titleToBeUsedInErrorMessage)
                    ? Title
                    : titleToBeUsedInErrorMessage;

            FirstHint = firstHint;
            SecondHint = secondHint;
            ErrorMessage = string.Empty;
            ShowTheHighlightBar = showTheHighlightBar;
        }

        public ThreePartDate()
        {
            Title = string.Empty;
            TitleToBeUsedInErrorMessage = string.Empty;
            FirstHint = string.Empty;
            SecondHint = string.Empty;
            ErrorMessage = string.Empty;
        }

        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public bool HasError { get; set; }
        public bool HasDayError { get; set; }
        public bool HasMonthError { get; set; }
        public bool HasYearError { get; set; }
        public string ErrorMessage { get; set; }

        public bool ShowTheHighlightBar { get; init; }

        public string Title { get; init; }
        public string TitleToBeUsedInErrorMessage { get; init; }
        public string FirstHint { get; init; }
        public string SecondHint { get; init; }

        private static string ErrorInvalidDay => "A valid date must have a correct input for Day.";
        private static string ErrorInvalidMonth => "A valid date must have a correct input for Month.";
        private static string ErrorInvalidYear => "A valid date must have a correct input for Year.";
        private static string ErrorInvalidLeapYear => "It is not a leap year. February only has 28 days.";
        private static string ErrorInvalidDate => "Please provide a valid date.";

        private string ErrorEmptyDay => $"The date for '{TitleToBeUsedInErrorMessage}' must include the Day.";
        private string ErrorEmptyMonth => $"The date for '{TitleToBeUsedInErrorMessage}' must include the Month.";
        private string ErrorEmptyYear => $"The date for '{TitleToBeUsedInErrorMessage}' must include the Year.";
        private string ErrorTwoEmptyBox => $"The date for '{TitleToBeUsedInErrorMessage}' cannot be left blank.";


        public bool HasErrors()
        {
            if (Day == 0 && Month == 0 && Year == 0)
            {
                HasError = true;
                HasDayError = true;
                HasMonthError = true;
                HasYearError = true;
                ErrorMessage = ErrorTwoEmptyBox;
                return true;
            }

            if (Day == 0 && Month == 0 || Day == 0 && Year == 0 || Month == 0 && Year == 0)
            {
                HasError = true;
                HasDayError = true;
                HasMonthError = true;
                HasYearError = true;
                ErrorMessage = ErrorTwoEmptyBox;
                return true;
            }

            if (Day == 0)
            {
                HasError = true;
                HasDayError = true;
                ErrorMessage = ErrorEmptyDay;
                return true;
            }

            if (Day is < 1 or > 31)
            {
                HasError = true;
                HasDayError = true;
                ErrorMessage = ErrorInvalidDay;
                return true;
            }

            if (Month == 0)
            {
                HasError = true;
                HasMonthError = true;
                ErrorMessage = ErrorEmptyMonth;
                return true;
            }

            if (Month is < 1 or > 12)
            {
                HasError = true;
                HasMonthError = true;
                ErrorMessage = ErrorInvalidMonth;
                return true;
            }

            if (Year == 0)
            {
                HasError = true;
                HasYearError = true;
                ErrorMessage = ErrorEmptyYear;
                return true;
            }

            if (Year < 1000)  // less than 4 Digit year is not allowed
            {
                HasError = true;
                HasYearError = true;
                ErrorMessage = ErrorInvalidYear;
                return true;
            }

            // check for February, April, June, September, November. 
            if (Month is 2 or 4 or 6 or 9 or 11 && Day > 30)
            {
                HasError = true;
                HasDayError = true;
                HasMonthError = true;
                HasYearError = true;
                ErrorMessage = "The date contains a month that cannot have 31 days.";
                return true;
            }

            CultureInfo provider = CultureInfo.InvariantCulture;
            var format = "yyyy-MM-dd";
            var dateString = $"{Year}-{Month:D2}-{Day:D2}";

            // leap year check
            if (Month == 2 && Day > 28)
            {
                if (!DateTime.IsLeapYear(Year))
                {
                    HasError = true;
                    HasDayError = true;
                    HasMonthError = true;
                    HasYearError = true;
                    ErrorMessage = ErrorInvalidLeapYear;
                    return true;
                }

                try
                {
                    _ = DateTime.ParseExact(dateString, format, provider);
                    return false;
                }
                catch (FormatException)
                {
                    HasError = true;
                    HasDayError = true;
                    HasMonthError = true;
                    HasYearError = true;
                    ErrorMessage = ErrorInvalidLeapYear;
                    return true;
                }
            }

            try
            {
                _ = DateTime.ParseExact(dateString, format, provider);
                return false;
            }
            catch (FormatException)
            {
                HasError = true;
                HasDayError = true;
                HasMonthError = true;
                HasYearError = true;
                ErrorMessage = ErrorInvalidDate;
                return true;
            }
        }
    }
}
