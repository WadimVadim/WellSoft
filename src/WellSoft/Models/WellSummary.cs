namespace WellSoft.Models;

public class WellSummary
{
    public string WellId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double TotalDepth { get; set; }
    public int IntervalCount { get; set; }
    public double AvgPorosity { get; set; }
    public string MostCommonRock { get; set; }
}
