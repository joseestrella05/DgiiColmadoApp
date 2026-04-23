using DgiiColmadoApp.Models;

namespace DgiiColmadoApp.Services;

public class DgiiReportService
{
    private readonly List<DgiiReportRecord> _records = [];

    public IReadOnlyList<DgiiReportRecord> GetAll() =>
        _records.OrderByDescending(x => x.CreatedAt).ToList();

    public void Add(DgiiReport report)
    {
        var copy = new DgiiReport
        {
            BusinessName = report.BusinessName,
            TaxId = report.TaxId,
            FiscalPeriod = report.FiscalPeriod,
            GrossSales = report.GrossSales,
            ItbisCollected = report.ItbisCollected,
            Notes = report.Notes
        };

        _records.Add(new DgiiReportRecord(Guid.NewGuid(), DateTime.UtcNow, copy));
    }
}

public record DgiiReportRecord(Guid Id, DateTime CreatedAt, DgiiReport Data);
