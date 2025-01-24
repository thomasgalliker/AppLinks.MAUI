namespace AppLinks.MAUI.Extensions
{
    internal static class QueueExtensions
    {
        internal static IEnumerable<T> TryDequeueAll<T>(this Queue<T> queue)
        {
            while (queue.TryDequeue(out var item))
            {
                yield return item;
            }
        }
    }
}