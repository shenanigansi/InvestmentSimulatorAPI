using InvestmentSimulatorAPI.Models;
using InvestmentSimulatorAPI.Interfaces;
using InvestmentSimulatorAPI.Models.Database;

namespace InvestmentSimulatorAPI.Repositories
{
    public class TransactionRepository : IBaseRepository<TransactionModel>
    {
        private ApplicationDbContext _dbContext;

        public TransactionRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task Create(TransactionModel entity)
        {
            await _dbContext.Transactions.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(TransactionModel entity)
        {
            _dbContext.Transactions.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(TransactionModel entity)
        {
            _dbContext.Transactions.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<TransactionModel> GetAll()
        {
            return _dbContext.Transactions;
        }
    }
}