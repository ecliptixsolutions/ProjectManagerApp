using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagerApp.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string ClientName { get; set; } = null!;

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ExpectedEndDate { get; set; }

    [Range(0, 9999999999)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ProjectCost { get; set; }

    [Range(0, 9999999999)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedExpense { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "New";

    [Required]
    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending";

    public bool IsDeleted { get; set; }

    [Required]
    public int CreatedBy { get; set; }
    public AppUser? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Revenue> Revenues { get; set; } = new List<Revenue>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<SalaryPayment> SalaryPayments { get; set; } = new List<SalaryPayment>();
    public ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();
    public ICollection<DailyWorkUpdate> WorkUpdates { get; set; } = new List<DailyWorkUpdate>();
}
