using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Handlers
{
    public class EventSourcingHandler : IEventSourcingHandler<PostAggregate>
    {
        private readonly IEventStore eventStore;

        public EventSourcingHandler(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task<PostAggregate> GetByIdAsync(Guid aggregateId)
        {
            var aggregate = new PostAggregate();
            List<BaseEvent> events = await this.eventStore.GetEventsAsync(aggregateId);

            if (events == null || !events.Any())
            {
                return aggregate;
            }

            aggregate.ReplayEvents(events);
            aggregate.Version = events.Select(x => x.Version).Max();

            return aggregate;
        }

        public async Task SaveAsync(AggregateRoot aggregate)
        {
            await this.eventStore.SaveEventsAsync(aggregate.Id, aggregate.GetUncommittedChanges(), aggregate.Version);
            aggregate.MarkChangesAsCommitted();
        }
    }
}
