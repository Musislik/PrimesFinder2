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

        PrimesFinder(MySqlCom sql, Network network)
        {
            this.network = network;
            this.sql = sql;
            this.lastPrime = sql.LastPrime;
        }

        bool IsPrime(BigInteger number)
        {
            int i = 0;
            do
            {
                var divisor = sql.PrimeReader(i);
                if (BigInteger.Multiply(divisor, divisor) < number) i++;
                else return true;
                if (network.IsDivisible(number, divisor)) return false;
            }
            while(true);
        }

    }
    public class Prime
    {
        BigInteger value;
        int ID, size;        
    }
}
