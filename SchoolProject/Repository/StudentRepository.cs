using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OfficeOpenXml;
using SchoolProject.Connection;
using SchoolProject.DTOs;
using SchoolProject.DTOs.Paging;
using SchoolProject.Filter;
using SchoolProject.Model;
using SchoolProject.Services;
using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text.Json;
using System.Xml.Linq;

namespace SchoolProject.Repository
{
    public class StudentRepository
    {
        private readonly AppDbContext _Context;
        private readonly EncryptionService _encryptionService;

        public StudentRepository(AppDbContext appDbContext, EncryptionService encryptionService)
        {
            this._Context = appDbContext;
            _encryptionService = encryptionService;
        }
        public async Task<int> AddStudent([FromForm] StudentDTo studentDto)
        {
            //using var transaction = await _Context.Database.BeginTransactionAsync();
            //try
            //{
            var address = new StudentAddress
            {
                AddressName = studentDto.AddressName
            };
            var course = new StudentCourse
            {
                Title = studentDto.Title
            };
            var studentEntity = new StudentModel
            {
                Name = studentDto.Name,
                Age = studentDto.Age,
                Gender = studentDto.Gender.ToString(),
                RollNumber = studentDto.RollNumber,
                Description = studentDto.Description,
                Address = address,
                Courses = course
            };
            _Context.Students.Add(studentEntity);
            await _Context.SaveChangesAsync();
            return 1;
            //    await transaction.CommitAsync();

            //    return 1;
            //}
            //catch (Exception)
            //{
            //    // Rollback if any error occurs
            //    await transaction.RollbackAsync();
            //    throw; // Optional: rethrow or handle the exception
            //}
        }
        public async Task<List<StudentRTos>> GetAll()
        {
            var students = await _Context.Students
                .Include(s => s.Address)
                .Include(s => s.Courses)
                .ToListAsync();

            var studentDtos = students.Select(x => new StudentRTos
            {
                Id = x.Id,
                Name = x.Name,
                Age = x.Age,
                Gender = x.Gender,
                RollNumber = x.RollNumber,
                Description = x.Description,
                AddressName = x.Address.AddressName,
                Titles = x.Courses.Title

            }).ToList();

            return studentDtos;
        }
        public async Task<StudentRTos> GetStudentById(int id)
        {
            var student = await _Context.Students
                                 .Include(s => s.Address)
                                 .Include(c => c.Courses)
                                 .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return null;
            return new StudentRTos
            {
                Id = student.Id,
                Name = student.Name,
                Age = student.Age,
                Gender = student.Gender,
                RollNumber = student.RollNumber,
                Description = student.Description,
                AddressName = student.Address.AddressName,
                Titles = student.Courses.Title,

            };
        }
        public async Task<bool> UpdateStudents(StudentUpdateDTo dto)
        {
            {
                var student = await _Context.Students
                                            .Include(s => s.Address)
                                            .Include(c => c.Courses)
                                            .FirstOrDefaultAsync(s => s.Id == dto.Id);
                if (student == null) throw new Exception("Student not found");

                if (dto.Name != null) student.Name = dto.Name;
                if (dto.Age.HasValue) student.Age = dto.Age.Value;
                if (dto.Gender != null) student.Gender = dto.Gender;
                if (dto.RollNumber.HasValue) student.RollNumber = dto.RollNumber.Value;
                if (dto.Description != null) student.Description = dto.Description;

                //Update nested Address if exists
                if (student.Address != null)
                {
                    if (dto.AddressName != null) student.Address.AddressName = dto.AddressName;
                }
                if (dto.Title != null)
                {
                    student.Courses.Title = dto.Title;

                }
                await _Context.SaveChangesAsync();
                return true;
            }
        }
        public async Task<bool> DeleteStudents(int id)
        {
            var student = await _Context.Students
                .Include(s => s.Address)
                .Include(s => s.Courses)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return false;
            if (student.Address != null)
            {
                _Context.StudentAddresss.Remove(student.Address);
            }

            if (student.Courses != null)
            {
                _Context.StudentCourses.Remove(student.Courses);
            }

            _Context.Students.Remove(student);
            await _Context.SaveChangesAsync();

            return true;
        }
        public byte[] ExportStudentsToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var students = _Context.Students
                .Include(s => s.Address)
                .Include(s => s.Courses)
                .ToList();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Students");

            //worksheet.Cells[1, 1].Value = "Id";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Age";
            worksheet.Cells[1, 4].Value = "Gender";
            worksheet.Cells[1, 5].Value = "Description";
            worksheet.Cells[1, 6].Value = "AddressName";
            worksheet.Cells[1, 7].Value = "Title";

