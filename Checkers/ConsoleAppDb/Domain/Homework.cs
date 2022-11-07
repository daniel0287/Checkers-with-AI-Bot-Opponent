using System.ComponentModel.DataAnnotations;

namespace ConsoleAppDb.Domain;

public class Homework
{
    public int Id { get; set; }

    [MaxLength(512)]
    public string Description { get; set; } = default!;

    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public double? AverageGrade { get; set; }
}