using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Stores
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreRepository eventStoreRepository;

        public EventStore(IEventStoreRepository eventStoreRepository)
        {
            this.eventStoreRepository = eventStoreRepository;
        }

        public async Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId)
        {
            List<EventModel> eventStream = await this.eventStoreRepository.FindByAggregateId(aggregateId);

            if (eventStream == null || !eventStream.Any())
            {
                throw new AggregateNotFoundException("Incorrect post ID provided!");
            }

            return eventStream.OrderBy(x => x.Version).Select(x => x.EventData).ToList();
        }

        public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)
        {
            List<EventModel> eventStream = await this.eventStoreRepository.FindByAggregateId(aggregateId);

            if (expectedVersion != -1 && eventStream[^1].Version != expectedVersion)
            {
                throw new ConcurrencyException();
            }

            int version = expectedVersion;

            foreach (BaseEvent @event in events)
            {
                version++;
                @event.Version = version;
                string eventType = @event.GetType().Name;
                var eventModel = new EventModel
                {
                    TimeStamp = DateTime.Now,
                    AggregateIdentifier = aggregateId,
                    AggregateType = nameof(PostAggregate),
                    Version = version,
                    EventType = eventType,
                    EventData = @event
                };

                await this.eventStoreRepository.SaveAsync(eventModel);
            }
        }
    }
}
