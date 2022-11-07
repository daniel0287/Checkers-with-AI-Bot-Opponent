namespace ConsoleAppDb.Domain;

public class PersonCourseHomework
{
    public int Id { get; set; }

    public int PersonCourseId { get; set; }
    public PersonCourse? PersonCourse { get; set; }
        
    public int HomeWorkId { get; set; }
    public Homework? Homework { get; set; }
    
    public double Grade { get; set; }
}