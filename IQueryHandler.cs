using System.Threading.Tasks;

namespace PremioTek.Mibus
{
    /// <summary>
    /// QueryHandler for handling <see cref="IQuery"/> messages
    /// </summary>
    public interface IQueryHandler<in TQuery, out TResponse> where TQuery : IQuery<TResponse>
    {
        TResponse Handle(TQuery query);
    }

    /// <summary>
    /// Asynchronous QueryHandler for handling <see cref="IQuery"/> messages
    /// </summary>
    public interface IAsyncQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
        Task<TResponse> Handle(TQuery query);
    }
}
