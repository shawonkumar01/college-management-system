using CollegeManagement.Core.DTOs;
using CollegeManagement.Core.Entities;
using CollegeManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StudentController> _logger;

        public StudentController(AppDbContext context, ILogger<StudentController> logger)
        {
            _context = context;
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

        private bool IsStudent()
        {
            return GetCurrentUserRole() == UserRole.Student;
        }

        // Get student's enrolled classes
        [HttpGet("my-classes")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyClasses()
        {
            try
            {
                var studentId = GetCurrentUserId();
                var classes = await _context.Enrollments
                    .Include(e => e.Class)
                    .Where(e => e.StudentId == studentId)
                    .Select(e => new ClassDto
                    {
                        Id = e.Class.Id,
                        Name = e.Class.Name,
                        Code = e.Class.Code,
                        Description = e.Class.Description,
                        TeacherId = e.Class.TeacherId,
                        TeacherName = e.Class.Teacher != null ? $"{e.Class.Teacher.FirstName} {e.Class.Teacher.LastName}" : null,
                        CreatedAt = e.Class.CreatedAt
                    })
                    .ToListAsync();

                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student classes");
                return StatusCode(500, new { message = "An error occurred while retrieving classes" });
            }
        }

        // Get assignments for student's classes
        [HttpGet("assignments")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyAssignments()
        {
            try
            {
                var studentId = GetCurrentUserId();
                var enrolledClassIds = await _context.Enrollments
                    .Where(e => e.StudentId == studentId)
                    .Select(e => e.ClassId)
                    .ToListAsync();

                var assignments = await _context.Assignments
                    .Include(a => a.Class)
                    .Include(a => a.Subject)
                    .Where(a => enrolledClassIds.Contains(a.ClassId) && a.Status == AssignmentStatus.Published)
                    .Select(a => new StudentAssignmentDto
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Description = a.Description,
                        Deadline = a.Deadline,
                        MaxMarks = a.MaxMarks,
                        AllowLateSubmission = a.AllowLateSubmission,
                        CreatedAt = a.CreatedAt,
                        ClassId = a.ClassId,
                        ClassName = a.Class.Name,
                        SubjectId = a.SubjectId,
                        SubjectName = a.Subject.Name,
                        HasSubmitted = _context.Submissions.Any(s => s.AssignmentId == a.Id && s.StudentId == studentId)
                    })
                    .ToListAsync();

                // Populate submission details for submitted assignments
                foreach (var assignment in assignments)
                {
                    if (assignment.HasSubmitted)
                    {
                        var submission = await _context.Submissions
                            .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == studentId);
                        
                        if (submission != null)
                        {
                            assignment.Submission = new SubmissionDto
                            {
                                Id = submission.Id,
                                Answer = submission.Answer,
                                Status = submission.Status,
                                MarksObtained = submission.MarksObtained,
                                Feedback = submission.Feedback,
                                SubmittedAt = submission.SubmittedAt,
                                ReviewedAt = submission.ReviewedAt,
                                AssignmentId = submission.AssignmentId,
                                AssignmentTitle = assignment.Title,
                                StudentId = submission.StudentId,
                                StudentName = "",
                                StudentEmail = ""
                            };
                        }
                    }
                }

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student assignments");
                return StatusCode(500, new { message = "An error occurred while retrieving assignments" });
            }
        }

        [HttpGet("assignments/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var enrolledClassIds = await _context.Enrollments
                    .Where(e => e.StudentId == studentId)
                    .Select(e => e.ClassId)
                    .ToListAsync();

                var assignment = await _context.Assignments
                    .Include(a => a.Class)
                    .Include(a => a.Subject)
                    .FirstOrDefaultAsync(a => a.Id == id && enrolledClassIds.Contains(a.ClassId) && a.Status == AssignmentStatus.Published);

                if (assignment == null)
                {
                    return NotFound(new { message = "Assignment not found" });
                }

                var hasSubmitted = await _context.Submissions
                    .AnyAsync(s => s.AssignmentId == id && s.StudentId == studentId);

                SubmissionDto? submission = null;
                if (hasSubmitted)
                {
                    var sub = await _context.Submissions
                        .FirstOrDefaultAsync(s => s.AssignmentId == id && s.StudentId == studentId);
                    
                    if (sub != null)
                    {
                        submission = new SubmissionDto
                        {
                            Id = sub.Id,
                            Answer = sub.Answer,
                            Status = sub.Status,
                            MarksObtained = sub.MarksObtained,
                            Feedback = sub.Feedback,
                            SubmittedAt = sub.SubmittedAt,
                            ReviewedAt = sub.ReviewedAt,
                            AssignmentId = sub.AssignmentId,
                            AssignmentTitle = assignment.Title,
                            StudentId = sub.StudentId,
                            StudentName = "",
                            StudentEmail = ""
                        };
                    }
                }

                var assignmentDto = new StudentAssignmentDto
                {
                    Id = assignment.Id,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    Deadline = assignment.Deadline,
                    MaxMarks = assignment.MaxMarks,
                    AllowLateSubmission = assignment.AllowLateSubmission,
                    CreatedAt = assignment.CreatedAt,
                    ClassId = assignment.ClassId,
                    ClassName = assignment.Class.Name,
                    SubjectId = assignment.SubjectId,
                    SubjectName = assignment.Subject.Name,
                    HasSubmitted = hasSubmitted,
                    Submission = submission
                };

                return Ok(assignmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignment");
                return StatusCode(500, new { message = "An error occurred while retrieving assignment" });
            }
        }

        // Submit assignment
        [HttpPost("assignments/{assignmentId}/submissions")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitAssignment(int assignmentId, [FromBody] CreateSubmissionRequest request)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var enrolledClassIds = await _context.Enrollments
                    .Where(e => e.StudentId == studentId)
                    .Select(e => e.ClassId)
                    .ToListAsync();

                var assignment = await _context.Assignments
                    .FirstOrDefaultAsync(a => a.Id == assignmentId && enrolledClassIds.Contains(a.ClassId) && a.Status == AssignmentStatus.Published);

                if (assignment == null)
                {
                    return NotFound(new { message = "Assignment not found" });
                }

                // Check if already submitted
                var existingSubmission = await _context.Submissions
                    .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

                if (existingSubmission != null)
                {
                    return BadRequest(new { message = "Assignment already submitted" });
                }

                // Check deadline
                if (!assignment.AllowLateSubmission && DateTime.UtcNow > assignment.Deadline)
                {
                    return BadRequest(new { message = "Assignment deadline has passed" });
                }

                var submission = new Submission
                {
                    Answer = request.Answer,
                    Status = SubmissionStatus.Submitted,
                    AssignmentId = assignmentId,
                    StudentId = studentId,
                    SubmittedAt = DateTime.UtcNow
                };

                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();

                var submissionDto = new StudentSubmissionDto
                {
                    Id = submission.Id,
                    Answer = submission.Answer,
                    Status = submission.Status,
                    SubmittedAt = submission.SubmittedAt,
                    AssignmentId = submission.AssignmentId,
                    AssignmentTitle = assignment.Title,
                    AssignmentDeadline = assignment.Deadline,
                    AssignmentMaxMarks = assignment.MaxMarks,
                    CanUpdate = assignment.AllowLateSubmission || DateTime.UtcNow <= assignment.Deadline
                };

                return CreatedAtAction(nameof(GetMySubmissions), new { id = submission.Id }, submissionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting assignment");
                return StatusCode(500, new { message = "An error occurred while submitting assignment" });
            }
        }

        // Update submission
        [HttpPut("submissions/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateSubmission(int id, [FromBody] UpdateSubmissionRequest request)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var submission = await _context.Submissions
                    .Include(s => s.Assignment)
                    .FirstOrDefaultAsync(s => s.Id == id && s.StudentId == studentId);

                if (submission == null)
                {
                    return NotFound(new { message = "Submission not found" });
                }

                // Check if already reviewed
                if (submission.Status != SubmissionStatus.Submitted)
                {
                    return BadRequest(new { message = "Cannot update submission after it has been reviewed" });
                }

                // Check deadline
                if (!submission.Assignment.AllowLateSubmission && DateTime.UtcNow > submission.Assignment.Deadline)
                {
                    return BadRequest(new { message = "Assignment deadline has passed" });
                }

                submission.Answer = request.Answer;
                submission.SubmittedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var submissionDto = new StudentSubmissionDto
                {
                    Id = submission.Id,
                    Answer = submission.Answer,
                    Status = submission.Status,
                    SubmittedAt = submission.SubmittedAt,
                    AssignmentId = submission.AssignmentId,
                    AssignmentTitle = submission.Assignment.Title,
                    AssignmentDeadline = submission.Assignment.Deadline,
                    AssignmentMaxMarks = submission.Assignment.MaxMarks,
                    CanUpdate = submission.Assignment.AllowLateSubmission || DateTime.UtcNow <= submission.Assignment.Deadline
                };

                return Ok(submissionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating submission");
                return StatusCode(500, new { message = "An error occurred while updating submission" });
            }
        }

        // Get student's submissions
        [HttpGet("submissions")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMySubmissions()
        {
            try
            {
                var studentId = GetCurrentUserId();
                var submissions = await _context.Submissions
                    .Include(s => s.Assignment)
                    .Where(s => s.StudentId == studentId)
                    .Select(s => new StudentSubmissionDto
                    {
                        Id = s.Id,
                        Answer = s.Answer,
                        Status = s.Status,
                        MarksObtained = s.MarksObtained,
                        Feedback = s.Feedback,
                        SubmittedAt = s.SubmittedAt,
                        ReviewedAt = s.ReviewedAt,
                        AssignmentId = s.AssignmentId,
                        AssignmentTitle = s.Assignment.Title,
                        AssignmentDeadline = s.Assignment.Deadline,
                        AssignmentMaxMarks = s.Assignment.MaxMarks,
                        CanUpdate = s.Status == SubmissionStatus.Submitted && 
                                     (s.Assignment.AllowLateSubmission || DateTime.UtcNow <= s.Assignment.Deadline)
                    })
                    .ToListAsync();

                return Ok(submissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student submissions");
                return StatusCode(500, new { message = "An error occurred while retrieving submissions" });
            }
        }

        [HttpGet("submissions/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetSubmissionById(int id)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var submission = await _context.Submissions
                    .Include(s => s.Assignment)
                    .FirstOrDefaultAsync(s => s.Id == id && s.StudentId == studentId);

                if (submission == null)
                {
                    return NotFound(new { message = "Submission not found" });
                }

                var submissionDto = new StudentSubmissionDto
                {
                    Id = submission.Id,
                    Answer = submission.Answer,
                    Status = submission.Status,
                    MarksObtained = submission.MarksObtained,
                    Feedback = submission.Feedback,
                    SubmittedAt = submission.SubmittedAt,
                    ReviewedAt = submission.ReviewedAt,
                    AssignmentId = submission.AssignmentId,
                    AssignmentTitle = submission.Assignment.Title,
                    AssignmentDeadline = submission.Assignment.Deadline,
                    AssignmentMaxMarks = submission.Assignment.MaxMarks,
                    CanUpdate = submission.Status == SubmissionStatus.Submitted && 
                                 (submission.Assignment.AllowLateSubmission || DateTime.UtcNow <= submission.Assignment.Deadline)
                };

                return Ok(submissionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submission");
                return StatusCode(500, new { message = "An error occurred while retrieving submission" });
            }
        }
    }
}
