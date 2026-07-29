using InvestmentSimulatorAPI.Interfaces;
using InvestmentSimulatorAPI.Models.Database;
using Serilog;

namespace InvestmentSimulatorAPI.Services
{
    public class TransactionService : BaseServiceDb<TransactionModel>
    {
        public TransactionService(IBaseRepository<TransactionModel> repository) 
            : base(repository, Log.Logger.ForContext<TransactionService>()) { }
    }
}