using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Data;
using ProjectManagerApp.Helpers;

namespace ProjectManagerApp.Controllers;

[AdminAuthorize]
public class AuditLogsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuditLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? actionType, string? module, string? search)
    {
        var query = _context.AuditLogs
            .Include(l => l.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(actionType))
            query = query.Where(l => l.ActionType == actionType);
        if (!string.IsNullOrWhiteSpace(module))
            query = query.Where(l => l.ModuleName == module);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l =>
                (l.UserName ?? string.Empty).Contains(search) ||
                (l.Metadata ?? string.Empty).Contains(search) ||
                (l.EntityType ?? string.Empty).Contains(search));

        var logs = await query.OrderByDescending(l => l.Timestamp).Take(500).ToListAsync();

        ViewBag.ActionTypes = await _context.AuditLogs.Select(l => l.ActionType).Distinct().ToListAsync();
        ViewBag.Modules = await _context.AuditLogs.Select(l => l.ModuleName).Distinct().ToListAsync();
        return View(logs);
    }
}
