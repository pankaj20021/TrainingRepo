using System.ComponentModel.DataAnnotations;

namespace SchoolProject.Model
{
    public class StudentCourse
    {
        [Key]
        public int CourseId { get; set; }
        public string? Title { get; set; }
       

        // many to many 
        public ICollection<StudentModel> Students { get; set; }

       
    }
}
