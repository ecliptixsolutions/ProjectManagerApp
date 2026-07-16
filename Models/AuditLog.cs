using System.ComponentModel.DataAnnotations;

namespace ProjectManagerApp.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public AppUser? User { get; set; }

    [MaxLength(100)]
    public string? UserName { get; set; }

    [MaxLength(50)]
    public string? UserRole { get; set; }

    [Required]
    [MaxLength(100)]
    public string ActionType { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = null!;

    [MaxLength(100)]
    public string? ModuleName { get; set; }

    public int? EntityId { get; set; }

    [MaxLength(2000)]
    public string? OldValue { get; set; }

    [MaxLength(2000)]
    public string? NewValue { get; set; }

    [MaxLength(2000)]
    public string? Metadata { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; }
}
