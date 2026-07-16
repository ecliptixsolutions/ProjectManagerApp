using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Data;
using ProjectManagerApp.Helpers;
using ProjectManagerApp.Models;
using ProjectManagerApp.ViewModels;

namespace ProjectManagerApp.Controllers;

[AdminAuthorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Revenue(DateTime? from, DateTime? to, string? source)
    {
        var query = _context.Revenues.Include(r => r.Project).AsQueryable();
        if (from.HasValue) query = query.Where(r => r.RevenueDate >= from.Value);
        if (to.HasValue) query = query.Where(r => r.RevenueDate <= to.Value);
        if (!string.IsNullOrWhiteSpace(source))
            query = query.Where(r => r.RevenueSource == source);

        var revenues = await query.OrderByDescending(r => r.RevenueDate).ToListAsync();
        var now = DateTime.UtcNow;

        var vm = new RevenueReportViewModel
        {
            DailyRevenue = revenues.Where(r => r.RevenueDate == now.Date).Sum(r => r.Amount),
            MonthlyRevenue = revenues.Where(r => r.RevenueDate.Year == now.Year && r.RevenueDate.Month == now.Month).Sum(r => r.Amount),
            YearlyRevenue = revenues.Where(r => r.RevenueDate.Year == now.Year).Sum(r => r.Amount),
            Revenues = revenues
        };

        ViewBag.Sources = await _context.Revenues.Select(r => r.RevenueSource).Distinct().ToListAsync();
        return View(vm);
    }

    public async Task<IActionResult> Expense(DateTime? from, DateTime? to, string? category)
    {
        var query = _context.Expenses.Include(e => e.Project).AsQueryable();
        if (from.HasValue) query = query.Where(e => e.ExpenseDate >= from.Value);
        if (to.HasValue) query = query.Where(e => e.ExpenseDate <= to.Value);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(e => e.ExpenseCategory == category);

        var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
        var now = DateTime.UtcNow;

        var vm = new ExpenseReportViewModel
        {
            DailyExpense = expenses.Where(e => e.ExpenseDate == now.Date).Sum(e => e.Amount),
            MonthlyExpense = expenses.Where(e => e.ExpenseDate.Year == now.Year && e.ExpenseDate.Month == now.Month).Sum(e => e.Amount),
            YearlyExpense = expenses.Where(e => e.ExpenseDate.Year == now.Year).Sum(e => e.Amount),
            Expenses = expenses
        };

        ViewBag.Categories = await _context.Expenses.Select(e => e.ExpenseCategory).Distinct().ToListAsync();
        return View(vm);
    }

    public async Task<IActionResult> ProfitLoss(DateTime? from, DateTime? to)
    {
        if (!from.HasValue) from = new DateTime(DateTime.UtcNow.Year, 1, 1);
        if (!to.HasValue) to = DateTime.UtcNow;

        var projects = await _context.Projects
            .Include(p => p.Revenues.Where(r => r.RevenueDate >= from.Value && r.RevenueDate <= to.Value))
            .Include(p => p.Expenses.Where(e => e.ExpenseDate >= from.Value && e.ExpenseDate <= to.Value))
            .Include(p => p.SalaryPayments.Where(s => s.PaymentDate >= from.Value && s.PaymentDate <= to.Value))
            .ToListAsync();

        var vm = new OverviewReportViewModel
        {
            TotalRevenue = projects.Sum(p => p.Revenues.Sum(r => r.Amount)),
            TotalExpense = projects.Sum(p => p.Expenses.Sum(e => e.Amount)),
            TotalSalary = projects.Sum(p => p.SalaryPayments.Sum(s => s.NetSalary)),
            NetAmount = projects.Sum(p => p.Revenues.Sum(r => r.Amount) - p.Expenses.Sum(e => e.Amount) - p.SalaryPayments.Sum(s => s.NetSalary)),
            Projects = projects
        };

        ViewBag.FromDate = from.Value.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.Value.ToString("yyyy-MM-dd");
        return View(vm);
    }

    public async Task<IActionResult> Salaries(string? month, string? status)
    {
        var query = _context.SalaryPayments
            .Include(s => s.Employee)
            .Include(s => s.Project)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(month))
            query = query.Where(s => s.Month == month);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.PaymentStatus == status);

        ViewBag.Months = await _context.SalaryPayments.Select(s => s.Month).Distinct().OrderDescending().ToListAsync();
        var payments = await query.OrderByDescending(s => s.PaymentDate).ToListAsync();
        return View(payments);
    }

    public async Task<IActionResult> EmployeeProductivity(int? employeeId, DateTime? from, DateTime? to)
    {
        var query = _context.DailyWorkUpdates
            .Include(w => w.Employee)
            .Include(w => w.Project)
            .AsQueryable();

        if (employeeId.HasValue) query = query.Where(w => w.EmployeeId == employeeId.Value);
        if (from.HasValue) query = query.Where(w => w.WorkDate >= from.Value);
        if (to.HasValue) query = query.Where(w => w.WorkDate <= to.Value);

        var updates = await query.OrderByDescending(w => w.WorkDate).ToListAsync();
        ViewBag.Employees = await _context.Employees.Where(e => e.Status == "Active").OrderBy(e => e.FullName).ToListAsync();
        return View(updates);
    }
}
