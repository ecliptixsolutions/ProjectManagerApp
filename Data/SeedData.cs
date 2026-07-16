using Microsoft.AspNetCore.Identity;
using ProjectManagerApp.Models;

namespace ProjectManagerApp.Data;

public static class SeedData
{
    private static void EnsureUser(AppUser user, string password, ApplicationDbContext context, PasswordHasher<AppUser> hasher)
    {
        if (!context.Users.Any(u => u.UserName == user.UserName))
        {
            user.PasswordHash = hasher.HashPassword(user, password);
            context.Users.Add(user);
            context.SaveChanges();
        }
    }

    public static void EnsureSeedData(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        var hasher = new PasswordHasher<AppUser>();

        if (!context.Users.Any())
        {
            var admin = new AppUser { UserName = "admin", Role = "Admin" };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
            context.Users.Add(admin);
            context.SaveChanges();

            var empUser = new AppUser { UserName = "employee1", Role = "Employee" };
            empUser.PasswordHash = hasher.HashPassword(empUser, "Emp@123");
            context.Users.Add(empUser);
            context.SaveChanges();

            var employee = new Employee
            {
                EmployeeId = "EMP001",
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "9876543210",
                Designation = "Software Developer",
                Department = "Engineering",
                Status = "Active",
                SalaryAmount = 50000,
                SalaryType = "Monthly",
                JoinedDate = new DateTime(2024, 1, 15),
                EmploymentType = "Permanent",
                CreatedBy = 1,
                AppUserId = empUser.Id
            };
            context.Employees.Add(employee);
            context.SaveChanges();
        }

        EnsureUser(new AppUser { UserName = "parth_shah", Role = "Admin" }, "name@7071", context, hasher);
        EnsureUser(new AppUser { UserName = "parth_ram", Role = "Admin" }, "name@7071", context, hasher);
        EnsureUser(new AppUser { UserName = "swet_patel", Role = "Admin" }, "name@7071", context, hasher);

        if (!context.Projects.Any())
        {
            context.Projects.AddRange(
                new Project
                {
                    Name = "Project Alpha",
                    ClientName = "Client A",
                    Description = "Sample project for revenue and expense tracking.",
                    Status = "InProgress",
                    PaymentStatus = "PartialReceived",
                    CreatedBy = 1
                },
                new Project
                {
                    Name = "Project Beta",
                    ClientName = "Client B",
                    Description = "Testing project for salary allocation.",
                    Status = "New",
                    PaymentStatus = "Pending",
                    CreatedBy = 1
                }
            );
            context.SaveChanges();

            var project = context.Projects.First();
            var employee = context.Employees.First();
            if (employee != null && project != null)
            {
                context.ProjectAssignments.Add(new ProjectAssignment
                {
                    EmployeeId = employee.Id,
                    ProjectId = project.Id,
                    Role = "Developer",
                    AssignedBy = 1,
                    AssignmentDate = DateTime.UtcNow.Date
                });
                context.SaveChanges();
            }
        }
    }
}
