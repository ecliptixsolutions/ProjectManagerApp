using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Data;
using ProjectManagerApp.Helpers;
using ProjectManagerApp.Models;
using ProjectManagerApp.ViewModels;
using System.Security.Claims;

namespace ProjectManagerApp.Controllers;

[AdminAuthorize]
public class TransactionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public TransactionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? type, string? search, DateTime? from, DateTime? to)
    {
        ViewBag.TypeFilter = type;

        if (type == "Expense")
        {
            var expenses = await _context.Expenses
                .Include(e => e.Project)
                .Where(e => !from.HasValue || e.ExpenseDate >= from.Value)
                .Where(e => !to.HasValue || e.ExpenseDate <= to.Value)
                .Where(e => string.IsNullOrWhiteSpace(search) ||
                    (e.ExpenseCategory ?? string.Empty).Contains(search) ||
                    (e.ExpenseReason ?? string.Empty).Contains(search))
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
            return View(expenses);
        }

        var revenues = await _context.Revenues
            .Include(r => r.Project)
            .Where(r => !from.HasValue || r.RevenueDate >= from.Value)
            .Where(r => !to.HasValue || r.RevenueDate <= to.Value)
            .Where(r => string.IsNullOrWhiteSpace(search) ||
                (r.RevenueSource ?? string.Empty).Contains(search) ||
                (r.Reason ?? string.Empty).Contains(search))
            .OrderByDescending(r => r.RevenueDate)
            .ToListAsync();
        return View(revenues);
    }

    public async Task<IActionResult> ProjectTransactions(int projectId)
    {
        var project = await _context.Projects
            .Include(p => p.Revenues)
            .Include(p => p.Expenses)
            .Include(p => p.SalaryPayments)
            .SingleOrDefaultAsync(p => p.Id == projectId);

        if (project is null)
            return NotFound();

        ViewData["ProjectName"] = project.Name;
        ViewData["ProjectId"] = project.Id;
        return View(project);
    }

    public IActionResult AddRevenue(int? projectId)
    {
        return View(new RevenueCreateViewModel { ProjectId = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRevenue(RevenueCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var revenue = new Revenue
        {
            ProjectId = model.ProjectId,
            RevenueSource = model.RevenueSource,
            Reason = model.Reason,
            Amount = model.Amount,
            RevenueDate = model.RevenueDate,
            ReceivedBy = model.ReceivedBy,
            PaymentMethod = model.PaymentMethod,
            Notes = model.Notes,
            AddedBy = GetCurrentUserId(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Revenues.Add(revenue);
        await _context.SaveChangesAsync();

        var redirectTarget = model.ProjectId.HasValue
            ? RedirectToAction(nameof(ProjectTransactions), new { projectId = model.ProjectId.Value })
            : RedirectToAction(nameof(Index));
        await LogAudit("Create", "Revenue", revenue.Id, $"Revenue: {revenue.RevenueSource} - {revenue.Amount:C}");
        return redirectTarget;
    }

    public IActionResult AddExpense(int? projectId)
    {
        return View(new ExpenseCreateViewModel { ProjectId = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExpense(ExpenseCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var expense = new Expense
        {
            ProjectId = model.ProjectId,
            ExpenseCategory = model.ExpenseCategory,
            ExpenseReason = model.ExpenseReason,
            Amount = model.Amount,
            ExpenseDate = model.ExpenseDate,
            PaidBy = model.PaidBy,
            PaymentMethod = model.PaymentMethod,
            Notes = model.Notes,
            AddedBy = GetCurrentUserId(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var redirectTarget = model.ProjectId.HasValue
            ? RedirectToAction(nameof(ProjectTransactions), new { projectId = model.ProjectId.Value })
            : RedirectToAction(nameof(Index), new { type = "Expense" });
        await LogAudit("Create", "Expense", expense.Id, $"Expense: {expense.ExpenseCategory} - {expense.Amount:C}");
        return redirectTarget;
    }

    public async Task<IActionResult> EditRevenue(int id)
    {
        var revenue = await _context.Revenues.FindAsync(id);
        if (revenue is null)
            return NotFound();

        return View(new RevenueCreateViewModel
        {
            Id = revenue.Id,
            ProjectId = revenue.ProjectId,
            RevenueSource = revenue.RevenueSource,
            Reason = revenue.Reason,
            Amount = revenue.Amount,
            RevenueDate = revenue.RevenueDate,
            ReceivedBy = revenue.ReceivedBy,
            PaymentMethod = revenue.PaymentMethod,
            Notes = revenue.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRevenue(RevenueCreateViewModel model)
    {
        if (!model.Id.HasValue)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var revenue = await _context.Revenues.FindAsync(model.Id.Value);
        if (revenue is null)
            return NotFound();

        revenue.RevenueSource = model.RevenueSource;
        revenue.Reason = model.Reason;
        revenue.Amount = model.Amount;
        revenue.RevenueDate = model.RevenueDate;
        revenue.ReceivedBy = model.ReceivedBy;
        revenue.PaymentMethod = model.PaymentMethod;
        revenue.Notes = model.Notes;
        revenue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await LogAudit("Update", "Revenue", revenue.Id, $"Updated revenue #{revenue.Id}");

        var redirectTarget = revenue.ProjectId.HasValue
            ? RedirectToAction(nameof(ProjectTransactions), new { projectId = revenue.ProjectId.Value })
            : RedirectToAction(nameof(Index));
        return redirectTarget;
    }

    public async Task<IActionResult> EditExpense(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense is null)
            return NotFound();

        return View(new ExpenseCreateViewModel
        {
            Id = expense.Id,
            ProjectId = expense.ProjectId,
            ExpenseCategory = expense.ExpenseCategory,
            ExpenseReason = expense.ExpenseReason,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            PaidBy = expense.PaidBy,
            PaymentMethod = expense.PaymentMethod,
            Notes = expense.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditExpense(ExpenseCreateViewModel model)
    {
        if (!model.Id.HasValue)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var expense = await _context.Expenses.FindAsync(model.Id.Value);
        if (expense is null)
            return NotFound();

        expense.ExpenseCategory = model.ExpenseCategory;
        expense.ExpenseReason = model.ExpenseReason;
        expense.Amount = model.Amount;
        expense.ExpenseDate = model.ExpenseDate;
        expense.PaidBy = model.PaidBy;
        expense.PaymentMethod = model.PaymentMethod;
        expense.Notes = model.Notes;
        expense.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await LogAudit("Update", "Expense", expense.Id, $"Updated expense #{expense.Id}");

        var redirectTarget = expense.ProjectId.HasValue
            ? RedirectToAction(nameof(ProjectTransactions), new { projectId = expense.ProjectId.Value })
            : RedirectToAction(nameof(Index), new { type = "Expense" });
        return redirectTarget;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRevenue(int id)
    {
        var revenue = await _context.Revenues.FindAsync(id);
        if (revenue is null)
            return NotFound();

        revenue.IsDeleted = true;
        revenue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await LogAudit("Delete", "Revenue", revenue.Id, $"Deleted revenue #{revenue.Id}");

        var redirectTarget = revenue.ProjectId.HasValue
            ? RedirectToAction(nameof(ProjectTransactions), new { projectId = revenue.ProjectId.Value })
            : RedirectToAction(nameof(Index));
        return redirectTarget;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense is null)
            return NotFound();

        expense.IsDeleted = true;
        expense.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await LogAudit("Delete", "Expense", expense.Id, $"Deleted expense #{expense.Id}");

        var redirectTarget = expense.ProjectId.HasValue
            ? RedirectToAction(nameof(ProjectTransactions), new { projectId = expense.ProjectId.Value })
            : RedirectToAction(nameof(Index), new { type = "Expense" });
        return redirectTarget;
    }

    private int GetCurrentUserId()
    {
        var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idValue, out var id) ? id : 0;
    }

    private async Task LogAudit(string action, string entityType, int entityId, string? metadata)
    {
        var user = await _context.Users.FindAsync(GetCurrentUserId());
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = GetCurrentUserId(),
            UserName = user?.UserName,
            UserRole = user?.Role,
            ActionType = action,
            EntityType = entityType,
            EntityId = entityId,
            ModuleName = "Transactions",
            Metadata = metadata,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
