using System.Diagnostics;

namespace Playground;

public static class Primes
{
    public static IEnumerable<ulong> GetPrimes(int size)
    {
        var sw = Stopwatch.StartNew();
        var primes = new List<ulong>(size) {2};
        for (ulong i = 2; primes.Count < size; i++)
        {
            var passed = true;
            foreach (ulong prime in primes)
                if (i % prime == 0)
                    goto exit;
                else if (prime * prime > i) break;
            if (passed)
                primes.Add(i);
            exit: ;
        }

        Console.WriteLine(sw.ElapsedMilliseconds);
        return primes;
    }
}