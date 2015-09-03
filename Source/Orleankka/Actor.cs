using System;
using System.Threading.Tasks;

using Orleankka.Core;
using Orleankka.Meta;
using Orleankka.Services;
using Orleankka.Utility;

using Orleans;

namespace Orleankka
{
    public interface IActor
    {}

    public abstract class Actor : IActor
    {
        ActorRef self;

        protected Actor()
        {}

        protected Actor(string id, IActorRuntime runtime)
        {
            Requires.NotNull(runtime, nameof(runtime));
            Requires.NotNullOrWhitespace(id, nameof(id));

            Id = id;
            Runtime = runtime;
        }

        internal void Initialize(string id, IActorRuntime runtime, ActorPrototype prototype)
        {
            Id = id;
            Runtime = runtime;
            Prototype = prototype;
        }

        protected string Id
        {
            get; private set;
        }

        private IActorRuntime Runtime
        {
            get; set;
        }

        internal ActorPrototype Prototype
        {
            get; set;
        }

        protected IActorSystem System           => Runtime.System;
        protected IActivationService Activation => Runtime.Activation;
        protected IReminderService Reminders    => Runtime.Reminders;
        protected ITimerService Timers          => Runtime.Timers;

        protected ActorRef Self
        {
            get
            {
                if (self == null)
                {
                    var path = ActorPath.From(GetType(), Id);
                    self = System.ActorOf(path);
                }

                return self;
            }
        }

        protected internal virtual void Define()
        {}

        protected internal virtual Task OnActivate()
        {
            return TaskDone.Done;
        }

        protected internal virtual Task OnDeactivate()
        {
            return TaskDone.Done;
        }

        protected internal virtual Task OnReminder(string id)
        {
            var message = $"Override {"OnReminder"}() method in class {GetType()} to implement corresponding behavior";

            throw new NotImplementedException(message);
        }

        protected internal virtual Task<object> OnReceive(object message)
        {
            return DispatchAsync(message);
        }

        protected void Dispatch(object message)
        {
            Requires.NotNull(message, nameof(message));
            Prototype.Dispatch(this, message);
        }

        protected TResult DispatchResult<TResult>(object message)
        {
            return (TResult)DispatchResult(message);
        }

        protected object DispatchResult(object message)
        {
            Requires.NotNull(message, nameof(message));
            return Prototype.DispatchResult(this, message);
        }

        protected async Task<TResult> DispatchAsync<TResult>(object message)
        {
            return (TResult)await DispatchAsync(message);
        }

        protected Task<object> DispatchAsync(object message)
        {
            Requires.NotNull(message, nameof(message));
            return Prototype.DispatchAsync(this, message);
        }

        protected void On<TRequest, TResult>(Func<TRequest, TResult> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TResult>(Func<Query<TResult>, TResult> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest, TResult>(Func<TRequest, Task<TResult>> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TResult>(Func<Query<TResult>, Task<TResult>> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest>(Action<TRequest> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest>(Func<TRequest, Task> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }
    }
}