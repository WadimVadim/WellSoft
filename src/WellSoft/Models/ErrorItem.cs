namespace WellSoft.Models;

public class ErrorItem
{
    public int LineNumber { get; set; }
    public string WellId { get; set; }
    public string Message { get; set; }

    public ErrorItem(int lineNumber, string wellId, string message)
    {
        LineNumber = lineNumber;
        WellId = wellId;
        Message = message;
    }
}
