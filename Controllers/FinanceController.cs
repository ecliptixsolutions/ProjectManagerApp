using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Data;
using ProjectManagerApp.Helpers;
using ProjectManagerApp.Models;
using ProjectManagerApp.ViewModels;
using System.Security.Claims;

namespace ProjectManagerApp.Controllers;

[AdminAuthorize]
public class FinanceController : Controller
{
    private readonly ApplicationDbContext _context;

    public FinanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Projects(string? search, string? status, string? paymentStatus)
    {
        var query = _context.Projects
            .Include(p => p.Revenues)
            .Include(p => p.Expenses)
            .Include(p => p.SalaryPayments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                (p.Name ?? string.Empty).Contains(search) ||
                (p.ClientName ?? string.Empty).Contains(search) ||
                (p.Category ?? string.Empty).Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(paymentStatus))
            query = query.Where(p => p.PaymentStatus == paymentStatus);

        var projects = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        ViewBag.StatusFilter = status;
        ViewBag.PaymentFilter = paymentStatus;
        return View(projects);
    }

    public IActionResult Create()
    {
        return View(new Project());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        if (!ModelState.IsValid)
            return View(project);

        project.CreatedBy = GetCurrentUserId();
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        await LogAudit("Create", "Project", project.Id, $"Created project: {project.Name}");
        return RedirectToAction(nameof(Projects));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project is null)
            return NotFound();

        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(project);

        var existing = await _context.Projects.FindAsync(id);
        if (existing is null)
            return NotFound();

        var changes = new List<string>();
        if (existing.Name != project.Name) { existing.Name = project.Name; changes.Add("Name"); }
        if (existing.ClientName != project.ClientName) { existing.ClientName = project.ClientName; changes.Add("Client"); }
        if (existing.Category != project.Category) { existing.Category = project.Category; changes.Add("Category"); }
        if (existing.Description != project.Description) { existing.Description = project.Description; changes.Add("Description"); }
        if (existing.StartDate != project.StartDate) { existing.StartDate = project.StartDate; changes.Add("StartDate"); }
        if (existing.ExpectedEndDate != project.ExpectedEndDate) { existing.ExpectedEndDate = project.ExpectedEndDate; changes.Add("EndDate"); }
        if (existing.ProjectCost != project.ProjectCost) { existing.ProjectCost = project.ProjectCost; changes.Add("ProjectCost"); }
        if (existing.EstimatedExpense != project.EstimatedExpense) { existing.EstimatedExpense = project.EstimatedExpense; changes.Add("EstimatedExpense"); }
        if (existing.Notes != project.Notes) { existing.Notes = project.Notes; changes.Add("Notes"); }
        if (existing.Status != project.Status) { existing.Status = project.Status; changes.Add("Status"); }
        if (existing.PaymentStatus != project.PaymentStatus) { existing.PaymentStatus = project.PaymentStatus; changes.Add("PaymentStatus"); }

        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await LogAudit("Update", "Project", existing.Id, $"Updated fields: {string.Join(", ", changes)}");
        return RedirectToAction(nameof(Projects));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project is null)
            return NotFound();

        project.IsDeleted = true;
        project.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await LogAudit("Delete", "Project", project.Id, $"Deleted project: {project.Name}");
        return RedirectToAction(nameof(Projects));
    }

    public async Task<IActionResult> ProjectReport(int id)
    {
        var project = await _context.Projects
            .Include(p => p.Revenues)
            .Include(p => p.Expenses)
            .Include(p => p.SalaryPayments)
            .Include(p => p.Assignments).ThenInclude(a => a.Employee)
            .Include(p => p.WorkUpdates).ThenInclude(w => w.Employee)
            .SingleOrDefaultAsync(p => p.Id == id);

        if (project is null)
            return NotFound();

        var report = new ProjectReportViewModel
        {
            Project = project,
            TotalRevenue = project.Revenues.Sum(r => r.Amount),
            TotalExpense = project.Expenses.Sum(e => e.Amount),
            TotalSalary = project.SalaryPayments.Sum(s => s.NetSalary),
            NetAmount = project.Revenues.Sum(r => r.Amount) - project.Expenses.Sum(e => e.Amount) - project.SalaryPayments.Sum(s => s.NetSalary)
        };

        return View(report);
    }

    public async Task<IActionResult> Overview()
    {
        var projects = await _context.Projects
            .Include(p => p.Revenues)
            .Include(p => p.Expenses)
            .Include(p => p.SalaryPayments)
            .ToListAsync();

        var overview = new OverviewReportViewModel
        {
            TotalRevenue = projects.Sum(p => p.Revenues.Sum(r => r.Amount)),
            TotalExpense = projects.Sum(p => p.Expenses.Sum(e => e.Amount)),
            TotalSalary = projects.Sum(p => p.SalaryPayments.Sum(s => s.NetSalary)),
            NetAmount = projects.Sum(p => p.Revenues.Sum(r => r.Amount) - p.Expenses.Sum(e => e.Amount) - p.SalaryPayments.Sum(s => s.NetSalary)),
            Projects = projects,
            TotalProjects = projects.Count,
            ActiveProjects = projects.Count(p => p.Status == "InProgress"),
            CompletedProjects = projects.Count(p => p.Status == "Completed")
        };

        return View(overview);
    }

    public async Task<IActionResult> Assignments(int? projectId)
    {
        var query = _context.ProjectAssignments
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .Include(a => a.AssignedByUser)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(a => a.ProjectId == projectId.Value);

        var assignments = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        ViewBag.Projects = await _context.Projects.ToListAsync();
        return View(assignments);
    }

    public async Task<IActionResult> AssignProject(int projectId)
    {
        var project = await _context.Projects
            .Include(p => p.Assignments).ThenInclude(a => a.Employee)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project is null)
            return NotFound();

        var assignedIds = project.Assignments
            .Where(a => !a.IsDeleted && a.Status == "Active")
            .Select(a => a.EmployeeId)
            .ToHashSet();

        var vm = new AssignProjectViewModel
        {
            ProjectId = projectId,
            Project = project,
            AvailableEmployees = await _context.Employees
                .Where(e => e.Status == "Active" && !assignedIds.Contains(e.Id))
                .OrderBy(e => e.FullName)
                .ToListAsync(),
            CurrentAssignments = project.Assignments
                .Where(a => !a.IsDeleted && a.Status == "Active")
                .ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignProject(AssignProjectViewModel model)
    {
        if (model.EmployeeIds.Any())
        {
            foreach (var empId in model.EmployeeIds)
            {
                var assignment = new ProjectAssignment
                {
                    EmployeeId = empId,
                    ProjectId = model.ProjectId,
                    Role = model.Role,
                    AssignmentDate = model.AssignmentDate,
                    AssignedBy = GetCurrentUserId(),
                    CreatedAt = DateTime.UtcNow
                };
                _context.ProjectAssignments.Add(assignment);
            }
            await _context.SaveChangesAsync();
            await LogAudit("Assign", "ProjectAssignment", model.ProjectId,
                $"Assigned {model.EmployeeIds.Count} employee(s) to project #{model.ProjectId}");
        }

        return RedirectToAction(nameof(AssignProject), new { projectId = model.ProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAssignment(int id)
    {
        var assignment = await _context.ProjectAssignments.FindAsync(id);
        if (assignment is null)
            return NotFound();

        assignment.IsDeleted = true;
        await _context.SaveChangesAsync();
        await LogAudit("Remove", "ProjectAssignment", id, "Removed project assignment");
        return RedirectToAction(nameof(AssignProject), new { projectId = assignment.ProjectId });
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
            ModuleName = "Finance",
            Metadata = metadata,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
