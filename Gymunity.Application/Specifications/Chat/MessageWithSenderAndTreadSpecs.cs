using Gymunity.Domain.Entities.Messaging;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.Chat
{
    internal class MessageWithSenderAndTreadSpecs : BaseSpecification<Message>
    {
        public MessageWithSenderAndTreadSpecs(string senderId) : base(m => m.SenderId == senderId)
        {
            AddInclude(m => m.Sender);
            AddInclude(m => m.Thread);
        }
    }
}