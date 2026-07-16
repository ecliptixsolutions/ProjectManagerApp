using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Models;

namespace ProjectManagerApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Revenue> Revenues => Set<Revenue>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<SalaryPayment> SalaryPayments => Set<SalaryPayment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();
    public DbSet<DailyWorkUpdate> DailyWorkUpdates => Set<DailyWorkUpdate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Revenue>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SalaryPayment>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<ProjectAssignment>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<DailyWorkUpdate>().HasQueryFilter(w => !w.IsDeleted);

        modelBuilder.Entity<Revenue>().ToTable(tb => tb.HasCheckConstraint("CK_Revenues_Amount_Positive", "Amount > 0"));
        modelBuilder.Entity<Expense>().ToTable(tb => tb.HasCheckConstraint("CK_Expenses_Amount_Positive", "Amount > 0"));
        modelBuilder.Entity<SalaryPayment>().ToTable(tb => tb.HasCheckConstraint("CK_SalaryPayments_NetSalary_Positive", "NetSalary >= 0"));
        modelBuilder.Entity<Employee>().ToTable(tb => tb.HasCheckConstraint("CK_Employees_SalaryAmount_NonNegative", "SalaryAmount >= 0"));

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<AuditLog>()
            .Property(a => a.Timestamp)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<ProjectAssignment>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.Assignments)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectAssignment>()
            .HasOne(a => a.Project)
            .WithMany(p => p.Assignments)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DailyWorkUpdate>()
            .HasOne(w => w.Employee)
            .WithMany(e => e.WorkUpdates)
            .HasForeignKey(w => w.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DailyWorkUpdate>()
            .HasOne(w => w.Project)
            .WithMany(p => p.WorkUpdates)
            .HasForeignKey(w => w.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.AppUser)
            .WithMany()
            .HasForeignKey(e => e.AppUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Revenue>()
            .HasOne(r => r.Project)
            .WithMany(p => p.Revenues)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Project)
            .WithMany(p => p.Expenses)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SalaryPayment>()
            .HasOne(s => s.Project)
            .WithMany(p => p.SalaryPayments)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
