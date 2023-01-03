using MySql.Data.MySqlClient;
using System.Numerics;
using Primes;
using System.Threading;
using System.Data;
using System.Diagnostics;

namespace Primes.Communication
{
    public class MySqlCom
    {
        public string ConnString { get { return connString; } }
        private string connString = "";
        public bool State
        {
            get
            {
                try
                {
                    using (var connection = new MySqlConnection(mySqlConnectionString_PrimesReader))
                    {                        
                        connection.Open();
                        connection.Close();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString() + this.ConnString);
                    return false;
                }

            }
        }


        private string mySqlConnectionString_PrimesReader = "server=localhost; port=3306; uid=PrimesReader; pwd=9987; database=sys; charset=utf8; SslMode=Required;"; //Pokud bude problem, vymazat sslMode=none;
        private string mySqlConnectionString_PrimesWriter = "server=localhost; port=3306; uid=PrimesWriter; pwd=9987; database=sys; charset=utf8; SslMode=Required;";
        private string mySqlConnectionString_Root = "server=localhost; port=3306; uid=root; pwd=9987; database=sys; charset=utf8; SslMode=Required;"; //Pokud bude problem, vymazat sslMode=none;

        public MySqlCom(string connString)
        {
            this.connString = connString;
            mySqlConnectionString_PrimesReader = connString + "uid = PrimesReader; pwd = 9987; charset=utf8;";
            mySqlConnectionString_PrimesWriter = connString + "uid = PrimesWriter; pwd = 9987; charset=utf8;";
            mySqlConnectionString_Root = connString + "uid = root; pwd = 9987; charset=utf8;";
            
        }
        public void dbReset()
        {
            InsertCommand("use sys; delete from Primes where PrimeID > 4; ALTER TABLE Primes AUTO_INCREMENT=5;");
        }
        public void dbSetup()
        {
            InsertCommand("use sys; delete from Primes where PrimeID > 0; ALTER TABLE Primes AUTO_INCREMENT=1;");
            PrimesWriter(new BigInteger[] { new BigInteger(2), new BigInteger(3), new BigInteger(5), new BigInteger(7) });
        }
        public void InsertWritingCommand(string command, uint count)
        {
            using (var connection = new MySqlConnection(mySqlConnectionString_Root))
            {
                connection.Open();
                var Command = connection.CreateCommand();
                Command.CommandType = System.Data.CommandType.Text;
                Command.CommandText = "use sys; ";
                Command.ExecuteNonQuery();
                //asdasdasd
            }
        }

        public BigInteger LastPrime
        {
            get
            {
                try
                {
                    using (var connection = new MySqlConnection(mySqlConnectionString_PrimesReader))
                    {
                        var cmd = new MySqlCommand("select * from sys.LastPrimeValue;", connection);
                        MySqlDataReader myData;
                        UInt32 Size;
                        byte[] rawData;

                        connection.Open();
                        myData = cmd.ExecuteReader();

                        if (!myData.HasRows)
                            throw new Exception("Chyba pøi ètení prvoèísla z LastPrime");

                        myData.Read();
                        Size = myData.GetUInt32(myData.GetOrdinal("Size"));
                        rawData = new byte[Size];
                        myData.GetBytes(myData.GetOrdinal("Value"), 0, rawData, 0, (int)Size);

                        return new BigInteger(rawData, true);
                    }
                }
                catch (MySqlException e)
                {

                    System.Console.WriteLine(e.ToString());
                    throw;
                }
            }
        }
        

