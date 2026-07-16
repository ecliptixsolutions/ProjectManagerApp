using ProjectManagerApp.Models;

namespace ProjectManagerApp.ViewModels;

public class OverviewReportViewModel
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal TotalSalary { get; set; }
    public decimal NetAmount { get; set; }
    public List<Project> Projects { get; set; } = new();
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
}

public class RevenueReportViewModel
{
    public decimal DailyRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public List<Revenue> Revenues { get; set; } = new();
}

public class ExpenseReportViewModel
{
    public decimal DailyExpense { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal YearlyExpense { get; set; }
    public List<Expense> Expenses { get; set; } = new();
}
