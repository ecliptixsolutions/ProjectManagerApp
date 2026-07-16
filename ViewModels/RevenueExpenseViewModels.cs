using System.ComponentModel.DataAnnotations;
using ProjectManagerApp.Models;

namespace ProjectManagerApp.ViewModels;

public class RevenueCreateViewModel
{
    public int? Id { get; set; }

    public int? ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string RevenueSource { get; set; } = null!;

    [MaxLength(500)]
    public string? Reason { get; set; }

    [Required]
    [Range(0.01, 9999999999)]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    public DateTime RevenueDate { get; set; } = DateTime.UtcNow.Date;

    [MaxLength(100)]
    public string? ReceivedBy { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class ExpenseCreateViewModel
{
    public int? Id { get; set; }

    public int? ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ExpenseCategory { get; set; } = null!;

    [MaxLength(500)]
    public string? ExpenseReason { get; set; }

    [Required]
    [Range(0.01, 9999999999)]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow.Date;

    [MaxLength(100)]
    public string? PaidBy { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class SalaryPaymentCreateViewModel
{
    public int? Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public int? ProjectId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Month { get; set; } = null!;

    [Range(0, 9999999999)]
    public decimal BasicSalary { get; set; }

    [Range(0, 9999999999)]
    public decimal Bonus { get; set; }

    [Range(0, 9999999999)]
    public decimal Deduction { get; set; }

    [Range(0, 9999999999)]
    public decimal NetSalary { get; set; }

    [DataType(DataType.Date)]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow.Date;

    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending";

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class DashboardViewModel
{
    // Project Stats
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int PendingProjects { get; set; }

    // Employee Stats
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }

    // Financial Stats
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalSalary { get; set; }
    public decimal NetProfit { get; set; }

    // Recent Data
    public List<Project> RecentProjects { get; set; } = new();
    public List<DailyWorkUpdate> RecentWorkUpdates { get; set; } = new();
    public List<AuditLog> RecentAuditLogs { get; set; } = new();
}

public class AssignProjectViewModel
{
    public int ProjectId { get; set; }

    [Required]
    public List<int> EmployeeIds { get; set; } = new();

    [MaxLength(100)]
    public string? Role { get; set; }

    [DataType(DataType.Date)]
    public DateTime AssignmentDate { get; set; } = DateTime.UtcNow.Date;

    public Project? Project { get; set; }
    public List<Employee> AvailableEmployees { get; set; } = new();
    public List<ProjectAssignment> CurrentAssignments { get; set; } = new();
}
