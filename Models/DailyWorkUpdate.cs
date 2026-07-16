using System.ComponentModel.DataAnnotations;

namespace ProjectManagerApp.Models;

public class DailyWorkUpdate
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required]
    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    [DataType(DataType.Date)]
    public DateTime WorkDate { get; set; } = DateTime.UtcNow.Date;

    [Required]
    [MaxLength(2000)]
    public string WorkDescription { get; set; } = null!;

    [Range(0.1, 24)]
    public double HoursWorked { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [MaxLength(500)]
    public string? AttachmentPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}
