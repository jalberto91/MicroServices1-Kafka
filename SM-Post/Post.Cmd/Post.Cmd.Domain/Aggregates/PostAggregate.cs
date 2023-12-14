using CQRS.Core.Domain;
using Post.Common.Events;

namespace Post.Cmd.Domain.Aggregates
{
    public class PostAggregate : AggregateRoot
    {
        private bool active;
        private string author;
        private readonly Dictionary<Guid, Tuple<string, string>> comments = new();

        public bool Active
        {
            get => this.active; set => this.active = value;
        }

        public PostAggregate()
        {
        }

        public PostAggregate(Guid id, string author, string message)
        {
            this.RaiseEvent(new PostCreatedEvent
            {
                Id = id,
                Author = author,
                Message = message,
                DatePosted = DateTime.Now
            });
        }

        public void EditMessage(string message)
        {
            if (!this.active)
            {
                throw new InvalidOperationException("You cannot edit the message of an inactive post!");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new InvalidOperationException($"The value of {nameof(message)} cannot be null or empty. Please provide a valid {nameof(message)}!");
            }

            this.RaiseEvent(new MessageUpdatedEvent
            {
                Id = this.id,
                Message = message
            });
        }

        public void LikePost()
        {
            if (!this.active)
            {
                throw new InvalidOperationException("You cannot like an inactive post!");
            }

            this.RaiseEvent(new PostLikedEvent
            {
                Id = this.id
            });
        }

        public void AddComment(string comment, string username)
        {
            if (!this.active)
            {
                throw new InvalidOperationException("You cannot add a comment to an inactive post!");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new InvalidOperationException($"The value of {nameof(comment)} cannot be null or empty. Please provide a valid {nameof(comment)}!");
            }

            this.RaiseEvent(new CommentAddedEvent
            {
                Id = this.id,
                CommentId = Guid.NewGuid(),
                Comment = comment,
                Username = username,
                CommentDate = DateTime.Now
            });
        }

        public void EditComment(Guid commentId, string comment, string username)
        {
            if (!this.active)
            {
                throw new InvalidOperationException("You cannot edit a comment of an inactive post!");
            }

            if (!this.comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("You are not allowed to edit a comment that was made by another user!");
            }

            this.RaiseEvent(new CommentUpdatedEvent
            {
                Id = this.id,
                CommentId = commentId,
                Comment = comment,
                Username = username,
                EditDate = DateTime.Now
            });
        }

        public void RemoveComment(Guid commentId, string username)
        {
            if (!this.active)
            {
                throw new InvalidOperationException("You cannot remove a comment of an inactive post!");
            }

            if (!this.comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("You are not allowed to remove a comment that was made by another user!");
            }

            this.RaiseEvent(new CommentRemovedEvent
            {
                Id = this.id,
                CommentId = commentId
            });
        }

        public void DeletePost(string username)
        {
            if (!this.active)
            {
                throw new InvalidOperationException("The post has already been removed!");
            }

            if (!this.author.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("You are not allowed to delete a post that was made by someone else!");
            }

            this.RaiseEvent(new PostRemovedEvent
            {
                Id = this.id
            });
        }

        public void Apply(MessageUpdatedEvent @event)
        {
            this.id = @event.Id;
        }

        public void Apply(PostLikedEvent @event)
        {
            this.id = @event.Id;
        }

        public void Apply(CommentAddedEvent @event)
        {
            this.id = @event.Id;
            this.comments.Add(@event.CommentId, new Tuple<string, string>(@event.Comment, @event.Username));
        }

        public void Apply(CommentUpdatedEvent @event)
        {
            this.id = @event.Id;
            this.comments[@event.CommentId] = new Tuple<string, string>(@event.Comment, @event.Username);
        }

        public void Apply(CommentRemovedEvent @event)
        {
            this.id = @event.Id;
            this.comments.Remove(@event.CommentId);
        }

        public void Apply(PostCreatedEvent @event)
        {
            this.id = @event.Id;
            this.active = true;
            this.author = @event.Author;
        }

        public void Apply(PostRemovedEvent @event)
        {
            this.id = @event.Id;
            this.active = false;
        }
    }
}
