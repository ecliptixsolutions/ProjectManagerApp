using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagerApp.Models;

public class SalaryPayment
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    [Required]
    [MaxLength(20)]
    public string Month { get; set; } = null!;

    [Range(0, 9999999999)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }

    [Range(0, 9999999999)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Bonus { get; set; }

    [Range(0, 9999999999)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Deduction { get; set; }

    [Range(0, 9999999999)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetSalary { get; set; }

    [DataType(DataType.Date)]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow.Date;

    [Required]
    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending";

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public int PaidBy { get; set; }
    public AppUser? PaidByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}
