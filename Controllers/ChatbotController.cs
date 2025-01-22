using Microsoft.AspNetCore.Mvc;
using PetAlert.Services;
using PetAlert.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetAlert.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly ChatbotService _chatbotService;

        public ChatbotController(ChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromForm] ChatbotViewModel vm)
        {
            if (!string.IsNullOrWhiteSpace(vm.UserMessage))
            {
                var response = await _chatbotService.GetChatbotResponseAsync(vm.UserMessage);
                return Content(response, "text/html"); // Return as HTML response
            }
            return Content("❌ Invalid request", "text/html");
        }
    }
}
