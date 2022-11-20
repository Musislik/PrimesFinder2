using Primes.Networking;
using System.Numerics;
using Primes.PrimesFinder;
using Primes.Communication;
using System.Net.Http.Headers;
using System.Diagnostics;

bool running = false;
string connStringDB = "Server=88.101.172.29; Port=2606; Database=sys; ";
//string connStringDB = "Server=PrimesDB; Port=3306; Database=sys; ";
//string connStringDB = "Server=10.0.1.26; Port=3306; Database=sys; ";


var network = new Network(Environment.GetEnvironmentVariable("Scan") == "True", Convert.ToInt32(Environment.GetEnvironmentVariable("WaitTime")), Convert.ToInt32(Environment.GetEnvironmentVariable("TasksLimit")));
//var network = new Network(true, 100, 1000000000);

//HttpClient broadCast = new HttpClient();
//broadCast.BaseAddress = new Uri("http://255.255.255.255:255/");
//broadCast.DefaultRequestHeaders.Clear();
//broadCast.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

var sql = new MySqlCom(connStringDB);
Console.WriteLine("sql state: " + sql.State);
int parallelCount = 100, primesWriterCount = 1000;
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



app.MapPost("/set/parallelCount/{0}", (int count) =>
{
    parallelCount = count;
    return StatusCodes.Status200OK;
});
app.MapPost("/set/primesWriterCount/{0}", (int count) =>
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

        PrimesFinder pf = new PrimesFinder(new MySqlCom(connStringDB), network);
        var sw = new Stopwatch();
        BigInteger firstNumberToCheck = sql.LastPrime + 2;
        List<BigInteger> primes = sql.PrimesReader();
        List<Task> tasks = new List<Task>();
        List<Task> tasks2 = new List<Task>();

        if (primes.Count < 100)
        {
            var numberToCheck0 = primes[primes.Count - 1];
            for (int i = 0; primes.Count < 100; i++)
            {
                IsPrime(numberToCheck0 + i * 2, primes);

            }
        }


        for (BigInteger numberToCheck = firstNumberToCheck; running; numberToCheck += (parallelCount * 2))
        {
            for (int i = 0; i <= parallelCount; i++)
            {
                tasks2.Add(IsPrime(numberToCheck + (i * 2), primes));
            }
            if (tasks2.Count > 0)
            {
                for (int i = 0; i < tasks2.Count; i++)
                {
                    while (!tasks2[i].IsCompleted)
                    {
                        Thread.Sleep(10);
                    }
                }
                tasks2.Clear();
            }
            if (primesToWrite.Count > primesWriterCount || BigInteger.Pow(primes[primes.Count - 1], 2) <= numberToCheck)
            {
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
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
                tasks.Add(new Task(async () => await sql.PrimesWriterAtOnce(writingPrimes)));
                tasks[0].Start();
                primesToWrite.Clear();
                sw.Reset();
                sw.Start();
            }
        };

        if (tasks.Count > 0)
        {
            var j = 0;
            while (!tasks[0].IsCompleted & tasks.Count > 0)
            {
                Thread.Sleep(10);
            }
        }
        var writingPrimes2 = primesToWrite.ToArray();
        sql.PrimesWriterAtOnce(writingPrimes2);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        throw;
    }
}

async Task<bool> IsPrime(BigInteger number, List<BigInteger> primes)
{
    try
    {
        var sw = new Stopwatch();
        //Console.WriteLine("IsPrime: " + number);
        bool isDivisible = false;
        int biggestIndex = 0;
        bool exit = false;
        for (int primeIndex = 0; primes[primeIndex] * primes[primeIndex] <= number; primeIndex++)
        {
            biggestIndex = primeIndex;
        }
        Parallel.For(0, biggestIndex, (i, aa) =>
        {
            if (number % primes[i] == 0)
            {
                exit = true;
                aa.Stop();
            }
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