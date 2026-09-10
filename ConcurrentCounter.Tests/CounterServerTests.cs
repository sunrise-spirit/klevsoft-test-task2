using ConcurrentCounter;
using Xunit;

namespace ConcurrentCounter.Tests;


public class CounterServerTests
{
    [Fact]
    public void AddToCount_IncreasesCountByValue()
    {
        int before = CounterServer.GetCount();

        CounterServer.AddToCount(5);

        Assert.Equal(before + 5, CounterServer.GetCount());
    }

    [Fact]
    public void AddToCount_SupportsNegativeValues()
    {
        int before = CounterServer.GetCount();

        CounterServer.AddToCount(-3);

        Assert.Equal(before - 3, CounterServer.GetCount());
    }

    [Fact]
    public async Task AddToCount_NoLostUpdatesUnderConcurrentWriters()
    {
        
        int before = CounterServer.GetCount();
        const int writerCount = 20;
        const int incrementsPerWriter = 5000;

        var writers = Enumerable.Range(0, writerCount)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < incrementsPerWriter; i++)
                    CounterServer.AddToCount(1);
            }))
            .ToArray();

        await Task.WhenAll(writers);

        int expected = before + writerCount * incrementsPerWriter;
        Assert.Equal(expected, CounterServer.GetCount());
    }

    [Fact]
    public async Task GetCount_IsConsistentUnderConcurrentReadersAndWriters()
    {
        int before = CounterServer.GetCount();
        const int writerCount = 10;
        const int incrementsPerWriter = 1000;
        const int readerCount = 10;
        const int readsPerReader = 2000;

        var readers = Enumerable.Range(0, readerCount)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < readsPerReader; i++)
                {
                    // все прибавления в этом тесте положительные,поэтому значение
                    // не должно уменьшаться ниже того,что было до старта.
                    int snapshot = CounterServer.GetCount();
                    Assert.True(snapshot >= before);
                }
            }))
            .ToArray();

        var writers = Enumerable.Range(0, writerCount)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < incrementsPerWriter; i++)
                    CounterServer.AddToCount(1);
            }))
            .ToArray();

        await Task.WhenAll(readers.Concat(writers));

        int expected = before + writerCount * incrementsPerWriter;
        Assert.Equal(expected, CounterServer.GetCount());
    }
}
