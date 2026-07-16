using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagerApp.Models;

public class Employee
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string EmployeeId { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = null!;

    [Phone]
    [MaxLength(30)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Designation { get; set; }

    [DataType(DataType.Date)]
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow.Date;

    [MaxLength(50)]
    public string? EmploymentType { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    [Range(0, 9999999999)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalaryAmount { get; set; }

    [Required]
    [MaxLength(20)]
    public string SalaryType { get; set; } = "Monthly";

    [MaxLength(200)]
    public string? BankName { get; set; }

    [MaxLength(50)]
    public string? AccountNumber { get; set; }

    [MaxLength(20)]
    public string? IfscCode { get; set; }

    [MaxLength(100)]
    public string? UpiId { get; set; }

    public int? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public int CreatedBy { get; set; }
    public AppUser? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public ICollection<SalaryPayment> SalaryPayments { get; set; } = new List<SalaryPayment>();
    public ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();
    public ICollection<DailyWorkUpdate> WorkUpdates { get; set; } = new List<DailyWorkUpdate>();
}