            int row = 2;
            foreach (var s in students)
            {
                worksheet.Cells[1, 1].Value = s.Id;
                worksheet.Cells[row, 2].Value = s.Name;
                worksheet.Cells[row, 3].Value = s.Age;
                worksheet.Cells[row, 4].Value = s.Gender;
                worksheet.Cells[row, 5].Value = s.Description;
                row++;
            }
            var studentAddress = new StudentAddress
            {
                AddressName = worksheet.Cells[row, 5].Text,

            };
            var StuCourse = new StudentCourse
            {
                Title = worksheet.Cells[row, 6].Text,
            };

            return package.GetAsByteArray();
        }
        public async Task<int> ImportStudentsFromExcelAsync(IFormFile file)
        {
            if (file == null || file.Length <= 0)
                return 0;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            var studentsToAdd = new List<StudentModel>();

            for (int row = 2; row <= rowCount; row++)
            {
                var name = worksheet.Cells[row, 2].Text.Trim();
                var ageText = worksheet.Cells[row, 3].Text.Trim();
                var gender = worksheet.Cells[row, 4].Text.Trim();
                var addressName = worksheet.Cells[row, 5].Text.Trim();
                var Description = worksheet.Cells[row, 6].Text.Trim();
                var courseTitle = worksheet.Cells[row, 6].Text.Trim();

                if (!int.TryParse(ageText, out int age))
                    continue;

                // chech student Exist or not
                bool exists = await _Context.Students
                    .AnyAsync(s => s.Name == name && s.Age == age);
                if (exists)
                    continue;
                var address = new StudentAddress
                {
                    AddressName = addressName,
                };
                var course = new StudentCourse
                {
                    Title = courseTitle
                };
                _Context.StudentAddresss.Add(address);
                _Context.StudentCourses.Add(course);
                await _Context.SaveChangesAsync();
                var student = new StudentModel
                {
                    Name = name,
                    Age = age,
                    Gender = gender,
                    AddressId = address.AddressId,
                    Courses = course
                };
                studentsToAdd.Add(student);
            }
            await _Context.Students.AddRangeAsync(studentsToAdd);
            await _Context.SaveChangesAsync();

            return studentsToAdd.Count;
        }

