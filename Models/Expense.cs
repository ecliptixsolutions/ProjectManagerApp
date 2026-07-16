using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagerApp.Models;

public class Expense
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string ExpenseCategory { get; set; } = null!;

    [MaxLength(500)]
    public string? ExpenseReason { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    [DataType(DataType.Date)]
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow.Date;

    [Range(0.01, 9999999999)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(100)]
    public string? PaidBy { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public int AddedBy { get; set; }
    public AppUser? AddedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}
