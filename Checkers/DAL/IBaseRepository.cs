namespace DAL;

public interface IBaseRepository
{
    String Name { get; }

    void SaveChanges();
}