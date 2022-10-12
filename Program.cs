using Primes.Networking;
using System.Numerics;
using Primes.PrimesFinder;
using Primes.Communication;

/*
var client = new HttpClient();
client.BaseAddress = new Uri("http://26.0.1.0/");
Console.WriteLine(client.GetAsync("state").Result.StatusCode);
Console.ReadLine();
*/

bool running = false;
string connStringDB = "Server=10.0.1.26; Port=3306; Database=sys; ";
var network = new Network(Environment.GetEnvironmentVariable("Scan") == "True", Convert.ToInt32(Environment.GetEnvironmentVariable("WaitTime")), Convert.ToInt32(Environment.GetEnvironmentVariable("TasksLimit")));
//var network = new Network(false, 100, 1000);

var sql = new MySqlCom(connStringDB);
Console.WriteLine("sql state: " + sql.State);

sql.dbReset();

var builder = WebApplication.CreateBuilder(args);
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
    PrimesFinder pf = new PrimesFinder(new MySqlCom(connStringDB), network);
    
    for (BigInteger i = sql.LastPrime + 2; running == true; i += 2)
    {
        if (pf.IsPrime(i)) sql.PrimesWriter(new List<BigInteger> { i });
    }
}