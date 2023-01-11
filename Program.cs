using System.Numerics;
using Primes.Communication;
using System.Net.Http.Headers;
using System.Diagnostics;
using Primes.Divisibility;

bool running = false;
//string connStringDB = "Server=88.101.172.29; Port=2606; Database=sys; ";
//string connStringDB = "Server=PrimesDB; Port=3306; Database=sys; ";
string connStringDB = "Server=10.0.1.26; Port=3306; Database=sys; ";

var sql = new MySqlCom(connStringDB);
Console.WriteLine("sql state: " + sql.State);
int parallelCount = 1000, primesWriterCount = 500;
var primesToWrite = new List<BigInteger>();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions((options) => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/start", () =>
{
    if (!running)
    {
        running = true;
        Task.Run(() => Run());
    }
});
app.MapGet("/stop", () =>
{
    running = false;
});



app.MapGet("/set/parallelCount/{count}", (int count) =>
{
    parallelCount = count;
    return StatusCodes.Status200OK;
});
app.MapGet("/set/primesWriterCount/{count}", (int count) =>
{
    primesWriterCount = count;
    return StatusCodes.Status200OK;
});

app.MapGet("/mysql/reset", () =>
{
    Console.WriteLine("Reset DB!");
    try
    {
        sql.dbReset();
    }
    catch (Exception)
    {
        return 400;
    }
    return 200;
});
app.MapGet("/mysql/setup", () =>
{
    Console.WriteLine("Setup DB!");
    try
    {
        sql.dbSetup();
    }
    catch (Exception)
    {
        return 400;
    }
    return 200;
});
app.MapGet("/mysql/get/state", () =>
{
    Console.WriteLine("Sql state: " + sql.State);
    return sql.State;
});
app.MapGet("/mysql/get/connstring", () =>
{
    Console.WriteLine("Conn string: " + sql.ConnString);
    return sql.ConnString;
});
app.MapPost("/mysql/set/connString", (string connString) =>
{
    Console.WriteLine("Creating new connection client with new connection string: " + connString);
    try
    {
        sql = new MySqlCom(connString);
    }
    catch (Exception)
    {
        return StatusCodes.Status400BadRequest;
    }
    return StatusCodes.Status200OK;
});

app.Run();

async Task Run()
{
    try
    {
        Console.WriteLine("Starting to count primes. PrallelCount = {0}, PrimesWriterCount = {1}.", parallelCount, primesWriterCount);
        var sw = new Stopwatch();
        Console.WriteLine("Reading primes");
        List<BigInteger> primes = sql.PrimesReader();
        Console.WriteLine("Readed");
        List<Task> tasks = new List<Task>();
        List<Task> tasks2 = new List<Task>();
        BigInteger firstNumberToCheck = primes[primes.Count - 1] + 2;

        
        if (primes.Count < 100)
        {
            var numberToCheck0 = primes[primes.Count - 1] + 2;
            for (int i = 0; primes.Count < 100; i++)
            {
                IsPrime(numberToCheck0 + i * 2, primes);
            }
            var writingPrimes = primesToWrite.ToArray();
            primesToWrite.Clear();
            Console.WriteLine("count: {0}", primes.Count);
            await sql.PrimesWriter(writingPrimes);
        }

        //Main
        for (BigInteger numberToCheck = firstNumberToCheck; running & primes.Count >= 100 ;numberToCheck += parallelCount * 2)
        {
            sw.Start();
            //Parallel.For(0, parallelCount, (i) =>
            //{
            //tasks2.Add(IsPrime(numberToCheck + (i * 2), primes));
            //});

            for (int i = 0; i < parallelCount; i++)
            {
                tasks2.Add(IsPrime(numberToCheck + (i * 2), primes));
            }

            //Wait
            if (tasks2.Count > 0)
            {
                for (int i = 0; i < tasks2.Count; i++)
                {
                    while (!tasks2[i].IsCompleted)
                    {
                        Console.WriteLine("Waiting");
                        Thread.Sleep(10);                        
                    }
                }
                tasks2.Clear();
            }
            //Write
            if (primesToWrite.Count > primesWriterCount || BigInteger.Pow(primes[primes.Count - 1], 2) <= numberToCheck)
            {

                sw.Stop();
                Console.WriteLine("couting tooks: " + sw.ElapsedMilliseconds);
                if (tasks.Count > 0)
                {
                    while (!tasks[0].IsCompleted & tasks.Count > 0)
                    {
                        Thread.Sleep(10);
                    }
                }

                tasks.Clear();
                var writingPrimes = primesToWrite.ToArray();
                Console.WriteLine("count: {0}", primes.Count);
                await sql.PrimesWriter(writingPrimes);
                Console.WriteLine("test2");
                primesToWrite.Clear();
                sw.Reset();
                sw.Start();
            }
        };

        if (tasks.Count > 0)
        {
            Console.WriteLine("Waiting");
            while (!tasks[0].IsCompleted & tasks.Count > 0)
            {
                Thread.Sleep(10);                
            }
            Console.WriteLine("Done");
        }
        var writingPrimes2 = primesToWrite.ToArray();
        sql.PrimesWriter(writingPrimes2);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        throw;
    }
}


async Task<bool> IsPrime(BigInteger number, List<BigInteger> primes)
{
    if (primes == null) throw new ArgumentNullException(nameof(primes));
    try
    {
        var sw = new Stopwatch();
        //Console.WriteLine("IsPrime: " + number);
        bool isDivisible = false;
        int biggestIndex = 0;
        bool exit = false;

        sw.Start();
        if(BasicDivisibility.DivisibleByThree(number) ^ BasicDivisibility.DivisibleByFive(number))
        {
            return false;
        }
        
        for (int primeIndex = 0; primes[primeIndex] * primes[primeIndex] <= number; primeIndex++)
        {
            biggestIndex = primeIndex + 1;
            while (primes.Count < biggestIndex + 1)
            {
                Thread.Sleep(5);
            }
        }
        Parallel.For(0, biggestIndex, async (i, aa) =>
        {
            await Task.Run( () => {
            if (number % primes[i] == 0)
            {
                exit = true;
                aa.Stop();
            }
            });
        });

        

        if (exit) return false;
        primes.Add(number);
        primesToWrite.Add(number);
        return true;
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        throw;
    }
}
