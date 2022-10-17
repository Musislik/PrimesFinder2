using Primes.Networking;
using System.Numerics;
using Primes.PrimesFinder;
using Primes.Communication;
using System.Net.Http.Headers;
using System.Diagnostics;

bool running = false;
//string connStringDB = "Server=88.101.172.29; Port=2606; Database=sys; ";
//string connStringDB = "Server=PrimesDB; Port=3306; Database=sys; ";
string connStringDB = "Server=10.0.1.26; Port=3306; Database=sys; ";

//var network = new Network(Environment.GetEnvironmentVariable("Scan") == "True", Convert.ToInt32(Environment.GetEnvironmentVariable("WaitTime")), Convert.ToInt32(Environment.GetEnvironmentVariable("TasksLimit")));
var network = new Network(true, 100, 1000000000);

//HttpClient broadCast = new HttpClient();
//broadCast.BaseAddress = new Uri("http://255.255.255.255:255/");
//broadCast.DefaultRequestHeaders.Clear();
//broadCast.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

var sql = new MySqlCom(connStringDB);
Console.WriteLine("sql state: " + sql.State);
int parallelCount = 10;

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

void Run()
{
    do
    {    
        PrimesFinder pf = new PrimesFinder(new MySqlCom(connStringDB), network);
        var primesToWrite = new List<BigInteger>();
        var sw = new Stopwatch();
        BigInteger numberToCheck = sql.LastPrime + 2;
        sw.Start();
        Parallel.For(0, parallelCount, (parallelIndex) =>
        {
            BigInteger value = numberToCheck + 2 * parallelIndex;
            if(pf.IsPrime(value)) primesToWrite.Add(value);
        });
        sw.Stop();
        Console.WriteLine("Zkontrolování {0} èísel trvalo {1}ms", parallelCount, sw.ElapsedMilliseconds);

        //for (; running; i += 2)
        //{
        //    sw.Start();
        //    if (pf.IsPrime(i)) primesToWrite.Add(i);
        //    sw.Stop();
        //    Console.WriteLine("It tooks: " + sw.ElapsedMilliseconds + "ms");
        //    sw.Reset();
        //    if (primesToWrite.Count > 50000)
        //    {
        //        sql.PrimesWriter(primesToWrite);
        //        primesToWrite.Clear();
        //    }
        //}    
        sql.PrimesWriter(primesToWrite);
    } while (running);
}
