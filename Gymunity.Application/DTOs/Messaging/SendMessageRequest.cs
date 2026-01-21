using Gymunity.Domain.Enums;
using System.ComponentModel;

namespace Gymunity.Application.DTOs.Messaging
{
    public class SendMessageRequest
    {
        public string Content { get; set; } = null!;
        public string? MediaUrl { get; set; }
        //[Description("type of the message: 1 = text")]
        [Description("type of the message: 1 = text, 2 = image, 3 = video, 4 = audio, 5 = file")]
        public MessageType Type { get; set; } = MessageType.Text;
    }
}