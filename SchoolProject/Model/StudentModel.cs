using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Model
{
    public class StudentModel
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Gender { get; set; }
        public int RollNumber { get; set; }
        public string? Description { get; set; }


        [ForeignKey("AddressId")]
        public int AddressId { get; set; } 
        public StudentAddress? Address { get; set; }
        //many-to-many relation
        public StudentCourse Courses {get; set;}

    }
}
