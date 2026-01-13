using SchoolManagement.Models;

namespace SchoolManagement.Data
{
    public static class DbSeeder
    {
        public static void Seed(SchoolDbContext context)
        {
            if (!context.Schools.Any())
            {
                var schools = new List<School>();

                for (int i = 1; i <= 10; i++)
                {
                    schools.Add(new School
                    {
                        Name = $"School {i}",
                        Principal = $"Principal {i}",
                        Address = $"Address {i}"
                    });
                }

                context.Schools.AddRange(schools);
                context.SaveChanges();
            }
        }
    }
}
