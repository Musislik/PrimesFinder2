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

        public PrimesFinder(MySqlCom sql, Network network)
        {
            this.network = network;
            this.sql = sql;
            this.lastPrime = sql.LastPrime;
        }

        public bool IsPrime(BigInteger number)
        {

            if (BasicDivisibility.DivisibleByBasic(number)) return false;

            Console.WriteLine("IsPrime: " + number);
            int i = 4;  //[1] = 2
            bool tRes = false;
            List<DivideTask> tasksInProcess = new List<DivideTask>();
            
            
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

                var divisor = sql.PrimeReader(i);
                if (BigInteger.Multiply(divisor, divisor) <= number) i++;
                else
                {
                    for (int j = 0; j < tasksInProcess.Count; j++)
                    {
                        while (!tasksInProcess[j].Done)
                        {
                            Thread.Sleep(100);
                            Console.WriteLine("Waiting...");
                        }
                    }
                    
                    tasksCheck();

                    if (tRes)
                    {
                        return false;
                    }
                    return true;
                }
                
                tasksInProcess.Add(new DivideTask(number, divisor, tasksInProcess.Count));
                tasksInProcess = network.SendTask(tasksInProcess);
                
                void tasksCheck()
                {
                    Parallel.ForEach(tasksInProcess, task =>
                    {
                        if (task.Done)
                        {
                            if (task.Result) tRes = true;
                        }
                    });                    
                }
                void tasksCheckAndDelete()
                {
                    Parallel.ForEach(tasksInProcess, task =>
                    {
                        if (task.Done)
                        {
                            if (task.Result) tRes = true;
                            else tasksInProcess.Remove(task);
                        }
                    });
                }


            }
            while(true);
        }

    }
}
