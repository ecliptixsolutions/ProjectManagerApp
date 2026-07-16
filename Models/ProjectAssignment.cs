using System.ComponentModel.DataAnnotations;

namespace ProjectManagerApp.Models;

public class ProjectAssignment
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required]
    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    [MaxLength(100)]
    public string? Role { get; set; }

    [DataType(DataType.Date)]
    public DateTime AssignmentDate { get; set; } = DateTime.UtcNow.Date;

    [Required]
    public int AssignedBy { get; set; }
    public AppUser? AssignedByUser { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}
