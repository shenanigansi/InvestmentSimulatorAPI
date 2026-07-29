namespace InvestmentSimulatorAPI.Interfaces
{
    public interface IBaseRepository<T>
    {
        Task Create(T entity);
        Task Delete(T entity);
        Task Update(T entity);
        IQueryable<T> GetAll();
    }
}