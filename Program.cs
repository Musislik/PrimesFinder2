using Primes.Networking;
using System.Numerics;
using Primes.PrimesFinder;
using Primes.Communication;
using System.Net.Http.Headers;
using System.Diagnostics;

bool running = false;
//string connStringDB = "Server=88.101.172.29; Port=2606; Database=sys; ";
string connStringDB = "Server=PrimesDB; Port=3306; Database=sys; ";
//string connStringDB = "Server=10.0.1.26; Port=3306; Database=sys; ";


var network = new Network(Environment.GetEnvironmentVariable("Scan") == "True", Convert.ToInt32(Environment.GetEnvironmentVariable("WaitTime")), Convert.ToInt32(Environment.GetEnvironmentVariable("TasksLimit")));
//var network = new Network(true, 100, 1000000000);

//HttpClient broadCast = new HttpClient();
//broadCast.BaseAddress = new Uri("http://255.255.255.255:255/");
//broadCast.DefaultRequestHeaders.Clear();
//broadCast.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

var sql = new MySqlCom(connStringDB);
Console.WriteLine("sql state: " + sql.State);
int parallelCount = 100, primesWriterCount = 10000;

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
app.MapGet("/set/scan", () =>
{
    //network.ScanNetwork();
    //return StatusCodes.Status200OK;
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
    PrimesFinder pf = new PrimesFinder(new MySqlCom(connStringDB), network);
    var primesToWrite = new List<BigInteger>();
    var sw = new Stopwatch();
    BigInteger firstNumberToCheck = sql.LastPrime + 2;
    List<BigInteger> primes = sql.PrimesReader();
    List<Task> tasks = new List<Task>();


    for (BigInteger numberToCheck = firstNumberToCheck ; running ; numberToCheck += 2)
    {
        if (IsPrime(numberToCheck, primes).Result)
        {
            primesToWrite.Add(numberToCheck);
            primes.Add(numberToCheck);
        }

        if (primesToWrite.Count > primesWriterCount || primes.Count < 10 || BigInteger.Pow(primes[primes.Count - 1], 2) <= numberToCheck)
        {


            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            if (tasks.Count > 0)
            {
                var j = 0;
                while (!tasks[0].IsCompleted & tasks.Count > 0)
                {
                    Thread.Sleep(10);
                    j++;
                }
                Console.WriteLine("w: " + j);
            }

            tasks.Clear();
            var writingPrimes = primesToWrite.ToArray();
            Console.WriteLine("count: {0}", primes.Count);
            tasks.Add(new Task(async () => await sql.PrimesWriterAtOnce(writingPrimes)) );
            tasks[0].Start();
            primesToWrite.Clear();
            sw.Reset();
            sw.Start();            
        }        
    };
    //sw.Start();
    //Console.WriteLine("writing primes, count: " + primes.Count);
    //Write(primes);
    //sw.Stop();
    //Console.WriteLine("done, it tooks: " + sw.ElapsedMilliseconds);
    if (tasks.Count > 0)
    {
        var j = 0;
        while (!tasks[0].IsCompleted & tasks.Count > 0)
        {
            Thread.Sleep(10);
            j++;    
        }
        Console.WriteLine("w: " + j);
    }
    var writingPrimes2 = primesToWrite.ToArray();
    sql.PrimesWriterAtOnce(writingPrimes2);
}

async Task<bool> IsPrime(BigInteger number, List<BigInteger> primes)
{
    var sw = new Stopwatch();
    //Console.WriteLine("IsPrime: " + number);
    bool isDivisible = false;
    int biggestIndex = 0;
    bool exit = false;
    for (int primeIndex = 0; primes[primeIndex] * primeIndex <= number & primeIndex < primes.Count; primeIndex++)
    {
        biggestIndex = primeIndex;
    }
    Parallel.For(0, biggestIndex, (i, aa) =>
    {
        if (number % primes[i] == 0) exit = true;
        aa.Stop();
    });
    if (exit) return false;

    return true;
}
async Task Write(List<BigInteger> primes)
{
    string[] lines = new string[primes.Count]; 
    
    for(int i = 0; i < primes.Count; i++)  lines[i] = primes[i].ToString();
    await File.WriteAllLinesAsync("Primes.txt", lines);
}