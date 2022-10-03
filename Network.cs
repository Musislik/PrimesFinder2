using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Primes.Networking
{
    public class Network
    {
        public List<INetworkDevice> devices;
        public List<DivideTask> tasks;
        public int waitTime = 100;

        public bool ipInUse(byte[] ip4)
        {
            foreach (var item in devices)
            {
                if (item.Ipv4 == ip4) return true;
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
        public bool IsDivisible(BigInteger Dividend, BigInteger Divisor)
        {
            int id = tasks.Count;
            tasks.Add(new DivideTask(Dividend, Divisor, id));

            while (!isDone()) Thread.Sleep(waitTime);

            return result();

            bool result()
            {
                foreach (var task in tasks)
                {
                    if (task.ID == id) return task.Result;
                    else throw new Exception();
                }
                throw new Exception();
            }
            bool isDone()
            {
                foreach (var task in tasks)
                {
                    if (task.ID == id) return task.Done;
                    else throw new Exception();
                }
                throw new Exception();
            }
        }
        
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
                Console.WriteLine(dc.Ipv4 + " Čeká na task!");
                Thread.Sleep(waitTime);
            }
        }

        public void AddDatabase(string connString, byte[] ipv4, int id) { devices.Add(new Database(connString, ipv4, id)); }
        public void AddDivisibilityChecker(string baseAddress, byte[] ipv4, int id) { devices.Add(new DivisibilityChecker(baseAddress, ipv4, id)); }

        public Network(bool scan, int waitTime) 
        {
            this.waitTime = waitTime;

            if(scan) devices = new List<INetworkDevice>();
        }
        private void ScanNetwork()
        {
            var db = new Database("http://26.26.26.26/", new byte[] { 26, 26, 26, 26 }, devices.Count);

            if (db.Online) devices.Add(db);

            for (int i = 0; i <= 255; i++)
            {
                string baseAdress = "http://10.0.1." + i + "/";
                byte[] ip = { 26, 0, 1, Convert.ToByte(i) };

                if (DivisibilityChecker.DCExists(baseAdress, ip) & !ipInUse(ip)) devices.Add(new DivisibilityChecker(baseAdress, ip, devices.Count));               
            }
                                    
        }
        
    }
    public class DivideTask
    {
        public BigInteger Dividend;
        public BigInteger Divisor;
        public int ID;
        public bool Processing, Done, Result;
        public int DcId;
        
        public DivideTask(BigInteger Dividend, BigInteger Divisor, int ID)
        {
            this.Dividend = Dividend;
            this.Divisor = Divisor;
            this.ID = ID;

            this.DcId = 0;
            this.Processing = false;
            this.Done = false;
        }
    }
}
