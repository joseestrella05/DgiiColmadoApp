using DgiiColmadoApp.Models;
using System.Text.Json;

namespace DgiiColmadoApp.Services;

public class DgiiReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _storagePath;
    private readonly object _sync = new();
    private readonly List<DgiiReportRecord> _records = [];

    public DgiiReportService()
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _storagePath = Path.Combine(dataDirectory, "dgii-reports.json");
        LoadFromDisk();
    }

    public IReadOnlyList<DgiiReportRecord> GetAll() =>
        _records
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

    public IReadOnlyList<string> GetAvailablePeriods() =>
        _records
            .Select(x => x.Data.FiscalPeriod.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public DgiiMonthlyReportSummary? GetMonthlySummary(string fiscalPeriod)
    {
        if (string.IsNullOrWhiteSpace(fiscalPeriod))
        {
            return null;
        }

        var periodRecords = _records
            .Where(x => string.Equals(x.Data.FiscalPeriod, fiscalPeriod, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.CreatedAt)
            .ToList();

        if (periodRecords.Count == 0)
        {
            return null;
        }

        var totalSales = periodRecords.Sum(x => x.Data.GrossSales);
        var totalItbis = periodRecords.Sum(x => x.Data.ItbisCollected);
        var estimatedNetSales = totalSales - totalItbis;
        var reportDate = DateTime.Now;

        return new DgiiMonthlyReportSummary(
            FiscalPeriod: fiscalPeriod,
            RecordsCount: periodRecords.Count,
            TotalSales: totalSales,
            TotalItbis: totalItbis,
            EstimatedNetSales: estimatedNetSales,
            GeneratedAt: reportDate,
            Records: periodRecords);
    }

    public void Add(DgiiReport report)
    {
        var cleanReport = new DgiiReport
        {
            BusinessName = report.BusinessName.Trim(),
            TaxId = report.TaxId.Trim(),
            FiscalPeriod = report.FiscalPeriod.Trim(),
            GrossSales = report.GrossSales,
            ItbisCollected = report.ItbisCollected,
            Notes = string.IsNullOrWhiteSpace(report.Notes) ? null : report.Notes.Trim()
        };

        lock (_sync)
        {
            _records.Add(new DgiiReportRecord(Guid.NewGuid(), DateTime.UtcNow, cleanReport));
            SaveToDisk();
        }
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_storagePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_storagePath);
            var existingRecords = JsonSerializer.Deserialize<List<DgiiReportRecord>>(json, JsonOptions);

            if (existingRecords is { Count: > 0 })
            {
                _records.AddRange(existingRecords);
            }
        }
        catch
        {
            // If storage is corrupted, app keeps running with in-memory list.
        }
    }

    private void SaveToDisk()
    {
        var json = JsonSerializer.Serialize(_records, JsonOptions);
        File.WriteAllText(_storagePath, json);
    }
}

public record DgiiReportRecord(Guid Id, DateTime CreatedAt, DgiiReport Data);
public record DgiiMonthlyReportSummary(
    string FiscalPeriod,
    int RecordsCount,
    decimal TotalSales,
    decimal TotalItbis,
    decimal EstimatedNetSales,
    DateTime GeneratedAt,
    IReadOnlyList<DgiiReportRecord> Records);
