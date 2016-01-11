using System.Threading.Tasks;

namespace PremioTek.Mibus
{
    /// <summary>
    /// Event Handler
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent" /> type</typeparam>
    public interface IEventHandler<in T> where T : IEvent
    {
        void Handle(T args);
    }

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent" /> type</typeparam>
    public interface IAsyncEventHandler<in T> where T : IEvent
    {
        Task Handle(T args);
    }
}