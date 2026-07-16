using ProjectManagerApp.Models;

namespace ProjectManagerApp.ViewModels;

public class EmployeeDetailsViewModel
{
    public Employee Employee { get; set; } = null!;
    public IEnumerable<Project> Projects { get; set; } = Enumerable.Empty<Project>();
    public IEnumerable<ProjectAssignment> Assignments { get; set; } = Enumerable.Empty<ProjectAssignment>();
    public IEnumerable<DailyWorkUpdate> WorkUpdates { get; set; } = Enumerable.Empty<DailyWorkUpdate>();
    public decimal TotalPaid => Employee.SalaryPayments.Sum(s => s.NetSalary);
}

public class EmployeeProfileViewModel
{
    public Employee Employee { get; set; } = null!;
    public IEnumerable<ProjectAssignment> Assignments { get; set; } = Enumerable.Empty<ProjectAssignment>();
    public IEnumerable<DailyWorkUpdate> WorkUpdates { get; set; } = Enumerable.Empty<DailyWorkUpdate>();
}
