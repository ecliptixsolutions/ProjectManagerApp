using Microsoft.AspNetCore.Identity;
using ProjectManagerApp.Models;

namespace ProjectManagerApp.Data;

public static class SeedData
{
    public static void EnsureSeedData(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            var admin = new AppUser
            {
                UserName = "admin",
                Role = "Admin"
            };

            var hasher = new PasswordHasher<AppUser>();
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
            context.Users.Add(admin);

            context.SaveChanges();

            var empUser = new AppUser
            {
                UserName = "employee1",
                Role = "Employee"
            };
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
