namespace InvestmentSimulatorAPI.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Неверное имя пользователя или пароль")
        : base(message)
    {
    }
}