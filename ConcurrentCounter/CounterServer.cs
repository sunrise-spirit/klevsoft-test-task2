namespace ConcurrentCounter;


public static class CounterServer
{
    private static int _count;
    private static readonly ReaderWriterLockSlim Lock = new(LockRecursionPolicy.NoRecursion);

    public static int GetCount()
    {
        Lock.EnterReadLock();
        try
        {
            return _count;
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public static void AddToCount(int value)
    {
        Lock.EnterWriteLock();
        try
        {
            _count += value;
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }
}
