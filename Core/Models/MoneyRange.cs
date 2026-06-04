namespace Holiday_Explorer.Core.Models
{
    public sealed class MoneyRange
    {
        public required decimal Minimum { get; init; }

        public required decimal Maximum { get; init; }

        public required string CurrencySymbol { get; init; }

        public string DisplayText => $"{CurrencySymbol}{Minimum:0}–{CurrencySymbol}{Maximum:0}";
    }
}
