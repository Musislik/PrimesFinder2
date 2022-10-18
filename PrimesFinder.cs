using Primes.Networking;
using Primes.Communication;
using System.Numerics;
using Primes.Divisibility;
using System.Diagnostics;

namespace Primes.PrimesFinder
{
    public class PrimesFinder
    {
        MySqlCom sql;
        BigInteger lastPrime;
        Network network;
        List<BigInteger> primes;

        public PrimesFinder(MySqlCom sql, Network network)
        {
            this.network = network;
            this.sql = sql;
            this.lastPrime = sql.LastPrime;

            this.primes = sql.PrimesReader();
        }

        public bool IsPrime2(BigInteger number)
        {          
            Console.WriteLine("IsPrime: " + number);
            int i = 4;  //[1] = 2
            bool isDivisible = false;
            List<DivideTask> tasksInProcess = new List<DivideTask>();
            BigInteger divisor = sql.PrimeReader(i);


            do
            {
                while (tasksInProcess.Count >= network.tasksLimit)
                {
                    Console.WriteLine("waiting... Max tasks");
                    tasksCheckAndDelete();
                    if (isDivisible)
                    {
                        return false;
                    }

                    tasksInProcess = network.SendTask(tasksInProcess);

                    Thread.Sleep(50);
                }

                
                if (BigInteger.Multiply(divisor, divisor) <= number) divisor = primes[i++];
                else
                {
                    while (mustWait())
                    {
                        Console.WriteLine("Waiting...");
                        Thread.Sleep(100);
                        tasksCheckAndDelete();                        
                    }
                    if (isDivisible) return false;
                    primes.Add(number);
                    return true;
                }
                
                tasksInProcess.Add(new DivideTask(number, divisor, tasksInProcess.Count));
                tasksInProcess = network.SendTask(tasksInProcess);
                
                
                bool mustWait()
                {
                    bool output = false;
                    Parallel.ForEach(tasksInProcess, (task,state)  =>
                    {
                        if (task.Dividend == number) output = true;
                        state.Break();
                    });
                    return output;
                }
                bool tasksCheckAndDelete()
                {
                    while (tasksInProcess.Count >= network.tasksLimit)
                    {
                        Console.WriteLine("waiting");
                        Parallel.ForEach(tasksInProcess, (task) =>
                        {
                            if (task.Done)
                            {
                                if (task.Result) isDivisible = true;
                            }
                        });
                    }
                    tasksInProcess.Clear();
                    return isDivisible;
                }


            }
            while(true);
        }
        public bool IsPrime(BigInteger number, List<BigInteger> primes)
        {
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("IsPrime: " + number);
            int primeIndex = 3;  //[1] = 2
            bool isDivisible = false;
            List<DivideTask> tasksInProcess = new List<DivideTask>();
            if (number < 32)
            {
                if (BasicDivisibility.DivisibleByBasic(number)) return false;
            }
            
            while (true)
            {
                BigInteger divisor = primes[primeIndex++];
                while (tasksInProcess.Count >= network.tasksLimit)
                {                    
                    Console.WriteLine("waiting... Max tasks");
                    Thread.Sleep(100);
                    //task check

                    List<int> tasksToDelteIndex = new List<int>();

                    do
                    {
                        Parallel.For(0, tasksInProcess.Count, (index) =>
                        {
                            if (tasksInProcess[index].Done & tasksInProcess[index].Result)
                            {
                                isDivisible = true;
                                tasksToDelteIndex.Add(index);
                            }
                        });
                        if(tasksToDelteIndex.Count == 0) Thread.Sleep(10);
                    } while (tasksInProcess.Count == 0);

                    for (int i = 0; i < tasksToDelteIndex.Count; i++)
                    {
                        tasksInProcess.RemoveAt(tasksToDelteIndex[i] - i);
                    }
                }
                var numnum = BigInteger.Pow(divisor, 2);
                if (numnum <= number)
                {
                    if (numnum == number) return false;
                    tasksInProcess.Add(new DivideTask(number, divisor, 0));
                }
                else
                {
                    sw.Stop();
                    Console.WriteLine("all sended in {0}ms", sw.ElapsedMilliseconds);
                    sw.Reset();
                    bool output = true;
                    //task check
                    Parallel.ForEach(tasksInProcess, (task) =>
                    {
                        while (!task.Done) { };

                        if (task.Result) output = false;
                    });
                    return output;
                    //return
                }

                tasksInProcess = network.SendTask(tasksInProcess);
            }
        }

    }
}
