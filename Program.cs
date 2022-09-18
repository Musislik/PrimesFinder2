using Primes.Networking;
using System.Numerics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
string connStringDB = "server=PrimesDB; port=3306; database=sys; ";

app.MapGet("/", () => "Hello World!");

app.Run();

var Pnet = new Network();

Pnet.devices.Add(new Database(connStringDB, new byte[] { 26, 26, 26, 26 }, 0));
Pnet.devices.Add(new DivisibilityChecker("http://localhost:5165/", new byte[] { 26, 0, 1, 0 }, 1));
