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

app.MapGet("/GetTask", (DcID dcID) =>
{
    var Pnet = new Network(true, 100, 1000);
    return Pnet.GetTask(new DivisibilityChecker("http://78456", new byte[] {26,26,26,26 }, 0));
});

app.Run();


public class DcID
{
    
}