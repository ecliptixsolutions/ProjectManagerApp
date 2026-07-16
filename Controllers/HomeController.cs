using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Data;
using ProjectManagerApp.Models;
using ProjectManagerApp.ViewModels;

namespace ProjectManagerApp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated != true)
            return View();

        if (User.IsInRole("Admin"))
            return await AdminDashboard();

        return await EmployeeDashboard();
    }

    private async Task<IActionResult> AdminDashboard()
    {
        var projects = await _context.Projects
            .Include(p => p.Revenues)
            .Include(p => p.Expenses)
            .Include(p => p.SalaryPayments)
            .ToListAsync();

        var employees = await _context.Employees.ToListAsync();
        var now = DateTime.UtcNow;

        var vm = new DashboardViewModel
        {
            TotalProjects = projects.Count,
            ActiveProjects = projects.Count(p => p.Status == "InProgress"),
            CompletedProjects = projects.Count(p => p.Status == "Completed"),
            PendingProjects = projects.Count(p => p.Status == "New"),
            TotalEmployees = employees.Count,
            ActiveEmployees = employees.Count(e => e.Status == "Active"),
            TotalRevenue = projects.Sum(p => p.Revenues.Sum(r => r.Amount)),
            TotalExpenses = projects.Sum(p => p.Expenses.Sum(e => e.Amount)),
            TotalSalary = projects.Sum(p => p.SalaryPayments.Sum(s => s.NetSalary)),
            RecentProjects = projects.OrderByDescending(p => p.CreatedAt).Take(5).ToList(),
            RecentWorkUpdates = await _context.DailyWorkUpdates
                .Include(w => w.Employee)
                .Include(w => w.Project)
                .OrderByDescending(w => w.WorkDate)
                .Take(5)
                .ToListAsync(),
            RecentAuditLogs = await _context.AuditLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .Take(5)
                .ToListAsync()
        };
        vm.NetProfit = vm.TotalRevenue - vm.TotalExpenses - vm.TotalSalary;

        return View(vm);
    }

    private async Task<IActionResult> EmployeeDashboard()
    {
        var userId = GetCurrentUserId();
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.AppUserId == userId);
        if (employee == null)
            return View();

        var assignments = await _context.ProjectAssignments
            .Include(a => a.Project)
            .Where(a => a.EmployeeId == employee.Id && a.Status == "Active")
            .ToListAsync();

        var updates = await _context.DailyWorkUpdates
            .Include(w => w.Project)
            .Where(w => w.EmployeeId == employee.Id)
            .OrderByDescending(w => w.WorkDate)
            .Take(10)
            .ToListAsync();

        ViewBag.Employee = employee;
        ViewBag.Assignments = assignments;
        ViewBag.Updates = updates;

        return View("EmployeeDashboard");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private int GetCurrentUserId()
    {
        var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idValue, out var id) ? id : 0;
    }
}
