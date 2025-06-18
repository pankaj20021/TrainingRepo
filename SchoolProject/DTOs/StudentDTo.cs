namespace SchoolProject.DTOs
{
    public class StudentDTo
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public int RollNumber { get; set; }
        public string? Description { get; set; }
        public string AddressName { get; set; }
        public string Title { get; set; }
    }
    public enum Gender
    {
        Male,    
        Female,  
        Other    
    }

}
