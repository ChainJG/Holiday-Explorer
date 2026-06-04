namespace HolidayExplorer.Core.Models
{
    public sealed class MoneyRange
    {
        public decimal Minimum { get; init; }

        public decimal Maximum { get; init; }

        public string CurrencySymbol { get; init; } = "£";

        public string DisplayText => $"{CurrencySymbol}{Minimum:0}–{CurrencySymbol}{Maximum:0}";
    }
}
