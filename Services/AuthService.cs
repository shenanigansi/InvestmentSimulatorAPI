using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InvestmentSimulatorAPI.Exceptions;
using InvestmentSimulatorAPI.Interfaces;
using InvestmentSimulatorAPI.Models;
using InvestmentSimulatorAPI.Models.Database;
using InvestmentSimulatorAPI.Models.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ILogger = Serilog.ILogger;

namespace InvestmentSimulatorAPI.Services
{
    public class AuthService
    {
        private readonly ILogger _logger;
        private readonly IBaseRepository<UserModel> _repository;
        private const int SaltSize = 16; 
        private const int HashSize = 20; 
        private const int Iterations = 10000;

        public AuthService(IBaseRepository<UserModel> repository)
        {
            _logger = Log.ForContext<AuthService>();
            _repository = repository;
        }

        /// <summary>
        /// Получение пользователя по Id
        /// </summary>
        /// <param name="id">пользователь id</param>
        /// <returns></returns>
        public async Task<UserDtoModel> GetUserById(int id)
        {
            var user = await _repository.GetAll()
                .Include(u => u.Transactions)
                .Include(u => u.Portfolios)
                .Include(u => u.Favourites)
                .FirstOrDefaultAsync(u => u.Id == id);

            user = ValidateEntity(user, id.ToString());

            UserDtoModel dto = new()
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Timestamp = user.Timestamp,
                IsAdmin = user.IsAdmin,
                Transactions = user.Transactions?.Select(t => new TransactionDtoModel
                {
                    Symbol = t.Symbol,
                    Type = t.Type,
                    Quantity = t.Quantity,
                    Price = t.Price,
                    Timestamp = t.Timestamp
                }).ToList(),
                Portfolios = user.Portfolios?.Select(p => new PortfolioDtoModel
                {
                    Symbol = p.Symbol,
                    Quantity = p.Quantity
                }).ToList(),
                Favourites = user.Favourites?.Select(f => new FavouriteDtoModel
                {
                    Symbol = f.Symbol
                }).ToList()
            };

            return dto;
        }

        /// <summary>
        /// Создание пользователя
        /// </summary>
        /// <param name="user">пользователь</param>
        public async Task Create(UserModel user)
        {
            await _repository.Create(user);
            _logger.Information($"Пользователь {user.Username} создан");
        }
        
        /// <summary>
        /// Удаление пользователя
        /// </summary>
        /// <param name="user">пользователь</param>
        public async Task Delete(UserModel user)
        {
            await _repository.Delete(user);
            _logger.Information($"Пользователь {user.Username} удален");
        }
        
        /// <summary>
        /// Удаление пользователя по id
        /// </summary>
        /// <param name="id">пользователь id</param>
        public async Task Delete(string id)
        {
            ValidateId(id);
            var foundEntity = _repository.GetAll().SingleOrDefaultAsync(f => f.Id.ToString() == id);
            var result = ValidateEntity(foundEntity.Result, id);
            await _repository.Create(result);
            _logger.Information($"Пользователь {result.Username} удален");
        }
        
        /// <summary>
        /// Валидация Id
        /// </summary>
        /// <param name="id">пользователь id</param>
        /// <exception cref="ArgumentException"></exception>
        private void ValidateId(string id)
        {
            if(string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
                throw new ArgumentException($"Идентификатор не может быть пустым.", nameof(id));
        }
        
        /// <summary>
        /// Валидация сущности пользователя
        /// </summary>
        /// <param name="entity">пользователь</param>
        /// <param name="id">пользователь id</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        private UserModel ValidateEntity(UserModel? entity, string id)
        {
            if (entity == null)
                throw new KeyNotFoundException($"Сущность UserModel с ID {id} не найдена");

            return entity;
        }
        
        /// <summary>
        /// Хэширование пароля
        /// </summary>
        /// <param name="password">пароль</param>
        /// <returns></returns>
        public string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Проверка на дубликат пользователя
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <exception cref="ForbiddenException"></exception>
        public async Task InspectDuplicates(string username, string email)
        {
            if (await _repository.GetAll().AnyAsync(f => f.Username == username || f.Email == email))
            {
                throw new ForbiddenException("Пользователь уже существует");
            }
        }

        /// <summary>
        /// Верификация пользователя
        /// </summary>
        /// <param name="dtoModel">dto пользователь</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="UnauthorizedException"></exception>
        public async Task<UserModel> VerifyUser(LoginDtoModel dtoModel)
        {
            var foundUser = await _repository.GetAll().SingleOrDefaultAsync(f => f.Username == dtoModel.Username);

            if (foundUser == null)
                throw new KeyNotFoundException("Неверный логин или пароль");
            
            if(string.IsNullOrEmpty(dtoModel.Password) ||
               string.IsNullOrWhiteSpace(dtoModel.Password))
                throw new ArgumentException("Пароль пуст", nameof(dtoModel.Password));
            
            if(string.IsNullOrWhiteSpace(foundUser.PasswordHash) ||
                string.IsNullOrEmpty(foundUser.PasswordHash))
                throw new ArgumentException("Хэш пароля пуст", nameof(foundUser.PasswordHash));
            
            var parts = foundUser.PasswordHash.Split('.');

            if (parts.Length != 2)
                throw new UnauthorizedException();

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(dtoModel.Password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            var result = CryptographicOperations.FixedTimeEquals(computedHash, hash);

            if (!result)
                throw new UnauthorizedException();

            return foundUser;
        }
        
        /// <summary>
        /// Генерация JWT токена
        /// </summary>
        /// <param name="user">пользователь</param>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string GenerateJwtToken(UserModel user, IConfiguration config)
        {
            var result = config["Jwt:Key"];

            if (result is null || config["Jwt:Issuer"] == null 
                               || config["Jwt:Audience"] == null)
                throw new InvalidOperationException("Конфигурация JWT неверна");
                
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(result));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12), 
                signingCredentials: credentials
            );

            _logger.Information($"Успешный вход пользователя {user.Username} в учетную запись");
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}