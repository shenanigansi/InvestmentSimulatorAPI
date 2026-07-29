using InvestmentSimulatorAPI.Models;
using InvestmentSimulatorAPI.Interfaces;
using InvestmentSimulatorAPI.Models.Database;

namespace InvestmentSimulatorAPI.Repositories
{
    public class PortfolioRepository : IBaseRepository<PortfolioModel>
    {
        private ApplicationDbContext _dbContext;

        public PortfolioRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task Create(PortfolioModel entity)
        {
            await _dbContext.Portfolio.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(PortfolioModel entity)
        {
            _dbContext.Portfolio.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<PortfolioModel> GetAll()
        {
            return _dbContext.Portfolio;
        }

        public async Task Update(PortfolioModel entity)
        {
            _dbContext.Portfolio.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}