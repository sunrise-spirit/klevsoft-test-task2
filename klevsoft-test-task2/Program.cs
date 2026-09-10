using ConcurrentCounter;

const int writerCount = 5;
const int incrementsPerWriter = 100_000;
const int readerCount = 5;
const int readsPerReader = 200_000;

var tasks = new List<Task>();

// запускаем писателей каждый в своём потоке 100000 раз прибавляет 1 к счётчику
for (int w = 0; w < writerCount; w++)
{
    tasks.Add(Task.Run(() =>
    {
        for (int i = 0; i < incrementsPerWriter; i++)
            CounterServer.AddToCount(1);
    }));
}

// запускаем читателей каждый в своём потоке 200000 раз просто читает счётчик
for (int r = 0; r < readerCount; r++)
{
    tasks.Add(Task.Run(() =>
    {
        for (int i = 0; i < readsPerReader; i++)
            CounterServer.GetCount();
    }));
}

// ждём,пока все писатели и читатели закончат работу
await Task.WhenAll(tasks);

int expected = writerCount * incrementsPerWriter;
Console.WriteLine($"Ожидалось: {expected}, получено: {CounterServer.GetCount()}");
