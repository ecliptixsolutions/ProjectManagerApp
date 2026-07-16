using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Data;
using ProjectManagerApp.Helpers;
using ProjectManagerApp.Models;
using ProjectManagerApp.ViewModels;

namespace ProjectManagerApp.Controllers;

[AdminAuthorize]
public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? status)
    {
        var query = _context.Employees.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e =>
                (e.FullName ?? string.Empty).Contains(search) ||
                (e.EmployeeId ?? string.Empty).Contains(search) ||
                (e.Designation ?? string.Empty).Contains(search) ||
                (e.Department ?? string.Empty).Contains(search) ||
                (e.Email ?? string.Empty).Contains(search) ||
                (e.Phone ?? string.Empty).Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e => e.Status == status);

        var employees = await query.OrderBy(e => e.FullName).ToListAsync();
        ViewBag.StatusFilter = status;
        return View(employees);
    }

    public IActionResult Create()
    {
        return View(new Employee());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        ModelState.Remove("EmployeeId");
        if (!ModelState.IsValid)
            return View(employee);

        var lastEmp = await _context.Employees
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync();
        var nextId = lastEmp != null ? int.Parse(lastEmp.EmployeeId[3..]) + 1 : 1;
        employee.EmployeeId = $"EMP{nextId:D3}";
        employee.CreatedBy = GetCurrentUserId();
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        await LogAudit("Create", "Employee", employee.Id, $"Added employee: {employee.FullName} ({employee.EmployeeId})");

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
            return NotFound();

        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee employee)
    {
        if (id != employee.Id)
            return BadRequest();

        ModelState.Remove("EmployeeId");
        if (!ModelState.IsValid)
            return View(employee);

        var existing = await _context.Employees.FindAsync(id);
        if (existing is null)
            return NotFound();

        var oldData = System.Text.Json.JsonSerializer.Serialize(existing);
        var changes = new List<string>();

        if (existing.FullName != employee.FullName) { existing.FullName = employee.FullName; changes.Add("Name"); }
        if (existing.Email != employee.Email) { existing.Email = employee.Email; changes.Add("Email"); }
        if (existing.Phone != employee.Phone) { existing.Phone = employee.Phone; changes.Add("Phone"); }
        if (existing.Address != employee.Address) { existing.Address = employee.Address; changes.Add("Address"); }
        if (existing.Department != employee.Department) { existing.Department = employee.Department; changes.Add("Department"); }
        if (existing.Designation != employee.Designation) { existing.Designation = employee.Designation; changes.Add("Designation"); }
        if (existing.JoinedDate != employee.JoinedDate) { existing.JoinedDate = employee.JoinedDate; changes.Add("JoinedDate"); }
        if (existing.EmploymentType != employee.EmploymentType) { existing.EmploymentType = employee.EmploymentType; changes.Add("EmploymentType"); }
        if (existing.Status != employee.Status) { existing.Status = employee.Status; changes.Add("Status"); }
        if (existing.SalaryAmount != employee.SalaryAmount) { existing.SalaryAmount = employee.SalaryAmount; changes.Add("Salary"); }
        if (existing.SalaryType != employee.SalaryType) { existing.SalaryType = employee.SalaryType; changes.Add("SalaryType"); }
        if (existing.BankName != employee.BankName) { existing.BankName = employee.BankName; changes.Add("BankName"); }
        if (existing.AccountNumber != employee.AccountNumber) { existing.AccountNumber = employee.AccountNumber; changes.Add("AccountNumber"); }
        if (existing.IfscCode != employee.IfscCode) { existing.IfscCode = employee.IfscCode; changes.Add("IFSC"); }
        if (existing.UpiId != employee.UpiId) { existing.UpiId = employee.UpiId; changes.Add("UPI"); }
        if (existing.Notes != employee.Notes) { existing.Notes = employee.Notes; changes.Add("Notes"); }

        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogAudit("Update", "Employee", existing.Id,
            $"Updated fields: {string.Join(", ", changes)}");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
            return NotFound();

        employee.IsDeleted = true;
        employee.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await LogAudit("Delete", "Employee", employee.Id, $"Deactivated employee: {employee.FullName}");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.SalaryPayments)
            .Include(e => e.Assignments).ThenInclude(a => a.Project)
            .Include(e => e.WorkUpdates).ThenInclude(w => w.Project)
            .SingleOrDefaultAsync(e => e.Id == id);

        if (employee is null)
            return NotFound();

        var viewModel = new EmployeeDetailsViewModel
        {
            Employee = employee,
            Projects = await _context.Projects.ToListAsync(),
            Assignments = employee.Assignments.Where(a => !a.IsDeleted),
            WorkUpdates = employee.WorkUpdates.Where(w => !w.IsDeleted).OrderByDescending(w => w.WorkDate)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSalaryPayment(SalaryPaymentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var employee = await _context.Employees
                .Include(e => e.SalaryPayments)
                .SingleOrDefaultAsync(e => e.Id == model.EmployeeId);
            var vm = new EmployeeDetailsViewModel
            {
                Employee = employee ?? new Employee(),
                Projects = await _context.Projects.ToListAsync()
            };
            return View("Details", vm);
        }

        var payment = new SalaryPayment
        {
            EmployeeId = model.EmployeeId,
            ProjectId = model.ProjectId,
            Month = model.Month,
            BasicSalary = model.BasicSalary,
            Bonus = model.Bonus,
            Deduction = model.Deduction,
            NetSalary = model.NetSalary,
            PaymentDate = model.PaymentDate,
            PaymentStatus = model.PaymentStatus,
            Notes = model.Notes,
            PaidBy = GetCurrentUserId(),
            CreatedAt = DateTime.UtcNow
        };

        _context.SalaryPayments.Add(payment);
        await _context.SaveChangesAsync();
        await LogAudit("Create", "SalaryPayment", payment.Id,
            $"Salary {model.Month}: {model.NetSalary:C} for employee #{model.EmployeeId}");
        return RedirectToAction(nameof(Details), new { id = model.EmployeeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSalaryPayment(int id)
    {
        var payment = await _context.SalaryPayments.FindAsync(id);
        if (payment is null)
            return NotFound();

        payment.IsDeleted = true;
        await _context.SaveChangesAsync();
        await LogAudit("Delete", "SalaryPayment", payment.Id, $"Deleted salary payment for {payment.Month}");
        return RedirectToAction(nameof(Details), new { id = payment.EmployeeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLogin(int id, string username, string password)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
            return NotFound();

        if (employee.AppUserId.HasValue)
        {
            TempData["Error"] = "Employee already has a login account.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (await _context.Users.AnyAsync(u => u.UserName == username))
        {
            TempData["Error"] = "Username already taken.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var user = new AppUser
        {
            UserName = username,
            Role = "Employee"
        };
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        employee.AppUserId = user.Id;
        await _context.SaveChangesAsync();

        await LogAudit("Create", "EmployeeLogin", employee.Id, $"Created login for {employee.FullName}");
        TempData["Success"] = "Login account created successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string newPassword)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null || !employee.AppUserId.HasValue)
        {
            TempData["Error"] = "Employee has no login account.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var user = await _context.Users.FindAsync(employee.AppUserId.Value);
        if (user is null)
        {
            TempData["Error"] = "User account not found.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, newPassword);
        await _context.SaveChangesAsync();

        await LogAudit("Update", "EmployeePassword", employee.Id, $"Password reset for {employee.FullName}");
        TempData["Success"] = "Password reset successfully.";
        return RedirectToAction(nameof(Details), new { id });
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
            ModuleName = "Employees",
            Metadata = metadata,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
