using ConsoleAppDb.Domain;
using Microsoft.EntityFrameworkCore;

namespace ConsoleAppDb.DAL;

public class AppDbContext : DbContext
{
    public DbSet<Course> Courses { get; set; } = default!;
    public DbSet<Homework> Homeworks { get; set; } = default!;
    public DbSet<Person> Persons { get; set; } = default!;
    public DbSet<PersonCourse> PersonCourses { get; set; } = default!;
    public DbSet<PersonCourseHomework> PersonCourseHomeworks { get; set; } = default!;
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
    
}