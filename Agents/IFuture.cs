namespace Agents
{
    public interface IFuture<T>
    {
        ISynchedFuture<T> On(IScheduler scheduler);
    }

    public interface IFuture
    {
        ISynchedFuture On(IScheduler scheduler);
    }
}