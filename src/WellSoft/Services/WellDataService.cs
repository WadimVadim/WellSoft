using System.Globalization;
using System.IO;
using WellSoft.Models;

namespace WellSoft.Services;

public class WellDataService : IWellDataService
{
    public ValidationResult ProcessCsvFile(Stream fileStream)
    {
        var result = new ValidationResult();
        var lines = new List<string>();

        using (var reader = new StreamReader(fileStream))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
                lines.Add(line);
        }

        if (lines.Count == 0)
            return result;

        var wellsDict = new Dictionary<string, Well>();

        for (int i = 0; i < lines.Count; i++)
        {
            int lineNumber = i + 1;
            var fields = lines[i].Split(';');

            if (fields.Length != 7)
            {
                result.Errors.Add(new ErrorItem(lineNumber, fields.Length > 0 ? fields[0] : "",
                    "Неверное количество полей (ожидалось 7)"));
                continue;
            }

            string wellId = fields[0].Trim();
            if (string.IsNullOrWhiteSpace(wellId))
            {
                result.Errors.Add(new ErrorItem(lineNumber, "", "WellId не может быть пустым"));
                continue;
            }

            if (!double.TryParse(fields[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double x))
            {
                result.Errors.Add(new ErrorItem(lineNumber, wellId, "Некорректное значение X"));
                continue;
            }
            if (!double.TryParse(fields[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
            {
                result.Errors.Add(new ErrorItem(lineNumber, wellId, "Некорректное значение Y"));
                continue;
            }
            if (!double.TryParse(fields[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double depthFrom))
            {
                result.Errors.Add(new ErrorItem(lineNumber, wellId, "Некорректное значение DepthFrom"));
                continue;
            }
            if (!double.TryParse(fields[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double depthTo))
            {
                result.Errors.Add(new ErrorItem(lineNumber, wellId, "Некорректное значение DepthTo"));
                continue;
            }
            string rock = fields[5].Trim();
            if (!double.TryParse(fields[6], NumberStyles.Any, CultureInfo.InvariantCulture, out double porosity))
            {
                result.Errors.Add(new ErrorItem(lineNumber, wellId, "Некорректное значение Porosity"));
                continue;
            }

            var interval = new Interval
            {
                DepthFrom = depthFrom,
                DepthTo = depthTo,
                Rock = rock,
                Porosity = porosity,
                LineNumber = lineNumber
            };

            var intervalErrors = interval.Validate(wellId).ToList();
            if (intervalErrors.Any())
            {
                result.Errors.AddRange(intervalErrors);
                continue;
            }

            if (!wellsDict.TryGetValue(wellId, out var well))
            {
                well = new Well { WellId = wellId, X = x, Y = y };
                wellsDict[wellId] = well;
            }
            else
            {
                if (Math.Abs(well.X - x) > 1e-9 || Math.Abs(well.Y - y) > 1e-9)
                {
                    result.Errors.Add(new ErrorItem(lineNumber, wellId,
                        "Координаты скважины не совпадают с ранее загруженными интервалами"));
                    continue;
                }
            }

            well.Intervals.Add(interval);
        }

        foreach (var well in wellsDict.Values)
        {
            var wellErrors = well.Validate().ToList();
            if (wellErrors.Any())
                result.Errors.AddRange(wellErrors);
            else
                result.Wells.Add(well);
        }

        return result;
    }

    public List<WellSummary> CalculateSummaries(List<Well> wells)
    {
        var summaries = new List<WellSummary>();
        foreach (var well in wells)
        {
            double totalDepth = well.Intervals.Max(i => i.DepthTo);
            int intervalCount = well.Intervals.Count;

            double totalThickness = 0;
            double weightedPorositySum = 0;
            foreach (var interval in well.Intervals)
            {
                double thickness = interval.DepthTo - interval.DepthFrom;
                totalThickness += thickness;
                weightedPorositySum += thickness * interval.Porosity;
            }
            double avgPorosity = totalThickness > 0 ? weightedPorositySum / totalThickness : 0;

            var rockGroups = well.Intervals
                .GroupBy(i => i.Rock)
                .Select(g => new { Rock = g.Key, TotalThickness = g.Sum(i => i.DepthTo - i.DepthFrom) })
                .OrderByDescending(x => x.TotalThickness)
                .ThenBy(x => x.Rock);
            string mostCommonRock = rockGroups.FirstOrDefault()?.Rock ?? "N/A";

            summaries.Add(new WellSummary
            {
                WellId = well.WellId,
                X = well.X,
                Y = well.Y,
                TotalDepth = totalDepth,
                IntervalCount = intervalCount,
                AvgPorosity = avgPorosity,
                MostCommonRock = mostCommonRock
            });
        }
        return summaries;
    }
}
