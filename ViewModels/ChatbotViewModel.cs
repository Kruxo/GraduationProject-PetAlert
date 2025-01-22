using System.Collections.Generic;

namespace PetAlert.ViewModels
{
    public class ChatbotViewModel
    {
        public string UserMessage { get; set; } = "";  // Ensure it is explicitly a string
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    public class ChatMessage
    {
        public string Text { get; set; } = "";
        public bool IsUser { get; set; }
    }
}
