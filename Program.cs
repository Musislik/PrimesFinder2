using System.Numerics;
using Primes.Communication;
using System.Net.Http.Headers;
using System.Diagnostics;
using Primes.Divisibility;

bool running = false;
//string connStringDB = "Server=(public ip); Port=2606; Database=sys; ";
string connStringDB = "Server=PrimesDB; Port=3306; Database=sys; ";
//string connStringDB = "Server=10.0.1.26; Port=3306; Database=sys; ";

var sql = new MySqlCom(connStringDB);
Console.WriteLine("sql state: " + sql.State);
int parallelCount = 1000, primesWriterCount = 1000;
var primesToWrite = new List<BigInteger>();
var logs = new List<Log>();
var swLog = new Stopwatch();


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions((options) => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/start", () =>
{
    if (!running)
    {
        swLog.Start();
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
        Console.WriteLine("Reseting DB!");
        sql.dbReset();
        Console.WriteLine("Done");
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
        Console.WriteLine("Done");
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
        Console.WriteLine("Reading primes");
        logs.Add(new Log(swLog.ElapsedMilliseconds, LogType.Start, 0));
        logs.Add(new Log(swLog.ElapsedMilliseconds, LogType.ReadingPrimesStarted, 0));
        List<BigInteger> primes = sql.PrimesReader();
        logs.Add(new Log(swLog.ElapsedMilliseconds, LogType.ReadingPrimesEnded, primes.Count));
        Console.WriteLine("Readed");
        Console.WriteLine("Checking");
        int miss = 0;
        var sw = new Stopwatch();
        //This create an input for parallel loop located in main
        int[] parallelInput = new int[parallelCount];
        for (int i = 0; i < parallelCount; i++)
        {
            parallelInput[i] = (i * 2);
        }



        Parallel.For(1, primes.Count - 1, (i) =>
        {
            if (primes[i] < primes[i - 1])
            {
                miss++;
                Console.WriteLine("PrimeID:{0},{1}, value: {2},{3}", i, i - 1, primes[i], primes[i - 1]);
            }
        });
        Console.WriteLine("miss: " + miss);
        List<Task> writingTasks = new List<Task>();
        List<Task> countingTasks = new List<Task>();
        BigInteger firstNumberToCheck;
        if (primes.Count > 3)
        {
            firstNumberToCheck = primes[primes.Count - 1] + 2;
        }
        else if (primes.Count == 0)
        {
            primes.Add(2);
            primes.Add(3);
            primes.Add(5);
            primes.Add(7);
            firstNumberToCheck = 9;
        }
        else
        {
            throw new Exception("Chyba pøi ètení prvoèísel");
        }


        //if (primes.Count < 100)
        //{
        //    var numberToCheck0 = primes[primes.Count - 1] + 2;
        //    for (int i = 0; primes.Count < 100; i++)
        //    {
        //        IsPrime(numberToCheck0 + i * 2, primes);
        //    }
        //    var writingPrimes = primesToWrite.ToArray();
        //    primesToWrite.Clear();
        //    Console.WriteLine("count: {0}", primes.Count);
        //    await sql.PrimesWriter(writingPrimes);
        //}

        //Main
        for (BigInteger numberToCheck = firstNumberToCheck; running; numberToCheck += parallelCount * 2)
        {
            logs.Add(new Log(swLog.ElapsedMilliseconds, LogType.CountingPrimesStarted, primes.Count));
            sw.Start();
            Parallel.ForEach(parallelInput, (i) =>
            {
                var response = IsPrime(numberToCheck + i, primes);
                countingTasks.Add(response);
                //Console.WriteLine("{0} - Task{1} was deployed.", sw.ElapsedMilliseconds,i);
            });
            Console.WriteLine("{0} - All tasks was deployed.", sw.ElapsedMilliseconds);

            //Wait
            await Task.WhenAll(countingTasks);
            Console.WriteLine("{0} - All counting tasks ended.", sw.ElapsedMilliseconds);
            logs.Add(new Log(swLog.ElapsedMilliseconds, LogType.CountingPrimesEnded, primes.Count));
            //Write
            if (primesToWrite.Count > primesWriterCount || BigInteger.Pow(primes[primes.Count - 1], 2) <= numberToCheck)
            {

                sw.Stop();
                Console.WriteLine("{0} - Couting ended.", sw.ElapsedMilliseconds);

                primesToWrite.Sort();
                var writingPrimes = primesToWrite.ToArray();
                Console.WriteLine("Count: {0} primes", primes.Count);
                //await Task.WhenAll(writingTasks);
                if (writingTasks.Count > 0) logs.Add(new Log(swLog.ElapsedMilliseconds, LogType.WritingPrimesEnded, primes.Count));
                writingTasks.Clear();
                logs.Add(new Log(swLog.ElapsedMilliseconds, LogType.WritingPrimesStarted, primes.Count));
                var response = sql.PrimesWriter(writingPrimes);
                writingTasks.Add(response);

                primesToWrite.Clear();
                sw.Restart();
            }
        }
        Console.WriteLine(Directory.GetCurrentDirectory());
        logs.Add(new Log(swLog.ElapsedMilliseconds, LogType.Stop, primes.Count));
        string logOutput = "";
        foreach (var log in logs)
        {
            logOutput += log.time.ToString() + ", " + log.type.ToString() + ", " + log.PrimesCount.ToString() + ";\n";
        }
        Directory.CreateDirectory("/app/share/logs");

        File.WriteAllText(("/app/share/logs/Log_" + DateTime.Now.ToString("dd_MM_yyyy-HH:mm:ss") + ".log"), logOutput);
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
        if (BasicDivisibility.DivisibleByThree(number) ^ BasicDivisibility.DivisibleByFive(number))
        {
            return false;
        }

        for (int primeIndex = 0; primes[primeIndex] * primes[primeIndex] <= number; primeIndex++)
        {
            biggestIndex = primeIndex + 1;
            while (primes.Count < biggestIndex + 1)
            {
                Thread.Sleep(5);
                Console.WriteLine("Èekám na nalezení více prvoèísel");
            }
        }
        Parallel.For(0, biggestIndex, async (i, aa) =>
        {
            await Task.Run(() => {
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

enum LogType
{
    Start,
    Stop,
    ReadingPrimesStarted,
    ReadingPrimesEnded,
    WritingPrimesStarted,
    WritingPrimesEnded,
    CountingPrimesStarted,
    CountingPrimesEnded
}
class Log
{
    public long time;
    public LogType type;
    public int PrimesCount;

    public Log(long time, LogType type, int primesCount)
    {
        this.time = time;
        this.type = type;
        PrimesCount = primesCount;
    }
}
