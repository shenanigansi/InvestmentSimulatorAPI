namespace InvestmentSimulatorAPI.Models
{
    public class RegisterDtoModel
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
    }
}