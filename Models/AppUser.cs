using System.ComponentModel.DataAnnotations;

namespace ProjectManagerApp.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Admin";

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
