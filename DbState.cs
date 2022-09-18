using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Primes.Communication;

namespace Primes.DbState
{
    public class DbState
    {
        private MySqlCom sql;
        private BigInteger lastPrime;
        private BigInteger lastPrimeSize;
        private BigInteger lastPrimeId;

        public BigInteger LastPrimeSize { get => lastPrimeSize; }
        public BigInteger LastPrimeId { get => lastPrimeId; }
        public BigInteger LastPrime { get => lastPrime; }

        public DbState(MySqlCom Sql)
        {
            this.sql = Sql;
            lastPrime = sql.LastPrime;
            lastPrimeSize = GetLastPrimeSize(lastPrime);
            lastPrimeId = GetLastPrimeId(lastPrime);

        }

        private BigInteger GetLastPrimeSize(BigInteger lastPrime)
        {
            throw new NotImplementedException();
        }

        private BigInteger GetLastPrimeId(BigInteger lastPrime)
        {
            throw new NotImplementedException();
        }
    }
}