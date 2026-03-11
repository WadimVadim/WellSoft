namespace WellSoft.Models;

public class Well
{
    public string WellId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public List<Interval> Intervals { get; set; } = new();

    public IEnumerable<ErrorItem> Validate()
    {
        if (Intervals.Select(i => (X, Y)).Distinct().Count() > 1)
        {
            foreach (var interval in Intervals)
                yield return new ErrorItem(interval.LineNumber, WellId,
                    "Координаты скважины не совпадают с другими интервалами");
        }

        var sorted = Intervals.OrderBy(i => i.DepthFrom).ThenBy(i => i.DepthTo).ToList();
        for (int i = 0; i < sorted.Count - 1; i++)
        {
            var current = sorted[i];
            var next = sorted[i + 1];
            if (next.DepthFrom < current.DepthTo)
            {
                yield return new ErrorItem(next.LineNumber, WellId,
                    $"Интервал пересекается с интервалом из строки {current.LineNumber} (DepthFrom {current.DepthFrom}-{current.DepthTo})");
            }
        }
    }
}