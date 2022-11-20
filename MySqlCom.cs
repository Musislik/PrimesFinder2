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
                    Console.WriteLine(e.Message + this.ConnString);
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
            PrimesWriter(new List<BigInteger> { new BigInteger(2), new BigInteger(3), new BigInteger(5), new BigInteger(7) });
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

                    System.Console.WriteLine(e.Message);
                    throw;
                }
                return 0;
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
        //            System.Console.WriteLine(e.Message);
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
                Console.WriteLine(e.Message);
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
                Console.WriteLine(e.Message);
            }
            conn.Close();


            //var Primes = new List<BigInteger>();
            //try
            //{
            //    using (var connection = new MySqlConnection(mySqlConnectionString_PrimesReader))
            //    {
            //        connection.Open();

            //        using var Command = connection.CreateCommand();
            //        Command.CommandType = System.Data.CommandType.Text;
            //        Command.CommandText = "select value from sys.Primes;";
            //        var dataReader = Command.ExecuteReader();
            //        while (dataReader.Read()) Primes.Add(BigInteger.Parse(dataReader.GetString("value")));
            //    }
            //}
            //catch (MySqlException e)
            //{ 
            //    System.Console.WriteLine(e.Message);
            //}
            return primes;
        }
        public void PrimesWriter(List<BigInteger> values)
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

                //if(i + 2 >= values.Count) command += " (@image" + (i + 1) + ", " + values[i+1].GetByteCount(true) + ");";

                //command += "INSERT INTO sys.Primes SET Value = @image" + (i*2 + 1) + ", Size = @image" + (i * 2 + 2) + ";";

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
        public void ParallelPrimesWriter(List<BigInteger> values)
        {
            if (values.Count == 0 || values == null) goto end;
            _ = Parallel.ForEach(values, value =>
            {

                try
                {
                    using (MySqlConnection connection = new(mySqlConnectionString_PrimesWriter))
                    {
                        connection.Open();
                        var Command = connection.CreateCommand();
                        Command.CommandType = System.Data.CommandType.Text;
                        Command.CommandText = "insert into sys.Primes (Value) values (" + value + ");";
                        Command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                catch (MySqlException e)
                {
                    System.Console.WriteLine(e.Message);
                }

            });
        end:;
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

    }
}