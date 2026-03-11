namespace WellSoft.Models;

public class ValidationResult
{
    public List<Well> Wells { get; set; } = new();
    public List<ErrorItem> Errors { get; set; } = new();
}