        // for the Filter 
        private IQueryable<StudentModel> ApplyStudentFilters(IQueryable<StudentModel> query, StudentFilter studentFilter)
        {
            if (!string.IsNullOrWhiteSpace(studentFilter.Name))
            {
                query = query.Where(s => s.Name.Contains(studentFilter.Name));
            }

            if (studentFilter.Age.HasValue)
            {
                query = query.Where(s => s.Age == studentFilter.Age.Value);
            }

            if (!string.IsNullOrWhiteSpace(studentFilter.Gender))
            {
                var genderNormalized = studentFilter.Gender.ToLower();

                if (genderNormalized == "f") genderNormalized = "female";
                else if (genderNormalized == "m") genderNormalized = "male";

                query = query.Where(s => s.Gender != null && s.Gender.ToLower() == genderNormalized);
            }

            //if (!string.IsNullOrWhiteSpace(studentFilter.Search))
            //{
            //    var keyword = studentFilter.Search.ToLower();
            //    query = query.Where(s =>
            //        (!string.IsNullOrEmpty(s.Name) && s.Name.ToLower().Contains(keyword)) ||
            //        s.Age.ToString().Contains(keyword) ||
            //        (!string.IsNullOrEmpty(s.Gender) && s.Gender.ToLower().Contains(keyword)) ||
            //        s.RollNumber.ToString().Contains(keyword)
            //    );
            //}
            // for search Box 
            if (!string.IsNullOrWhiteSpace(studentFilter.Search))
            {
                var keyword = studentFilter.Search.ToLower();
                query = query.Where(s =>
                    (!string.IsNullOrEmpty(s.Name) &&
                        (s.Name.ToLower().StartsWith(keyword) || s.Name.ToLower().EndsWith(keyword))) ||

                    (!string.IsNullOrEmpty(s.Gender) &&
                        (s.Gender.ToLower().StartsWith(keyword) || s.Gender.ToLower().EndsWith(keyword))) ||
                    s.Age.ToString().Contains(keyword) || s.RollNumber.ToString().Contains(keyword)
                );
            }
            return query;
        }
        // Filter + Paginated
        public async Task<PagedResultDto<StudentRTos>> GetAllFilterData([FromQuery] StudentFilter filter, [FromQuery] PaginationDto pagination)
        {
            var query = _Context.Students
                .Include(s => s.Address)
                .Include(s => s.Courses)
                .AsNoTracking()
                .AsQueryable();

            query = ApplyStudentFilters(query, filter);
            var totalRecords = await query.CountAsync();
            var students = await query
                .OrderBy(s => s.Id)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();
            var studentRtos = students.Select(x => new StudentRTos
            {
                Id = x.Id,
                Name = x.Name,
                //Age = x.Age,
                Gender = x.Gender,
                //RollNumber = x.RollNumber,
                Description = x.Description,
                AddressName = x.Address != null ? x.Address.AddressName : "N/A",
                Titles = x.Courses != null ? x.Courses.Title : "N/A"
            }).ToList();

            return new PagedResultDto<StudentRTos>
            {
                Data = studentRtos,
                TotalRecords = totalRecords,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }

        //public List<StudentRTos> EncryptStudents(List<StudentRTos> students)
        //{
        //    var encryptedList = new List<StudentRTos>();

        //    foreach (var student in students)
        //    {
        //        var encryptedStudent = new StudentRTos
        //        {
        //            Id = student.Id,
        //            Name = _encryptionService.Encrypt(student.Name),
        //            //Age = student.Age,
        //            Gender = student.Gender != null ? _encryptionService.Encrypt(student.Gender) : null,
        //            //RollNumber = student.RollNumber,
        //            Description = student.Description != null ? _encryptionService.Encrypt(student.Description) : null,
        //            AddressName = _encryptionService.Encrypt(student.AddressName),
        //            Titles = student.Titles != null ? _encryptionService.Encrypt(student.Titles) : null
        //        };

        //        encryptedList.Add(encryptedStudent);
        //    }

        //    return encryptedList;
        //}

        //public string EncryptAllStudents(List<StudentRTos> students)
        //{
        //    var encryptedList = new List<StudentRTos>();

        //    foreach (var student in students)
        //    {
        //        string json = JsonSerializer.Serialize(students);

        //        var encryptedStudent = new StudentRTos
        //        {
        //            Id = student.Id,
        //            Name = _encryptionService.Encrypt(student.Name),
        //            //Age = student.Age,
        //            Gender = student.Gender != null ? _encryptionService.Encrypt(student.Gender) : null,
        //            //RollNumber = student.RollNumber,
        //            Description = student.Description != null ? _encryptionService.Encrypt(student.Description) : null,
        //            AddressName = _encryptionService.Encrypt(student.AddressName),
        //            Titles = student.Titles != null ? _encryptionService.Encrypt(student.Titles) : null
        //        };
        //        string encrypted = _encryptionService.Encrypt(json);

        //        encryptedList.Add(encryptedStudent);
        //    }

        //    return encryptedList;
        //}

        public string EncryptStudents(List<StudentRTos> students)
        {
            var encryptedList = new List<StudentRTos>();

            foreach (var student in students)
            {
                var encryptedStudent = new StudentRTos
                {
                    Id = student.Id,
                    Name = _encryptionService.Encrypt(student.Name),
                    //Age = _encryptionService.Encrypt(student.Age.ToString()),
                    Gender = student.Gender != null ? _encryptionService.Encrypt(student.Gender) : null,
                    //RollNumber = _encryptionService.Encrypt(student.RollNumber.ToString()),
                    Description = student.Description != null ? _encryptionService.Encrypt(student.Description) : null,
                    AddressName = _encryptionService.Encrypt(student.AddressName),
                    Titles = student.Titles != null ? _encryptionService.Encrypt(student.Titles) : null
                };

                encryptedList.Add(encryptedStudent);
            }

            string jsonString = System.Text.Json.JsonSerializer.Serialize(encryptedList);
            string finalEncryptedString = _encryptionService.Encrypt(jsonString);
            return finalEncryptedString;
        }

        public List<StudentRTos> DecryptStudents(string encryptedData)
        {
            string decryptedJson = _encryptionService.Decrypt(encryptedData);
            var encryptedList = System.Text.Json.JsonSerializer.Deserialize<List<StudentRTos>>(decryptedJson);

            var decryptedList = new List<StudentRTos>();
            foreach (var student in encryptedList)
            {
                var decryptedStudent = new StudentRTos
                {
                    Id = student.Id,
                    Name = _encryptionService.Decrypt(student.Name),
                    Gender = student.Gender != null ? _encryptionService.Decrypt(student.Gender) : null,
                    Description = student.Description != null ? _encryptionService.Decrypt(student.Description) : null,
                    AddressName = _encryptionService.Decrypt(student.AddressName),
                    Titles = student.Titles != null ? _encryptionService.Decrypt(student.Titles) : null
                };

                decryptedList.Add(decryptedStudent);
            }

            return decryptedList;
        }
    }
}

