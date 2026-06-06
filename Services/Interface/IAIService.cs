namespace ai_service.Services.Interface
{
    public interface IAIService
    {
        Task<string> GenerateResponse(string prompt);
    }
}
