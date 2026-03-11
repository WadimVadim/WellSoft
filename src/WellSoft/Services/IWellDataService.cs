using System.IO;
using WellSoft.Models;

namespace WellSoft.Services;

public interface IWellDataService
{
    ValidationResult ProcessCsvFile(Stream fileStream);
    List<WellSummary> CalculateSummaries(List<Well> wells);
}
