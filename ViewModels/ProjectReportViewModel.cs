using ProjectManagerApp.Models;

namespace ProjectManagerApp.ViewModels;

public class ProjectReportViewModel
{
    public Project Project { get; set; } = null!;
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal TotalSalary { get; set; }
    public decimal NetAmount { get; set; }
}
