using System.ComponentModel.DataAnnotations;

namespace ConsoleAppDb.Domain;

public class Course
{
    public int Id { get; set; }

    [MaxLength(64)]
    public string CourseName { get; set; } = default!;
    
    public ICollection<PersonCourse>? PersonCourses { get; set; }

    public ICollection<Homework>? Homeworks { get; set; }
}