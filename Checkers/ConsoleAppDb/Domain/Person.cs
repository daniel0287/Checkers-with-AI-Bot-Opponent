using System.ComponentModel.DataAnnotations;

namespace ConsoleAppDb.Domain;

public class Person
{
    public int Id { get; set; }

    [MaxLength(64)] public string FirstName { get; set; } = default!;
    [MaxLength(64)] public string LastName { get; set; } = default!;

    public ICollection<PersonCourse>? PersonCourses { get; set; }

    public override string ToString() => Id + " - " + FirstName + " " + LastName;
    
}