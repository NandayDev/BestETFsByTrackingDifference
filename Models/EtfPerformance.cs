namespace BestETFsByTD.Models
{
    public record EtfPerformance(int Year, double FundPerformance, double IndexPerformance)
    {
        public double TrackingDifference { get; } = FundPerformance - IndexPerformance;
    }
}
