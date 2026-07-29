using InvestmentSimulatorAPI.Exceptions;
using InvestmentSimulatorAPI.Interfaces;
using InvestmentSimulatorAPI.Models.Database;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace InvestmentSimulatorAPI.Services;

/// <summary>
/// Базовый сервис для всех сервисов, кроме Auth
/// </summary>
/// <typeparam name="T">IUserProduct</typeparam>
public class BaseServiceDb<T> where T : class, IUserProduct 
{
    protected readonly IBaseRepository<T> _repository;
    protected readonly ILogger _logger;
    
    protected BaseServiceDb(IBaseRepository<T> repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    /// <summary>
    /// Получение сущности по Id
    /// </summary>
    /// <param name="id">сущность id</param>
    /// <param name="userId">пользователь id</param>
    /// <returns></returns>
    public async Task<T> GetEntityByIdAsync(string id, int userId)
    {
        ValidateId(id);
        
        T? foundEntity = await _repository.GetAll().SingleOrDefaultAsync
            (f => f.Id.ToString() == id);
        
        foundEntity = ValidateEntity(foundEntity, id);

        ValidateEntityIdAndUserId(userId, foundEntity);
        
        _logger.Information($"User: {userId}. Сущность {typeof(T).Name} под Id {id} получена");
        
        return foundEntity;
    }
    
    /// <summary>
    /// Удаление сущности по Id
    /// </summary>
    /// <param name="id">сущность id</param>
    /// <param name="userId">пользователь id</param>
    public async Task DeleteEntityAsync(string id, int userId)
    {
        ValidateId(id);
            
        T? foundEntity = await _repository.GetAll().SingleOrDefaultAsync
            (f => f.Id.ToString() == id);

        foundEntity = ValidateEntity(foundEntity, id);
        
        ValidateEntityIdAndUserId(userId, foundEntity);
        
        await _repository.Delete(foundEntity);
        
        _logger.Information($"User: {userId}. Сущность {typeof(T).Name} под Id {id} удалена");
    }
    
    /// <summary>
    /// Удаление сущности
    /// </summary>
    /// <param name="entity">сущность</param>
    /// <param name="userId">пользователь id</param>
    public async Task DeleteEntityAsync(T entity, int userId)
    {   
        ValidateEntityIdAndUserId(userId, entity);
        await _repository.Delete(entity);
        _logger.Information($"User: {userId}. Сущность {typeof(T).Name} под Id {entity.Id} удалена");
    }
    
    /// <summary>
    /// Создание сущности
    /// </summary>
    /// <param name="entity">сущность</param>
    /// <param name="userId">пользователь id</param>
    public async Task CreateEntityAsync(T entity, int userId)
    {
        ValidateEntityIdAndUserId(userId, entity);
        await _repository.Create(entity);
        _logger.Information($"User: {userId}. Сущность {typeof(T).Name} под Id {entity.Id} создана");
    }
    
    /// <summary>
    /// Получение списка всех сущностей по пользователю
    /// </summary>
    /// <param name="userId">пользователь id</param>
    /// <returns></returns>
    public Task<List<T>> GetAll(int userId)
    {
        _logger.Information($"Список {typeof(T).Name} для пользователя {userId} получен");
        return _repository.GetAll().Where(x => x.UserId == userId).ToListAsync();
    }
    
    /// <summary>
    /// Получение списка всех сущностей
    /// </summary>
    /// <returns></returns>
    public Task<List<T>> GetAll()
    {
        _logger.Information($"Список {typeof(T).Name} получен");
        return _repository.GetAll().ToListAsync();
    }
    
    /// <summary>
    /// Валидация id
    /// </summary>
    /// <param name="id">сущность id</param>
    /// <exception cref="ArgumentException"></exception>
    private void ValidateId(string id)
    {
        if(string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Идентификатор не может быть пустым.", nameof(id));
    }
    
    /// <summary>
    /// Валидация сущности
    /// </summary>
    /// <param name="entity">сущность</param>
    /// <param name="id">сущность id</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    private T ValidateEntity(T? entity, string? id = default)
    {
        if (entity == null)
            throw new KeyNotFoundException($"Сущность {typeof(T).Name} с ID {id} не найдена");

        return entity;
    }
    
    /// <summary>
    /// Валидация сущности и пользователя
    /// </summary>
    /// <param name="userId">пользователь id</param>
    /// <param name="entity">сущность</param>
    /// <exception cref="ForbiddenException"></exception>
    private void ValidateEntityIdAndUserId(int userId, T entity)
    {
        if(entity.UserId != userId)
            throw new ForbiddenException($"Id сущности {typeof(T).Name} не совпадает с пользовательским id");
    }
}