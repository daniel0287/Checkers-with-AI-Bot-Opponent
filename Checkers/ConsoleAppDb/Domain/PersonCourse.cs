namespace ConsoleAppDb.Domain;

public class PersonCourse
{
    public int Id { get; set; }

    public bool IsStudent { get; set; } = true;
    
    public int PersonId { get; set; }
    public Person? Person { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }
    
    public double? HomeworkAverageGrade { get; set; }
    
    public double? FinalGrade { get; set; }
}