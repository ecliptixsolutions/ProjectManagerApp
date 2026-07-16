using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Data;
using ProjectManagerApp.Helpers;
using ProjectManagerApp.Models;

namespace ProjectManagerApp.Controllers;

public class DailyWorkController : Controller
{
    private readonly ApplicationDbContext _context;

    public DailyWorkController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AdminAuthorize]
    public async Task<IActionResult> Index(int? employeeId, int? projectId, DateTime? from, DateTime? to)
    {
        var query = _context.DailyWorkUpdates
            .Include(w => w.Employee)
            .Include(w => w.Project)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(w => w.EmployeeId == employeeId.Value);
        if (projectId.HasValue)
            query = query.Where(w => w.ProjectId == projectId.Value);
        if (from.HasValue)
            query = query.Where(w => w.WorkDate >= from.Value);
        if (to.HasValue)
            query = query.Where(w => w.WorkDate <= to.Value);

        var updates = await query.OrderByDescending(w => w.WorkDate).ThenByDescending(w => w.CreatedAt).ToListAsync();

        ViewBag.Employees = await _context.Employees.Where(e => e.Status == "Active").OrderBy(e => e.FullName).ToListAsync();
        ViewBag.Projects = await _context.Projects.OrderBy(p => p.Name).ToListAsync();
        return View(updates);
    }

    [Authorize]
    public async Task<IActionResult> MyUpdates()
    {
        var userId = GetCurrentUserId();
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.AppUserId == userId);
        if (employee == null)
            return Forbid();

        var updates = await _context.DailyWorkUpdates
            .Include(w => w.Project)
            .Where(w => w.EmployeeId == employee.Id)
            .OrderByDescending(w => w.WorkDate)
            .ToListAsync();

        ViewBag.Employee = employee;
        return View(updates);
    }

    [Authorize]
    public async Task<IActionResult> Create()
    {
        var userId = GetCurrentUserId();
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.AppUserId == userId);
        if (employee == null)
            return Forbid();

        var assignedProjects = await _context.ProjectAssignments
            .Include(a => a.Project)
            .Where(a => a.EmployeeId == employee.Id && a.Status == "Active" && !a.IsDeleted)
            .Select(a => a.Project)
            .ToListAsync();

        ViewBag.Employee = employee;
        ViewBag.Projects = assignedProjects;
        return View(new DailyWorkUpdate { WorkDate = DateTime.UtcNow.Date, EmployeeId = employee.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(DailyWorkUpdate update)
    {
        if (!ModelState.IsValid)
        {
            var userId = GetCurrentUserId();
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.AppUserId == userId);
            ViewBag.Employee = employee;
            ViewBag.Projects = await _context.ProjectAssignments
                .Include(a => a.Project)
                .Where(a => a.EmployeeId == employee!.Id && a.Status == "Active" && !a.IsDeleted)
                .Select(a => a.Project)
                .ToListAsync();
            return View(update);
        }

        update.CreatedAt = DateTime.UtcNow;
        update.UpdatedAt = DateTime.UtcNow;
        _context.DailyWorkUpdates.Add(update);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Work update submitted successfully.";
        return RedirectToAction(nameof(MyUpdates));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AdminAuthorize]
    public async Task<IActionResult> Delete(int id)
    {
        var update = await _context.DailyWorkUpdates.FindAsync(id);
        if (update is null)
            return NotFound();

        update.IsDeleted = true;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idValue, out var id) ? id : 0;
    }
}
