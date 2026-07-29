using InvestmentSimulatorAPI.Interfaces;
using InvestmentSimulatorAPI.Models;
using InvestmentSimulatorAPI.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace InvestmentSimulatorAPI.Repositories
{
    public class AuthRepository : IBaseRepository<UserModel>
    {
        private readonly ApplicationDbContext _context;
        private IBaseRepository<UserModel> _baseRepositoryImplementation;

        public AuthRepository(ApplicationDbContext context) => _context = context;

        public async Task Create(UserModel entity)
        {
            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(UserModel entity)
        {
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(UserModel entity)
        {
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<UserModel> GetAll()
        {
            return _context.Users;
        }

        public async Task<UserModel?> GetUserById(int userId)
        {
            return await _context.Users
                .Include(u => u.Transactions) 
                .Include(u => u.Portfolios)
                .Include(u => u.Favourites)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}