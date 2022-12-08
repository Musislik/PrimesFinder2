using MySql.Data.MySqlClient;
using System.Numerics;
using Primes;
using System.Threading;
using System.Data;

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
                            throw new Exception("Chyba p�i �ten� prvo��sla z LastPrime");

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
        //public BigInteger SecLastPrime
        //{
        //    get
        //    {
        //        BigInteger secLastPrime = 0;
        //        try
        //        {
        //            using (var connection = new MySqlConnection(mySqlConnectionString_PrimesReader))
        //            {
        //                connection.Open();

        //                using var Command = connection.CreateCommand();
        //                Command.CommandType = System.Data.CommandType.Text;
        //                Command.CommandText = "select * from sys.LastTwoPrimesValue;";
        //                var dataReader = Command.ExecuteReader();
        //                while (dataReader.Read())
        //                {
        //                    dataReader.GetString("value");
        //                    secLastPrime = BigInteger.Parse(dataReader.GetString("value"));
        //                }
        //            }
        //        }
        //        catch (MySqlException e)
        //        {
        //            System.Console.WriteLine(e.ToString());
        //        }
        //        return secLastPrime;
        //    }
        //}

        public void InsertCommand(string command)
        {
            using (var connection = new MySqlConnection(mySqlConnectionString_Root))
            {
                connection.Open();
                var Command = connection.CreateCommand();
                Command.CommandType = System.Data.CommandType.Text;
                Command.CommandText = command;
                Command.ExecuteNonQuery();
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
                        throw new Exception("Chyba p�i �ten� prvo��sla z primesReader");

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
                throw new Exception("Chyba p�i �ten� prvo��sla z listu");


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
        public void PrimesWriterOld(List<BigInteger> values)
        {
            
            foreach (BigInteger value in values)
            {
                var data = value.ToByteArray(true);
                using (var con = new MySqlConnection(mySqlConnectionString_PrimesWriter))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand("INSERT INTO sys.Primes SET Value = @image1, Size = @image2", con))
                    {
                        cmd.Parameters.Add("@image1", MySqlDbType.LongBlob).Value = data;
                        cmd.Parameters.Add("@image2", MySqlDbType.UInt32).Value = (uint)data.Length;
                        cmd.ExecuteNonQuery();
                    }
                }
            };
        }
        public async Task PrimesWriterAtOnce(BigInteger[] values)
        {
            //Console.WriteLine("Writing, count: " + values.Length);
            string command = "Insert into sys.Primes(Value, Size) Values";
            
                for (int i = 0; i < values.Length; i++)
                {

                    command += " (@image" + i + ", " + values[i].GetByteCount(true) + ")";

                    if (i + 1 < values.Length)
                    {
                        command += ",";
                    }
                    else
                    {
                        command += ";";
                    }
                }

            using (var con = new MySqlConnection(mySqlConnectionString_PrimesWriter))
            {
                using (var cmd = new MySqlCommand(command, con))
                {                    
                    for (int i = 0; i < values.Length; i++)
                    {
                        var data = values[i].ToByteArray(true);

                        cmd.Parameters.Add(("@image" + i), MySqlDbType.LongBlob).Value = data;
                        //cmd.Parameters.Add(("@image" + (i * 2 + 2)), MySqlDbType.UInt32).Value = (uint)data.Length;
                    };
                    con.Open();
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                }                
            }
            //Console.WriteLine("Writed");
        }
        
        

        public async Task PrimesWriter(BigInteger[] values)
        {

            string path = "./mysql/commands/procedureCalls/";
            string filePath = "./mysql/commands/procedureCalls/" + values.Length + ".txt";
            string command = null;

            if (File.Exists(filePath))
            {
                command = File.ReadAllText(filePath);
            }
            else
            {
                ProcedureCallStringCreator(values.Length);
                command = File.ReadAllText(filePath);
            }
            if(State)
            {
                try
                {
                    await Write();
                }
                catch (MySqlException e) when (e.Number == 1305) //Procedure is not created
                {
                    Console.WriteLine("Creating procedure Write{0}Primes", values.Length);
                    ProcedureCreator(values.Length);
                    await Write();
                }
                catch (MySqlException e)
                {
                    Console.WriteLine("Unexpected fail. " + e.Message);
                    throw;
                }
            }
            else
            {
                Console.WriteLine("DB is not connected! Failed to write!");
            }


            async Task Write()
            {
                using (var connection = new MySqlConnection(mySqlConnectionString_PrimesWriter))
                {

                    var Command = connection.CreateCommand();
                    Command.CommandType = System.Data.CommandType.Text;
                    Command.CommandText = command;

                    Parallel.For(0, values.Length, (i) => { 
                        var data = values[i].ToByteArray(true);
                        Command.Parameters.Add(("@value" + i), MySqlDbType.LongBlob).Value = data;
                        Command.Parameters.Add(("@size" + i), MySqlDbType.Int32).Value = values[i].GetByteCount(true);
                    });

                    await connection.OpenAsync();
                    await Command.ExecuteNonQueryAsync();
                }
            }

        }
        //public IEnum methods 
        public IEnumerator<BigInteger> Primes(BigInteger To)
        {
            var primes = PrimesReader();

            for (int i = 0; primes[i] < To; i++)
                yield return primes[i];
        }
        public IEnumerator<BigInteger> Primes(BigInteger From, BigInteger To)
        {
            var primes = PrimesReader();
            int index = 0;
            while (primes[index] < From)
            {
                if (index < primes.Count) index++;
                else yield break;
            }
            for (; primes[index] < To; index++)
                yield return primes[index];
        }

        public void ProcedureCreator(int primesWriterCount)
        {
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
            InsertCommand(command);

        }
    
        public void ProcedureCallStringCreator(int primesWriterCount)
        {
            string path = "./mysql/commands/procedureCalls/";
            string filePath = "./mysql/commands/procedureCalls/" + primesWriterCount + ".txt";
            string command = "call Write" + primesWriterCount +"Primes(";

            try
            {
                for (int i = 0; i < primesWriterCount; i++)
                {
                    command += "@value" + i + " ,@size" + i;

                    if (i + 1 < primesWriterCount)
                    {
                        command += ", ";
                    }
                    else
                    {
                        command += ");";
                    }
                }                
                if (Directory.Exists(path))
                {
                    if (File.Exists(filePath))
                    {
                        Console.WriteLine("overwriting file: " + filePath);
                        File.WriteAllTextAsync(filePath, command);
                    }
                    else
                    {
                        File.WriteAllTextAsync(filePath, command);
                    }
                }
                else
                {
                    Directory.CreateDirectory(path);
                    File.WriteAllTextAsync(filePath, command);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }
}