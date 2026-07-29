using InvestmentSimulatorAPI.Models.Database;
using InvestmentSimulatorAPI.Interfaces;
using InvestmentSimulatorAPI.Models;
using InvestmentSimulatorAPI.Models.DTO;

namespace InvestmentSimulatorAPI.Repositories
{
    public class FavouriteRepository : IBaseRepository<FavouritesModel>
    {
        private ApplicationDbContext _dbContext;
        public FavouriteRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task Create(FavouritesModel entity)
        {
            await _dbContext.Favourites.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(FavouritesModel entity)
        {
            _dbContext.Favourites.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(FavouritesModel entity)
        {
            _dbContext.Favourites.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<FavouritesModel> GetAll()
        {
            return _dbContext.Favourites;
        }
    }
}