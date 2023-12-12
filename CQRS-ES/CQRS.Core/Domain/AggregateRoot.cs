using CQRS.Core.Events;

namespace CQRS.Core.Domain
{
    public abstract class AggregateRoot
    {
        protected Guid id;
        private readonly List<BaseEvent> changes = new();

        public Guid Id => this.id;

        public int Version { get; set; } = -1;

        public IEnumerable<BaseEvent> GetUncommittedChanges()
        {
            return this.changes;
        }

        public void MarkChangesAsCommitted()
        {
            this.changes.Clear();
        }

        private void ApplyChange(BaseEvent @event, bool isNew)
        {
            System.Reflection.MethodInfo? method = this.GetType().GetMethod("Apply", new Type[] { @event.GetType() });

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method), $"The Apply method was not found in the aggregate for {@event.GetType().Name}!");
            }

            method.Invoke(this, new object[] { @event });

            if (isNew)
            {
                this.changes.Add(@event);
            }
        }

        protected void RaiseEvent(BaseEvent @event)
        {
            this.ApplyChange(@event, true);
        }

        public void ReplayEvents(IEnumerable<BaseEvent> events)
        {
            foreach (BaseEvent @event in events)
            {
                this.ApplyChange(@event, false);
            }
        }
    }
}
