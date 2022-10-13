using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Primes.Communication;

namespace Primes.Networking
{
    public class Network
    {
        
        public List<INetworkDevice> devices;
        public List<DivideTask> tasks;
        public int waitTime = 100;
        public int tasksLimit = 1000;

        public bool ipInUse(byte[] ip4)
        {   
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].Ipv4 == ip4) return true;
            }

            return false;
        }

        public IEnumerator<INetworkDevice> Databases()
        {
            foreach (var device in devices)
            {
                if (device.DevType == DeviceType.Database) yield return device;
            }
        }
        public IEnumerator<INetworkDevice> DivisibilityCheckers()
        {
            foreach (var device in devices)
            {
                if (device.DevType == DeviceType.DivisibilityChecker) yield return device;
            }
        }
        //public async Task<bool> IsDivisible(BigInteger Dividend, BigInteger Divisor)
        //{
        //    int id = tasks.Count;
        //    tasks.Add(new DivideTask(Dividend, Divisor, id));

        //    while (!isDone()) Thread.Sleep(waitTime);

        //    var res = result();

        //    return res;

        //    bool result()
        //    {
        //        foreach (var task in tasks)
        //        {
        //            if (task.ID == id) return task.Result;
        //            else throw new Exception();
        //        }
        //        throw new Exception();
        //    }
        //    bool isDone()
        //    {
        //        foreach (var task in tasks)
        //        {
        //            if (task.ID == id) return task.Done;
        //        }
        //        throw new Exception();
        //    }
        //}
        
        public DivideTask GetTask(DivisibilityChecker dc)
        {
            while (true)
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    if (!tasks[i].Processing & !tasks[i].Done)
                    {
                        tasks[i].Processing = true;
                        tasks[i].DcId = dc.Id;
                        return tasks[i];
                    }
                }
                Console.WriteLine("{0}.{1}.{2}.{3} Čeká na task!", dc.Ipv4[0],dc.Ipv4[1],dc.Ipv4[2],dc.Ipv4[3]);
                Thread.Sleep(waitTime);
            }
        }

        public List<DivideTask> SendTask (List<DivideTask> input)
        {

            DivideTask freeDivideTask(List<DivideTask> input)
            {
                foreach (var task in input)
                {
                    if (!task.Done & !task.Processing) return task;
                }
                return null;
            }

            DivisibilityChecker freeDc(List<INetworkDevice> input)
            {
                foreach (var device in input)
                {
                    if (device.DevType == DeviceType.DivisibilityChecker)
                    {
                        if (!((DivisibilityChecker)device).IsBusy())
                        {
                            return (DivisibilityChecker)device;
                        }
                    }
                }
                return null;
            }

            while (true)
            {
                var task = freeDivideTask(input);
                if (task != null)
                {
                    var dc = freeDc(devices);
                    if (dc != null) task.SendTask(dc);
                    else break;
                }
                else break;
            }

            return input;
        }


        public void AddDatabase(string connString, byte[] ipv4, uint id) { devices.Add(new Database(connString, ipv4, id)); }
        public void AddDivisibilityChecker(string baseAddress, byte[] ipv4, uint id) { devices.Add(new DivisibilityChecker(baseAddress, ipv4, id)); }

        public Network(bool scan, int waitTime, int tasksLimit) 
        {
            this.tasksLimit = tasksLimit;
            this.waitTime = waitTime;

            devices = new List<INetworkDevice>();
            tasks = new List<DivideTask>();

            if (scan) ScanNetwork();            
        }
        private void ScanNetwork()
        {
            Console.WriteLine("scanning network");
            var db = new Database("server = PrimesDB; port = 3306; database = sys; ", new byte[] { 26, 26, 26, 26 }, (uint)devices.Count);
            //var db = new Database("server = 10.0.1.26; port = 3306; database = sys; ", new byte[] { 26, 26, 26, 26 }, (uint)devices.Count);
            if (db.Online())
            {

                devices.Add(db);
                Console.WriteLine("DB has been added!");
            }

            //for (int i = 0; i < 255; i++)
            //{
            //    string baseAdress = "http://26.0.1." + i + ":80/";
            //    byte[] ip = { 26, 0, 1, Convert.ToByte(i) };

            //    if (DivisibilityChecker.DCExists(baseAdress, ip) & !ipInUse(ip))
            //    {
            //        var newDev = new DivisibilityChecker(baseAdress, ip, (uint)devices.Count);
            //        devices.Add(newDev);
            //        //newDev.Setup().Wait();
            //        Console.WriteLine("DC{0}, ip:{1}.{2}.{3}.{4}", devices.Count - 1, ip[0], ip[1], ip[2], ip[3]);
            //    }
            //}

            Parallel.For(0, 255, i =>
            {
                string baseAdress = "http://26.0.1." + i + ":80/";
                byte[] ip = { 26, 0, 1, Convert.ToByte(i) };

                    if (DivisibilityChecker.DCExists(baseAdress, ip))       //& IpInUse
                    {
                        var newDev = new DivisibilityChecker(baseAdress, ip, (uint)devices.Count);
                        devices.Add(newDev);
                        //newDev.Setup().Wait();
                        Console.WriteLine("DC{0}, ip:{1}.{2}.{3}.{4}", devices.Count - 1, ip[0], ip[1], ip[2], ip[3]);
                    }
                
            });
            Console.WriteLine("Scan ended");
                                    
        }
        
    }
    public class DivideTask
    {
        public BigInteger Dividend;
        public BigInteger Divisor;
        public int ID;
        public bool Processing, Done, Result;
        public uint DcId;
        
        public DivideTask(BigInteger Dividend, BigInteger Divisor, int ID)
        {
            this.Dividend = Dividend;
            this.Divisor = Divisor;
            this.ID = ID;

            this.DcId = 0;
            this.Processing = false;
            this.Done = false;
        }
        public void SendTask(DivisibilityChecker dc)
        {
            DcId = dc.Id;
            Processing = true;
            var result = dc.StartQuery(Divisor, Dividend, true);
            Result = result.Result.Content.ReadAsStringAsync().Result == "true";
            Processing = false;
            Done = true;
        }
    }    
}
