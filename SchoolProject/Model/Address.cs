using System.ComponentModel.DataAnnotations;

namespace SchoolProject.Model
{
    public class StudentAddress
    {
        [Key]
        public int AddressId { get; set; }
        public string? AddressName { get; set;}
      
       
    }
}
