using CollegeManagement.Core.DTOs;
using CollegeManagement.Core.Entities;
using CollegeManagement.Infrastructure.Data;
using CollegeManagement.Infrastructure.Services;
using CollegeManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, IAuthService authService, ILogger<AdminController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Student;
        }

        private bool IsAdmin()
        {
            return GetCurrentUserRole() == UserRole.Admin;
        }

        // User Management
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = u.Role
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PasswordHash = _authService.HashPassword(request.Password),
                    Role = request.Role,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role
                };

                return CreatedAtAction(nameof(GetAllUsers), userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "An error occurred while creating user" });
            }
        }

        [HttpPut("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                if (!string.IsNullOrEmpty(request.FirstName))
                    user.FirstName = request.FirstName;
                if (!string.IsNullOrEmpty(request.LastName))
                    user.LastName = request.LastName;
                if (!string.IsNullOrEmpty(request.Email))
                    user.Email = request.Email;
                if (request.Role.HasValue)
                    user.Role = request.Role.Value;

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, new { message = "An error occurred while updating user" });
            }
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, new { message = "An error occurred while deleting user" });
            }
        }

        // Class Management
        [HttpGet("classes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllClasses()
        {
            try
            {
                var classes = await _context.Classes
                    .Include(c => c.Teacher)
                    .Select(c => new ClassDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Code = c.Code,
                        Description = c.Description,
                        TeacherId = c.TeacherId,
                        TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : null,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all classes");
                return StatusCode(500, new { message = "An error occurred while retrieving classes" });
            }
        }

        [HttpPost("classes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
        {
            try
            {
                if (request.TeacherId.HasValue)
                {
                    var teacher = await _context.Users.FindAsync(request.TeacherId.Value);
                    if (teacher == null || teacher.Role != UserRole.Teacher)
                    {
                        return BadRequest(new { message = "Invalid teacher" });
                    }
                }

                var classEntity = new Class
                {
                    Name = request.Name,
                    Code = request.Code,
                    Description = request.Description,
                    TeacherId = request.TeacherId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Classes.Add(classEntity);
                await _context.SaveChangesAsync();

                var classDto = new ClassDto
                {
                    Id = classEntity.Id,
                    Name = classEntity.Name,
                    Code = classEntity.Code,
                    Description = classEntity.Description,
                    TeacherId = classEntity.TeacherId,
                    CreatedAt = classEntity.CreatedAt
                };

                return CreatedAtAction(nameof(GetAllClasses), classDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating class");
                return StatusCode(500, new { message = "An error occurred while creating class" });
            }
        }

        [HttpPut("classes/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] UpdateClassRequest request)
        {
            try
            {
                var classEntity = await _context.Classes.FindAsync(id);
                if (classEntity == null)
                {
                    return NotFound(new { message = "Class not found" });
                }

                if (request.TeacherId.HasValue)
                {
                    var teacher = await _context.Users.FindAsync(request.TeacherId.Value);
                    if (teacher == null || teacher.Role != UserRole.Teacher)
                    {
                        return BadRequest(new { message = "Invalid teacher" });
                    }
                }

                if (!string.IsNullOrEmpty(request.Name))
                    classEntity.Name = request.Name;
                if (!string.IsNullOrEmpty(request.Code))
                    classEntity.Code = request.Code;
                if (!string.IsNullOrEmpty(request.Description))
                    classEntity.Description = request.Description;
                if (request.TeacherId.HasValue)
                    classEntity.TeacherId = request.TeacherId;

                classEntity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var classDto = new ClassDto
                {
                    Id = classEntity.Id,
                    Name = classEntity.Name,
                    Code = classEntity.Code,
                    Description = classEntity.Description,
                    TeacherId = classEntity.TeacherId,
                    CreatedAt = classEntity.CreatedAt
                };

                return Ok(classDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating class");
                return StatusCode(500, new { message = "An error occurred while updating class" });
            }
        }

        [HttpDelete("classes/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            try
            {
                var classEntity = await _context.Classes.FindAsync(id);
                if (classEntity == null)
                {
                    return NotFound(new { message = "Class not found" });
                }

                _context.Classes.Remove(classEntity);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Class deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting class");
                return StatusCode(500, new { message = "An error occurred while deleting class" });
            }
        }

        // Subject Management
        [HttpGet("subjects")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSubjects()
        {
            try
            {
                var subjects = await _context.Subjects
                    .Include(s => s.Teacher)
                    .Select(s => new SubjectDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Code = s.Code,
                        Description = s.Description,
                        TeacherId = s.TeacherId,
                        TeacherName = s.Teacher != null ? $"{s.Teacher.FirstName} {s.Teacher.LastName}" : null,
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();

                return Ok(subjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all subjects");
                return StatusCode(500, new { message = "An error occurred while retrieving subjects" });
            }
        }

        [HttpPost("subjects")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequest request)
        {
            try
            {
                if (request.TeacherId.HasValue)
                {
                    var teacher = await _context.Users.FindAsync(request.TeacherId.Value);
                    if (teacher == null || teacher.Role != UserRole.Teacher)
                    {
                        return BadRequest(new { message = "Invalid teacher" });
                    }
                }

                var subject = new Subject
                {
                    Name = request.Name,
                    Code = request.Code,
                    Description = request.Description,
                    TeacherId = request.TeacherId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();

                var subjectDto = new SubjectDto
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    Code = subject.Code,
                    Description = subject.Description,
                    TeacherId = subject.TeacherId,
                    CreatedAt = subject.CreatedAt
                };

                return CreatedAtAction(nameof(GetAllSubjects), subjectDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject");
                return StatusCode(500, new { message = "An error occurred while creating subject" });
            }
        }

        [HttpPut("subjects/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectRequest request)
        {
            try
            {
                var subject = await _context.Subjects.FindAsync(id);
                if (subject == null)
                {
                    return NotFound(new { message = "Subject not found" });
                }

                if (request.TeacherId.HasValue)
                {
                    var teacher = await _context.Users.FindAsync(request.TeacherId.Value);
                    if (teacher == null || teacher.Role != UserRole.Teacher)
                    {
                        return BadRequest(new { message = "Invalid teacher" });
                    }
                }

                if (!string.IsNullOrEmpty(request.Name))
                    subject.Name = request.Name;
                if (!string.IsNullOrEmpty(request.Code))
                    subject.Code = request.Code;
                if (!string.IsNullOrEmpty(request.Description))
                    subject.Description = request.Description;
                if (request.TeacherId.HasValue)
                    subject.TeacherId = request.TeacherId;

                subject.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var subjectDto = new SubjectDto
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    Code = subject.Code,
                    Description = subject.Description,
                    TeacherId = subject.TeacherId,
                    CreatedAt = subject.CreatedAt
                };

                return Ok(subjectDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject");
                return StatusCode(500, new { message = "An error occurred while updating subject" });
            }
        }

        [HttpDelete("subjects/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            try
            {
                var subject = await _context.Subjects.FindAsync(id);
                if (subject == null)
                {
                    return NotFound(new { message = "Subject not found" });
                }

                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Subject deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject");
                return StatusCode(500, new { message = "An error occurred while deleting subject" });
            }
        }

        // Enrollment Management
        [HttpPost("enrollments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEnrollment([FromBody] CreateEnrollmentRequest request)
        {
            try
            {
                var student = await _context.Users.FindAsync(request.StudentId);
                if (student == null || student.Role != UserRole.Student)
                {
                    return BadRequest(new { message = "Invalid student" });
                }

                var classEntity = await _context.Classes.FindAsync(request.ClassId);
                if (classEntity == null)
                {
                    return BadRequest(new { message = "Invalid class" });
                }

                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.ClassId == request.ClassId && e.StudentId == request.StudentId);

                if (existingEnrollment != null)
                {
                    return BadRequest(new { message = "Student already enrolled in this class" });
                }

                var enrollment = new Enrollment
                {
                    ClassId = request.ClassId,
                    StudentId = request.StudentId,
                    EnrolledAt = DateTime.UtcNow
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Enrollment created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating enrollment");
                return StatusCode(500, new { message = "An error occurred while creating enrollment" });
            }
        }

        [HttpDelete("enrollments/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            try
            {
                var enrollment = await _context.Enrollments.FindAsync(id);
                if (enrollment == null)
                {
                    return NotFound(new { message = "Enrollment not found" });
                }

                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Enrollment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting enrollment");
                return StatusCode(500, new { message = "An error occurred while deleting enrollment" });
            }
        }
    }
}
