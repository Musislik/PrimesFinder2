using Primes.Networking;
using System.Numerics;
using Primes.PrimesFinder;
using Primes.Communication;


string connStringDB = "server=PrimesDB; port=3306; database=sys; ";
var network = new Network(true, 100, 1000);
var sql = new MySqlCom(connStringDB);
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/start", () => 
{

    PrimesFinder pf = new PrimesFinder(new MySqlCom(connStringDB),network);

    for (BigInteger i = sql.LastPrime + 2 ; ; i+=2)
    {
        if (pf.IsPrime(i)) sql.PrimesWriter(new List<BigInteger> {i});
    }

});
app.MapPost("/GetTask", (DcConfiguration DcConfig) =>
{
    return network.GetTask(new DivisibilityChecker(DcConfig.baseAdress, DcConfig.ip4, DcConfig.id));
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
