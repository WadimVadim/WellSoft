namespace WellSoft.Models;

public class Interval
{
    public double DepthFrom { get; set; }
    public double DepthTo { get; set; }
    public string Rock { get; set; }
    public double Porosity { get; set; }
    public int LineNumber { get; set; }

    public IEnumerable<ErrorItem> Validate(string wellId)
    {
        if (DepthFrom < 0)
            yield return new ErrorItem(LineNumber, wellId, "DepthFrom должен быть >= 0");
        if (DepthFrom >= DepthTo)
            yield return new ErrorItem(LineNumber, wellId, "DepthFrom должно быть меньше DepthTo");
        if (string.IsNullOrWhiteSpace(Rock))
            yield return new ErrorItem(LineNumber, wellId, "Rock не может быть пустым");
        if (Porosity < 0 || Porosity > 1)
            yield return new ErrorItem(LineNumber, wellId, "Porosity должна быть в диапазоне [0, 1]");
    }
}
