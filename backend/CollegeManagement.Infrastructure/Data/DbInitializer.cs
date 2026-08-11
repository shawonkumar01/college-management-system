using CollegeManagement.Core.Entities;
using CollegeManagement.Infrastructure.Services;
using CollegeManagement.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace CollegeManagement.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(AppDbContext context, IAuthService authService)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Users.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Create Admin
            var admin = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@college.edu",
                PasswordHash = authService.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(admin);

            // Create Teachers
            var teacher1 = new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "teacher@college.edu",
                PasswordHash = authService.HashPassword("Teacher123!"),
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(teacher1);

            var teacher2 = new User
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@college.edu",
                PasswordHash = authService.HashPassword("Teacher123!"),
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(teacher2);

            // Create Students
            var student1 = new User
            {
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "student@college.edu",
                PasswordHash = authService.HashPassword("Student123!"),
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(student1);

            var student2 = new User
            {
                FirstName = "Bob",
                LastName = "Williams",
                Email = "bob.williams@college.edu",
                PasswordHash = authService.HashPassword("Student123!"),
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(student2);

            var student3 = new User
            {
                FirstName = "Charlie",
                LastName = "Brown",
                Email = "charlie.brown@college.edu",
                PasswordHash = authService.HashPassword("Student123!"),
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(student3);

            await context.SaveChangesAsync();

            // Create Classes
            var class1 = new Class
            {
                Name = "Computer Science 101",
                Code = "CS101",
                Description = "Introduction to Computer Science",
                TeacherId = teacher1.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Classes.Add(class1);

            var class2 = new Class
            {
                Name = "Mathematics 101",
                Code = "MATH101",
                Description = "Calculus I",
                TeacherId = teacher2.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Classes.Add(class2);

            await context.SaveChangesAsync();

            // Create Subjects
            var subject1 = new Subject
            {
                Name = "Programming Fundamentals",
                Code = "PF101",
                Description = "Introduction to programming concepts",
                TeacherId = teacher1.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Subjects.Add(subject1);

            var subject2 = new Subject
            {
                Name = "Data Structures",
                Code = "DS201",
                Description = "Advanced data structures and algorithms",
                TeacherId = teacher1.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Subjects.Add(subject2);

            var subject3 = new Subject
            {
                Name = "Calculus I",
                Code = "CALC101",
                Description = "Differential and integral calculus",
                TeacherId = teacher2.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Subjects.Add(subject3);

            await context.SaveChangesAsync();

            // Enroll students in classes
            context.Enrollments.Add(new Enrollment
            {
                ClassId = class1.Id,
                StudentId = student1.Id,
                EnrolledAt = DateTime.UtcNow
            });

            context.Enrollments.Add(new Enrollment
            {
                ClassId = class1.Id,
                StudentId = student2.Id,
                EnrolledAt = DateTime.UtcNow
            });

            context.Enrollments.Add(new Enrollment
            {
                ClassId = class2.Id,
                StudentId = student1.Id,
                EnrolledAt = DateTime.UtcNow
            });

            context.Enrollments.Add(new Enrollment
            {
                ClassId = class2.Id,
                StudentId = student3.Id,
                EnrolledAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            // Create sample assignments
            var assignment1 = new Assignment
            {
                Title = "Hello World Program",
                Description = "Write a simple Hello World program in your preferred language",
                Deadline = DateTime.UtcNow.AddDays(7),
                MaxMarks = 100,
                Status = AssignmentStatus.Published,
                AllowLateSubmission = false,
                ClassId = class1.Id,
                SubjectId = subject1.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Assignments.Add(assignment1);

            var assignment2 = new Assignment
            {
                Title = "Linked List Implementation",
                Description = "Implement a linked list with insert, delete, and search operations",
                Deadline = DateTime.UtcNow.AddDays(14),
                MaxMarks = 100,
                Status = AssignmentStatus.Published,
                AllowLateSubmission = true,
                ClassId = class1.Id,
                SubjectId = subject2.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Assignments.Add(assignment2);

            var assignment3 = new Assignment
            {
                Title = "Derivative Problems",
                Description = "Solve the following derivative problems",
                Deadline = DateTime.UtcNow.AddDays(5),
                MaxMarks = 50,
                Status = AssignmentStatus.Published,
                AllowLateSubmission = false,
                ClassId = class2.Id,
                SubjectId = subject3.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Assignments.Add(assignment3);

            await context.SaveChangesAsync();

            // Create sample submissions
            context.Submissions.Add(new Submission
            {
                Answer = "print('Hello World')",
                Status = SubmissionStatus.Reviewed,
                MarksObtained = 95,
                Feedback = "Great work! Keep it up.",
                AssignmentId = assignment1.Id,
                StudentId = student1.Id,
                SubmittedAt = DateTime.UtcNow.AddDays(-1),
                ReviewedAt = DateTime.UtcNow
            });

            context.Submissions.Add(new Submission
            {
                Answer = "Here is my linked list implementation...",
                Status = SubmissionStatus.Submitted,
                AssignmentId = assignment2.Id,
                StudentId = student1.Id,
                SubmittedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }
}
