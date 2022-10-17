using Primes.Networking;
using Primes.Communication;
using System.Numerics;
using Primes.Divisibility;

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

        public bool IsPrime(BigInteger number)
        {          
            Console.WriteLine("IsPrime: " + number);
            int i = 4;  //[1] = 2
            bool tRes = false;
            List<DivideTask> tasksInProcess = new List<DivideTask>();
            BigInteger divisor = sql.PrimeReader(i);


            do
            {
                while (tasksInProcess.Count >= network.tasksLimit)
                {
                    Console.WriteLine("waiting... Max tasks");
                    tasksCheckAndDelete();
                    if (tRes)
                    {
                        return false;
                    }

                    tasksInProcess = network.SendTask(tasksInProcess);

                    Thread.Sleep(50);
                }

                
                if (BigInteger.Multiply(divisor, divisor) <= number) divisor = primes[i++];
                else
                {
                    for (int j = 0; j < tasksInProcess.Count; j++)
                    {
                        while (!tasksInProcess[j].Done)
                        {
                            
                        }
                    }
                    while (mustWait())
                    {
                        Console.WriteLine("Waiting...");
                        Thread.Sleep(100);
                        tasksCheckAndDelete();
                    }
                    //tady nekde
                    primes.Add(number);
                    return true;
                }
                
                tasksInProcess.Add(new DivideTask(number, divisor, tasksInProcess.Count));
                tasksInProcess = network.SendTask(tasksInProcess);
                
                
                bool mustWait()
                {
                    bool output = false;
                    Parallel.ForEach(tasksInProcess, task =>
                    {
                        if (task.Dividend == number) output = true;
                    });
                    return output;
                }
                bool tasksCheckAndDelete()
                {
                    bool tRes = false;
                    Parallel.ForEach(tasksInProcess, task =>
                    {
                        if (task.Done)
                        {
                            if (task.Result) tRes = true;
                            tasksInProcess.Remove(task);
                        }                        
                    });
                    return tRes;
                }


            }
            while(true);
        }

    }
}
