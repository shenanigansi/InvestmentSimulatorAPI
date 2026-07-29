using InvestmentSimulatorAPI.Interfaces;
using InvestmentSimulatorAPI.Models.Database;
using InvestmentSimulatorAPI.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace InvestmentSimulatorAPI.Services;
public class PortfolioService : BaseServiceDb<PortfolioModel>
{
    public PortfolioService(IBaseRepository<PortfolioModel> repository) 
        : base(repository, Log.Logger.ForContext<PortfolioService>()) { }

    /// <summary>
    /// Пополнение портфолио
    /// </summary>
    /// <param name="entity">портфолио</param>
    /// <param name="funds">сумма</param>
    /// <exception cref="ArgumentException"></exception>
    public async Task AddFunds(PortfolioModel entity, int? funds)
    {
        if (funds < 1 || funds == null)
            throw new ArgumentException("Взнос funds должен быть больше 0", nameof(funds));

        if (entity.Quantity == null)
            entity.Quantity = funds;
        else
            entity.Quantity += funds;
        
        await _repository.Update(entity);
        
        _logger.Information($"На портфолио {entity.Id} успешно зачислено {funds}");
    }

    /// <summary>
    /// Получение портфолио по символу
    /// </summary>
    /// <param name="fundDto">fundDto</param>
    /// <param name="userId">пользователь id</param>
    /// <returns></returns>
    public async Task<PortfolioModel> GetUserPortfolioBySymbol(FundDtoModel fundDto, int userId)
    {
        var foundPortfolio = await _repository.GetAll().
            Where(p => p.UserId == userId && p.Symbol == fundDto.Symbol).FirstOrDefaultAsync();

        if (foundPortfolio == null)
        {
            foundPortfolio = new PortfolioModel()
            {
                Symbol = fundDto.Symbol,
                UserId = userId
            };
            
            await _repository.Create(foundPortfolio);
        }
        
        return foundPortfolio;
    }
}