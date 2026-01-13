using SchoolManagement.Data;
using SchoolManagement.Models;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        using var context = new SchoolDbContext();
        context.Database.EnsureCreated();
        SeedSchools(context);

        Console.WriteLine("=== SCHOOL MANAGEMENT SYSTEM ===");

        while (true)
        {
            Console.WriteLine("\n1. List students");
            Console.WriteLine("2. Create student");
            Console.WriteLine("3. Update student");
            Console.WriteLine("4. Delete student");
            Console.WriteLine("0. Exit");
            Console.Write("Choose an option: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        ListStudents(context);
                        break;
                    case "2":
                        CreateStudent(context);
                        break;
                    case "3":
                        UpdateStudent(context);
                        break;
                    case "4":
                        DeleteStudent(context);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    // ================= READ =================
    static void ListStudents(SchoolDbContext context)
    {
        const int pageSize = 10;
        int page = 1;

        while (true)
        {
            var students = context.Students
                .OrderBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (!students.Any())
            {
                Console.WriteLine("No students found.");
                return;
            }

            Console.WriteLine($"\n--- Students (Page {page}) ---");
            foreach (var s in students)
            {
                var schoolName = context.Schools
                    .Where(sc => sc.Id == s.SchoolId)
                    .Select(sc => sc.Name)
                    .FirstOrDefault();

                Console.WriteLine(
                    $"{s.Id}. {s.FullName} | {s.StudentCode} | {s.Email} | {s.Phone} | School: {schoolName}");
            }

            Console.WriteLine("\nN: Next | P: Previous | Q: Quit");
            var key = Console.ReadKey().KeyChar;
            Console.WriteLine();

            if (key == 'N' || key == 'n') page++;
            else if (key == 'P' || key == 'p') page = Math.Max(1, page - 1);
            else break;
        }
    }

    // ================= CREATE =================
    static void CreateStudent(SchoolDbContext context)
    {
        Console.WriteLine("\n--- Create Student ---");

        var student = new Student
        {
            FullName = ReadRequiredString("Full name", 2, 100),
            StudentCode = ReadUniqueStudentCode(context),
            Email = ReadUniqueEmail(context),
            Phone = ReadOptionalPhone(),
            SchoolId = ReadSchoolId(context)
        };

        context.Students.Add(student);
        context.SaveChanges();

        Console.WriteLine("Student created successfully.");
    }

    // ================= UPDATE =================
    static void UpdateStudent(SchoolDbContext context)
    {
        Console.Write("\nEnter student ID to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        var student = context.Students.Find(id);
        if (student == null)
        {
            Console.WriteLine("Student not found.");
            return;
        }

        Console.WriteLine("Leave blank to keep current value.");

        var fullName = ReadOptionalString("Full name", 2, 100);
        if (fullName != null) student.FullName = fullName;

        var email = ReadOptionalEmail(context, student.Id);
        if (email != null) student.Email = email;

        var phone = ReadOptionalPhone(true);
        if (phone != null) student.Phone = phone;

        var schoolId = ReadOptionalSchoolId(context);
        if (schoolId.HasValue) student.SchoolId = schoolId.Value;

        context.SaveChanges();
        Console.WriteLine("Student updated successfully.");
    }

    // ================= DELETE =================
    static void DeleteStudent(SchoolDbContext context)
    {
        Console.Write("\nEnter student ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        var student = context.Students.Find(id);
        if (student == null)
        {
            Console.WriteLine("Student not found.");
            return;
        }

        Console.Write($"Are you sure you want to delete {student.FullName}? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y") return;

        context.Students.Remove(student);
        context.SaveChanges();

        Console.WriteLine("Student deleted successfully.");
    }

    // ================= VALIDATION =================

    static string ReadRequiredString(string label, int min, int max)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                Console.WriteLine($"{label} is required.");
            else if (input.Length < min || input.Length > max)
                Console.WriteLine($"{label} must be between {min} and {max} characters.");
            else
                return input;
        }
    }

    static string ReadOptionalString(string label, int min, int max)
    {
        Console.Write($"{label}: ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input)) return null;

        if (input.Length < min || input.Length > max)
            throw new Exception($"{label} must be between {min} and {max} characters.");

        return input;
    }

    static string ReadUniqueStudentCode(SchoolDbContext context)
    {
        while (true)
        {
            var value = ReadRequiredString("Student ID", 5, 20);
            if (context.Students.Any(s => s.StudentCode == value))
                Console.WriteLine("Student ID already exists.");
            else
                return value;
        }
    }

    static string ReadUniqueEmail(SchoolDbContext context)
    {
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        while (true)
        {
            Console.Write("Email: ");
            var email = Console.ReadLine();

            if (!regex.IsMatch(email))
                Console.WriteLine("Invalid email format.");
            else if (context.Students.Any(s => s.Email == email))
                Console.WriteLine("Email already exists.");
            else
                return email;
        }
    }

    static string ReadOptionalEmail(SchoolDbContext context, int studentId)
    {
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        Console.Write("Email: ");
        var email = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(email)) return null;

        if (!regex.IsMatch(email))
            throw new Exception("Invalid email format.");

        if (context.Students.Any(s => s.Email == email && s.Id != studentId))
            throw new Exception("Email already exists.");

        return email;
    }

    static string ReadOptionalPhone(bool allowEmpty = false)
    {
        Console.Write("Phone: ");
        var phone = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(phone))
            return allowEmpty ? null : "";

        if (!Regex.IsMatch(phone, @"^\d{10,11}$"))
            throw new Exception("Phone number must be 10-11 digits.");

        return phone;
    }

    static int ReadSchoolId(SchoolDbContext context)
    {
        Console.WriteLine("\nAvailable Schools:");
        foreach (var s in context.Schools.OrderBy(s => s.Id))
        {
            Console.WriteLine($"{s.Id}. {s.Name}");
        }

        while (true)
        {
            Console.Write("Choose School ID: ");
            if (int.TryParse(Console.ReadLine(), out int id) &&
                context.Schools.Any(s => s.Id == id))
                return id;

            Console.WriteLine("School does not exist.");
        }
    }

    static int? ReadOptionalSchoolId(SchoolDbContext context)
    {
        Console.Write("School ID: ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) return null;

        if (int.TryParse(input, out int id) &&
            context.Schools.Any(s => s.Id == id))
            return id;

        throw new Exception("School does not exist.");
    }

    // ================= SEED =================
    static void SeedSchools(SchoolDbContext context)
    {
        if (context.Schools.Any()) return;

        var schools = new List<School>
        {
            new School { Name = "Greenwich High School", Principal = "John Smith", Address = "London" },
            new School { Name = "Oxford Academy", Principal = "Emma Brown", Address = "Oxford" },
            new School { Name = "Cambridge School", Principal = "David Wilson", Address = "Cambridge" },
            new School { Name = "Brighton College", Principal = "Sarah Taylor", Address = "Brighton" },
            new School { Name = "Manchester High", Principal = "Michael Lee", Address = "Manchester" },
            new School { Name = "Leeds Grammar School", Principal = "Laura White", Address = "Leeds" },
            new School { Name = "Bristol Academy", Principal = "James Hall", Address = "Bristol" },
            new School { Name = "Liverpool School", Principal = "Robert King", Address = "Liverpool" },
            new School { Name = "Nottingham College", Principal = "Anna Scott", Address = "Nottingham" },
            new School { Name = "York High School", Principal = "Daniel Green", Address = "York" }
        };

        context.Schools.AddRange(schools);
        context.SaveChanges();
    }
}
