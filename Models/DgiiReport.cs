using System.ComponentModel.DataAnnotations;

namespace DgiiColmadoApp.Models;

public class DgiiReport
{
    [Required(ErrorMessage = "El nombre del colmado es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre no puede exceder 120 caracteres.")]
    public string BusinessName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El RNC o cédula es obligatorio.")]
    [StringLength(20, ErrorMessage = "El RNC/Cédula no puede exceder 20 caracteres.")]
    public string TaxId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El período fiscal es obligatorio.")]
    [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "Use formato YYYY-MM.")]
    public string FiscalPeriod { get; set; } = string.Empty;

    [Range(0.01, 999999999, ErrorMessage = "Ingrese un monto de ventas válido.")]
    public decimal GrossSales { get; set; }

    [Range(0, 999999999, ErrorMessage = "El ITBIS no puede ser negativo.")]
    public decimal ItbisCollected { get; set; }

    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres.")]
    public string? Notes { get; set; }
}
