namespace InvestmentSimulatorAPI.Models.ObjectResult;

internal record InvestmentResponse
{
    public bool Success { get; init; }
    public object? Data { get; init; }
    public string? Message { get; init; }

    public InvestmentResponse(bool success, object? data, string? message)
    {
        Success = success;
        Data = data;
        Message = message;
    }
    public InvestmentResponse() { }
}