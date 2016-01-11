using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace PremioTek.Mibus
{
    /// <summary>
    /// Asynchronous Interactor:
    /// used to mediate async  command/response message between a <see cref="IQuery"/> and its <see cref="IQueryHandler"/>
    /// </summary>
    public interface IMediator
    {
        TResult Execute<TResult>(IQuery<TResult> request);
        void Execute(ICommand request);
        void Raise(IEvent notification);

        Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> request);
        Task ExecuteAsync(ICommand command);
        Task RaiseAsync(IEvent _event);
    }

    /// <summary>
    /// Object Communication Mediator:
    /// an instance of <see cref="Mediator"/> allows objects to talk synchronously or asynchronously to each other.
    /// </summary>
    public class Mediator : IMediator
    {
        #region ctors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerFunc">Container factory that return a Handler or set of Handlers</param>
        public Mediator(Func<Type, object> containerFunc, Func<Type, IEnumerable<object>> multiContainerFunc)
        {
            ContainerFunc = containerFunc;
            MultiContainerFunc = multiContainerFunc;
        }

        

        #endregion

        #region Public API

        #region Query Handlers
        public TResult Execute<TResult>(IQuery<TResult> request)
        {
            var handler = getQueryHandler(request);
            return handler.Handle(request);
        }
        public async Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> request)
        {
            var handler = getAsyncQueryHandler(request);
            TResult result =await handler.Handle(request);
            return result;
        }
        #endregion


        #region Command Handlers
        public void Execute(ICommand command)
        {
            var handler = getCommandHandler(command);
            handler.Handle(command);
        }
        public async Task ExecuteAsync(ICommand command)
        {
            var handler = getAsyncCommandHandler(command);
            await handler.Handle(command);
        }
        #endregion


        public async Task RaiseAsync(IEvent _event)
        {
            var eventHandlers = getAsyncEventHandlers(_event);

            foreach (var handler in eventHandlers)
            {
               await handler.Handle(_event);
            }
        }

        public void Raise(IEvent _event)
        {
            var eventHandlers = getEventHandlers(_event);

            foreach (var handler in eventHandlers)
            {
                handler.Handle(_event);
            }
        }


        #endregion

        #region Container Registration Methods Handlers
        public Func<Type, object> ContainerFunc { get; private set; }
        public Func<Type, IEnumerable<object>> MultiContainerFunc { get; private set; }
        #endregion


        #region Private handler methods
        
        private QueryHandler<TResult> getQueryHandler<TResult>(IQuery<TResult> query)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            var wrapperType = typeof(QueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            object handler;
            try
            {
                handler = ContainerFunc(handlerType);

                if (handler == null)
                    throw CreateException(query);
            }
            catch (Exception e)
            {
                throw CreateException(query, e);
            }
            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (QueryHandler<TResult>)wrapperHandler;
        }
        private AsyncQueryHandler<TResult> getAsyncQueryHandler<TResult>(IQuery<TResult> query)
        {
            var handlerType = typeof(IAsyncQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            var wrapperType = typeof(AsyncQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            object handler;
            try
            {
                handler = ContainerFunc(handlerType);

                if (handler == null)
                    throw CreateException(query);
            }
            catch (Exception e)
            {
                throw CreateException(query, e);
            }
            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (AsyncQueryHandler<TResult>)wrapperHandler;
        }


        private CommandHandler getCommandHandler(ICommand command)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            var wrapperType = typeof(CommandHandler<>).MakeGenericType(command.GetType());
            object handler;
            try
            {
                handler = ContainerFunc(handlerType);

                if (handler == null)
                    throw CreateException(command);
            }
            catch (Exception e)
            {
                throw CreateException(command, e);
            }
            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (CommandHandler)wrapperHandler;
        }
        private AsyncCommandHandler getAsyncCommandHandler(ICommand command)
        {
            var handlerType = typeof(IAsyncCommandHandler<>).MakeGenericType(command.GetType());
            var wrapperType = typeof(AsyncCommandHandler<>).MakeGenericType(command.GetType());
            object handler;
            try
            {
                handler = ContainerFunc(handlerType);

                if (handler == null)
                    throw CreateException(command);
            }
            catch (Exception e)
            {
                throw CreateException(command, e);
            }
            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (AsyncCommandHandler)wrapperHandler;
        }


        private IEnumerable<EventHandler> getEventHandlers(IEvent _event)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(_event.GetType());
            var wrapperType = typeof(EventHandler<>).MakeGenericType(_event.GetType());

            var handlers = MultiContainerFunc(handlerType);

            return handlers.Select(handler => (EventHandler)Activator.CreateInstance(wrapperType, handler)).ToList();
        }

        private IEnumerable<AsyncEventHandler> getAsyncEventHandlers(IEvent _event)
        {
            var handlerType = typeof(IAsyncEventHandler<>).MakeGenericType(_event.GetType());
            var wrapperType = typeof(AsyncEventHandler<>).MakeGenericType(_event.GetType());

            var handlers = MultiContainerFunc(handlerType);

            return handlers.Select(handler => (AsyncEventHandler)Activator.CreateInstance(wrapperType, handler)).ToList();
        }

      


        private static InvalidOperationException CreateException(object message, Exception inner = null)
        {
            return new InvalidOperationException("Handler was not found for command of type " + message.GetType() + ".\r\nContainer or service locator not configured properly or handlers not registered with your container.", inner);
        }
        #endregion

        #region Private Handler Wrapper classes [Workaround for generic type casting issues]
        private abstract class QueryHandler<TResult>
        {
            public abstract TResult Handle(IQuery<TResult> message);
        }
        private class QueryHandler<TQuery, TResult> : QueryHandler<TResult> where TQuery : IQuery<TResult>
        {
            private readonly IQueryHandler<TQuery, TResult> _inner;

            public QueryHandler(IQueryHandler<TQuery, TResult> inner)
            {
                _inner = inner;
            }

            public override TResult Handle(IQuery<TResult> message)
            {
                return _inner.Handle((TQuery)message);
            }
        }
        
        private abstract class AsyncQueryHandler<TResult>
        {
            public abstract Task<TResult> Handle(IQuery<TResult> message);
        }
        private class AsyncQueryHandler<TQuery, TResult> : AsyncQueryHandler<TResult>
            where TQuery : IQuery<TResult>
        {
            private readonly IAsyncQueryHandler<TQuery, TResult> _inner;

            public AsyncQueryHandler(IAsyncQueryHandler<TQuery, TResult> inner)
            {
                _inner = inner;
            }

            public override Task<TResult> Handle(IQuery<TResult> message)
            {
                return _inner.Handle((TQuery)message);
            }
        }


        private abstract class CommandHandler
        {
            public abstract void Handle(ICommand command);
        }
        private class CommandHandler<TCommand> : CommandHandler where TCommand : ICommand
        {
            private readonly ICommandHandler<TCommand> _inner;

            public CommandHandler(ICommandHandler<TCommand> inner)
            {
                _inner = inner;
            }

            public override void Handle(ICommand command)
            {
                _inner.Execute((TCommand)command);
            }
        }

        private abstract class AsyncCommandHandler
        {
            public abstract Task Handle(ICommand message);
        }
        private class AsyncCommandHandler<TCommand> : AsyncCommandHandler where TCommand : ICommand
        {
            private readonly IAsyncCommandHandler<TCommand> _inner;

            public AsyncCommandHandler(IAsyncCommandHandler<TCommand> inner)
            {
                _inner = inner;
            }

            public async override Task Handle(ICommand message)
            {
                await _inner.ExecuteAsync((TCommand)message);
            }
        }

        private abstract class EventHandler
        {
            public abstract void Handle(IEvent message);
        }
        private class EventHandler<TEvent> : EventHandler where TEvent : IEvent
        {
            private readonly IEventHandler<TEvent> _inner;

            public EventHandler(IEventHandler<TEvent> inner)
            {
                _inner = inner;
            }

            public override void Handle(IEvent message)
            {
                _inner.Handle((TEvent)message);
            }
        }

        private abstract class AsyncEventHandler
        {
            public abstract Task Handle(IEvent message);
        }
        private class AsyncEventHandler<TEvent> : AsyncEventHandler where TEvent : IEvent
        {
            private readonly IAsyncEventHandler<TEvent> _inner;

            public AsyncEventHandler(IAsyncEventHandler<TEvent> inner)
            {
                _inner = inner;
            }

            public async override Task Handle(IEvent message)
            {
                await _inner.Handle((TEvent)message);
            }
        }
        #endregion

        
    }
}
