using InvestmentSimulatorAPI.Interfaces;
using InvestmentSimulatorAPI.Models.Database;
using Serilog;

namespace InvestmentSimulatorAPI.Services
{
    public class FavouriteService : BaseServiceDb<FavouritesModel>
    {
        public FavouriteService(IBaseRepository<FavouritesModel> repository) 
            : base(repository, Log.Logger.ForContext<FavouriteService>()) { }
    }
}