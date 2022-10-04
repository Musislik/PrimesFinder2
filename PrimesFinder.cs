using Primes.Networking;
using Primes.Communication;
using System.Numerics;

namespace Primes.PrimesFinder
{
    public class PrimesFinder
    {
        MySqlCom sql;
        BigInteger lastPrime;
        Network network;

        public PrimesFinder(MySqlCom sql, Network network)
        {
            this.network = network;
            this.sql = sql;
            this.lastPrime = sql.LastPrime;
        }

        public bool IsPrime(BigInteger number)
        {
            int i = 0;
            do
            {
                while (network.tasks.Count >= network.tasksLimit)
                {
                    Console.WriteLine("waiting... Max tasks");
                    Thread.Sleep(100);
                }
                var divisor = sql.PrimeReader(i);
                if (BigInteger.Multiply(divisor, divisor) < number) i++;
                else return true;
                if (network.IsDivisible(number, divisor)) return false;
            }
            while(true);
        }

    }
}
