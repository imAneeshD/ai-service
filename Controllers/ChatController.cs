using ai_service.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ai_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IAIService _aiService;

        public ChatController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var response = await _aiService.GenerateResponse("Hello Gemini");

            return Ok(response);
        }
    }
}
