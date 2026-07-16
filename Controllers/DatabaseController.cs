using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Data;
using ProjectManagerApp.Models;

namespace ProjectManagerApp.Controllers;

[Authorize(Roles = "Admin")]
public class DatabaseController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly string _backupDir;

    public DatabaseController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
        _backupDir = Path.Combine(_env.ContentRootPath, "backups");
        Directory.CreateDirectory(_backupDir);
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity?.Name != "swet_patel")
            return Forbid();

        var backups = new DirectoryInfo(_backupDir)
            .GetFiles("*.db")
            .OrderByDescending(f => f.LastWriteTime)
            .Select(f => new BackupEntry
            {
                FileName = f.Name,
                SizeBytes = f.Length,
                CreatedAt = f.LastWriteTime
            })
            .ToList();

        return View(backups);
    }

    [HttpGet]
    public IActionResult Download(string fileName)
    {
        if (User.Identity?.Name != "swet_patel")
            return Forbid();

        var path = Path.Combine(_backupDir, fileName);
        if (!System.IO.File.Exists(path))
            return NotFound();

        return PhysicalFile(path, "application/octet-stream", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBackup()
    {
        if (User.Identity?.Name != "swet_patel")
            return Forbid();

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var dbPath = GetDbPath();
        var backupPath = Path.Combine(_backupDir, $"backup_{timestamp}.db");

        if (System.IO.File.Exists(dbPath))
        {
            System.IO.File.Copy(dbPath, backupPath, overwrite: true);

            await _context.AuditLogs.AddAsync(new AuditLog
            {
                UserId = await GetUserId(),
                UserName = User.Identity?.Name ?? "unknown",
                ActionType = "Backup",
                EntityType = "Database",
                ModuleName = "Database",
                NewValue = $"Backup created: backup_{timestamp}.db"
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Backup created: backup_{timestamp}.db";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(string fileName)
    {
        if (User.Identity?.Name != "swet_patel")
            return Forbid();

        var backupPath = Path.Combine(_backupDir, fileName);
        if (!System.IO.File.Exists(backupPath))
        {
            TempData["Error"] = "Backup file not found.";
            return RedirectToAction(nameof(Index));
        }

        var dbPath = GetDbPath();
        System.IO.File.Copy(backupPath, dbPath, overwrite: true);

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = await GetUserId(),
            UserName = User.Identity?.Name ?? "unknown",
            ActionType = "Restore",
            EntityType = "Database",
            ModuleName = "Database",
            NewValue = $"Restored from: {fileName}"
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Database restored from: {fileName}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string fileName)
    {
        if (User.Identity?.Name != "swet_patel")
            return Forbid();

        var path = Path.Combine(_backupDir, fileName);
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);

        TempData["Success"] = $"Deleted: {fileName}";
        return RedirectToAction(nameof(Index));
    }

    private string GetDbPath()
    {
        var connStr = _context.Database.GetConnectionString();
        if (connStr != null && connStr.StartsWith("Data Source="))
            return connStr["Data Source=".Length..];
        return Path.Combine(_env.ContentRootPath, "app.db");
    }

    private async Task<int> GetUserId()
    {
        var name = User.Identity?.Name;
        if (name == null) return 0;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == name);
        return user?.Id ?? 0;
    }
}


