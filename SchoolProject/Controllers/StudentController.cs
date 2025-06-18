using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolProject.DTOs;
using SchoolProject.DTOs.Paging;
using SchoolProject.Filter;
using SchoolProject.Model;
using SchoolProject.Repository;
using SchoolProject.Services;
using System.Reflection;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SchoolProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly StudentRepository _studentRepository;
        private readonly EncryptionService _encryptionService;


        public StudentController(StudentRepository studentRepository, EncryptionService encryptionService)
        {
            _studentRepository = studentRepository;

            _encryptionService = encryptionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromForm] StudentDTo student)
        {
            var result = await _studentRepository.AddStudent(student);

            if (result == 1)
            {
                return Ok(ApiResponse<string>.Ok(null, "Student saved successfully"));
            }

            return BadRequest(ApiResponse<string>.Fail("Failed to save student"));
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _studentRepository.GetAll();

            if (students == null || !students.Any())
            {
                return NotFound(ApiResponse<List<StudentRTos>>.Fail("No students found"));
            }

            return Ok(ApiResponse<List<StudentRTos>>.Ok(students, "Students retrieved successfully"));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Filter")] // Filtered + Paginated 
        public async Task<IActionResult> GetAllFilter([FromQuery] StudentFilter filter, [FromQuery] PaginationDto pagination)
        {
            var result = await _studentRepository.GetAllFilterData(filter, pagination);
            if (result == null || result.Data == null || !result.Data.Any())
            {
                return NotFound(ApiResponse<PagedResultDto<StudentRTos>>.Fail("No students found"));
            }
            return Ok(ApiResponse<PagedResultDto<StudentRTos>>.Ok(result, "Students retrieved successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _studentRepository.GetStudentById(id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        //both update partial and full
        [HttpPut("id")]
        public async Task<IActionResult> UpdateStudent(int id, [FromForm] StudentUpdateDTo partialupdatedStudent)
        {
            if (id != partialupdatedStudent.Id) return BadRequest("ID mismatch");

            var updated = await _studentRepository.UpdateStudents(partialupdatedStudent);

            if (!updated) return NotFound("Student not found");

            return Ok(" Partial Student updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var deletedStudent = await _studentRepository.DeleteStudents(id);
            return Ok(deletedStudent);
        }

        [HttpGet("export-excel")]
        public IActionResult ExportExcel()
        {
            var fileBytes = _studentRepository.ExportStudentsToExcel();

            var fileName = $"Students_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            var result = await _studentRepository.ImportStudentsFromExcelAsync(file);

            if (result > 0)
                return Ok(ApiResponse<string>.Ok(null, $"{result} students imported successfully"));

            return BadRequest(ApiResponse<string>.Fail("Failed to import students"));
        }

        [HttpGet("encrypted")]
        public async Task<ActionResult<List<StudentRTos>>> GetEncryptedStudents()
        {
            var students = await _studentRepository.GetAll();
            var encrypted = _studentRepository.EncryptStudents(students);
            return Ok(encrypted);
        }

        [HttpPost("decrypt-raw")]
        public ActionResult<List<StudentRTos>> DecryptStudentsFromString([FromBody] string encryptedString)
        {
            if (string.IsNullOrWhiteSpace(encryptedString))
            {
                return BadRequest("Encrypted string is required.");
            }

            try
            {
                var decryptedStudents = _studentRepository.DecryptStudents(encryptedString);
                return Ok(decryptedStudents);
            }
            catch (Exception ex)
            {
                return BadRequest($"Decryption failed: {ex.Message}");
            }
        }


    }
}