        public async Task InsertCommand(string command)
        {
            using (var connection = new MySqlConnection(mySqlConnectionString_Root))
            {
                await connection.OpenAsync();
                var Command = connection.CreateCommand();
                Command.CommandType = System.Data.CommandType.Text;
                Command.CommandText = command;
                await Command.ExecuteNonQueryAsync();
            }
        }
        public BigInteger PrimeReader(BigInteger index)
        {
            //potreba udelat jako parametr
            try
            {
                using (var conn = new MySqlConnection(mySqlConnectionString_PrimesReader))
                {
                    var cmd = new MySqlCommand("select * from Primes where PrimeID=" + index, conn);
                    MySqlDataReader myData;
                    UInt32 Size;
                    byte[] rawData;

                    conn.Open();
                    myData = cmd.ExecuteReader();

                    if (!myData.HasRows)
                        throw new Exception("Chyba pøi ètení prvoèísla z primesReader");

                    myData.Read();
                    Size = myData.GetUInt32(myData.GetOrdinal("Size"));
                    rawData = new byte[Size];
                    myData.GetBytes(myData.GetOrdinal("Value"), 0, rawData, 0, (int)Size);

                    return new BigInteger(rawData, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return 0;
        }
        public List<BigInteger> PrimesReader()
        {
            var conn = new MySqlConnection(mySqlConnectionString_PrimesReader);
            var cmd = new MySqlCommand("select * from Primes", conn);
            var primes = new List<BigInteger>();
            MySqlDataReader myData;
            UInt32 Size;
            byte[] rawData;

            conn.Open();
            myData = cmd.ExecuteReader();

            if (!myData.HasRows)
                throw new Exception("Chyba pøi ètení prvoèísla z listu");


            try
            {
                while (myData.Read())
                {
                    Size = myData.GetUInt32(myData.GetOrdinal("Size"));
                    rawData = new byte[Size];
                    myData.GetBytes(myData.GetOrdinal("Value"), 0, rawData, 0, (int)Size);
                    var output = new BigInteger(rawData, true);
                    if (output > 0) primes.Add(output);
                    else Console.WriteLine(" asdasd {0}", output);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            conn.Close();



            return primes;
        }




        public async Task PrimesWriter(BigInteger[] values)
        {
            var sw = new Stopwatch();
            sw.Start();
            string path = "./mysql/commands/procedureCalls/";
            string filePath = "./mysql/commands/procedureCalls/" + values.Length + ".txt";
            string command = await ProcedureCallCommandReader(values.Length);
            if(State)
            {
                try
                {
                    await Write();
                }                
                catch (MySqlException e)
                {
                    Console.WriteLine("Unexpected fail. " + e.ToString());
                    Console.WriteLine(e.ToString());
                    throw;
                }
            }
            else
            {
                Console.WriteLine("DB is not connected! Failed to write!");
            }
            sw.Stop();
            Console.WriteLine("Writing - {0} - {1}ms", values.Length, sw.ElapsedMilliseconds);


            async Task Write()
            {
                using (var connection = new MySqlConnection(mySqlConnectionString_PrimesWriter))
                {

                    var Command = connection.CreateCommand();
                    Command.CommandType = System.Data.CommandType.Text;
                    Command.CommandText = command;

                    for (int i = 0; i < values.Length; i++)
                    { 
                        var data = values[i].ToByteArray(true);
                        Command.Parameters.Add(("@value" + i), MySqlDbType.LongBlob).Value = data;
                        Command.Parameters.Add(("@size" + i), MySqlDbType.Int32).Value = values[i].GetByteCount(true);
                    }

                    await connection.OpenAsync();
                    try
                    {
                        await Command.ExecuteNonQueryAsync();
                    }
                    catch (MySqlException e) when (e.Number == 1305) //procedura neexistuje
                    {
                        await connection.CloseAsync();
                        await ProcedureCreator(values.Length);
                        await connection.OpenAsync();
                        await Command.ExecuteNonQueryAsync();
                    }
                }
            }
            
            async Task<string> ProcedureCallCommandReader(int primesWriterCount)
            {
                var conn = new MySqlConnection(mySqlConnectionString_PrimesReader);
                var cmd = new MySqlCommand("select CommandText from sys.WritingCommands where Count=" + primesWriterCount, conn);
                string output = null;


                conn.Open();
                var myData = await cmd.ExecuteReaderAsync();

                if (!myData.HasRows)
                {
                    await ProcedureCreator(primesWriterCount);
                    conn.Close();
                    conn.Open();
                    myData = await cmd.ExecuteReaderAsync();
                }
                try
                {
                    while (myData.Read())
                    {
                        output = myData.GetString("CommandText");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                conn.Close();
                return output;
            }

        }

        

        public async Task ProcedureCreator(int primesWriterCount)
        {
            try
            {

                Console.WriteLine("Creating procedure Write{0}Primes", primesWriterCount);
                var sw = new Stopwatch();
                sw.Start();
                string command = "USE `sys`; \nDROP procedure IF EXISTS `Write" + primesWriterCount + "Primes`; \nCREATE PROCEDURE `Write" + primesWriterCount + "Primes` (";

                for (int i = 0; i < primesWriterCount; i++)
                {
                    command += "in value" + i + " longblob, in size" + i + " int";

                    if (i + 1 < primesWriterCount)
                    {
                        command += ", ";
                    }
                    else
                    {
                        command += ") \nBegin \nInsert into sys.Primes(Value, Size) Values";
                    }
                }
                for (int i = 0; i < primesWriterCount; i++)
                {
                    command += "(value" + i + ", size" + i + ")";

                    if (i + 1 < primesWriterCount)
                    {
                        command += ", ";
                    }
                    else
                    {
                        command += "; \nEND;";
                    }
                }
                
                Console.WriteLine("Inserting");
                await InsertCommand(command);
                Console.WriteLine("ProcedureCallCreator");
                await ProcedureCallCreator(primesWriterCount);
                sw.Stop();
                Console.WriteLine("Procedure creator - {0} - {1}ms", primesWriterCount, sw.ElapsedMilliseconds);
            }
            catch (MySqlException e) when (e.Number == 3507) //moc
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }        
        public async Task ProcedureCallCreator(int primesWriterCount)
        {
            var sw = new Stopwatch();
            sw.Start();
            string commandText = "call Write" + primesWriterCount + "Primes(";
            string command = "insert into sys.WritingCommands(Count, CommandText) Values(" + primesWriterCount + ",@image);";
            for (int i = 0; i < primesWriterCount; i++)
            {
                commandText += "@value" + i + " ,@size" + i;

                if (i + 1 < primesWriterCount)
                {
                    commandText += ", ";
                }
                else
                {
                    commandText += ");";
                }
            }

            try
            {                
                await Write();                               
            }
            catch (MySqlException e) when (e.Number == 1062) //Duplikace
            {
                Console.WriteLine("duplikace");
                //throw new NotImplementedException();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            sw.Stop();
            Console.WriteLine("Procedure call creator - {0} - {1}ms", primesWriterCount, sw.ElapsedMilliseconds);

            async Task Write()
            {
                using (var connection = new MySqlConnection(mySqlConnectionString_PrimesWriter))
                {
                    var Command = connection.CreateCommand();
                    Command.CommandType = System.Data.CommandType.Text;
                    Command.CommandText = command;
                    Command.Parameters.Add("@image", MySqlDbType.LongText).Value = commandText;

                    await connection.OpenAsync();
                    await Command.ExecuteNonQueryAsync();
                    
                }
            }
        }
    }
}