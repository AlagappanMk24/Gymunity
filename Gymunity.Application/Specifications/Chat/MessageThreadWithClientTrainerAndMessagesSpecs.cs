using Gymunity.Domain.Entities.Messaging;
using Gymunity.Domain.Specification;

namespace Gymunity.Application.Specifications.Chat
{
    internal class MessageThreadWithClientTrainerAndMessagesSpecs : BaseSpecification<MessageThread>
    {
        public MessageThreadWithClientTrainerAndMessagesSpecs(int threadId) : base(mt => mt.Id == threadId)
        {
            AddInclude(mt => mt.Client);
            AddInclude(mt => mt.Trainer);
            AddInclude(mt => mt.Messages);
        }
    }
}