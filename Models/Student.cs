namespace SchoolManagement.Models
{
    public class Student
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }
        public string FullName { get; set; }
        public string StudentCode { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
