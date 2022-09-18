using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primes.Networking
{
    public class Network
    {
        public List<INetworkDevice> devices;

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

        public void AddDatabase(string connString, byte[] ipv4, int id) { devices.Add(new Database(connString, ipv4, id)); }
        public void AddDivisibilityChecker(string baseAddress, byte[] ipv4, int id) { devices.Add(new DivisibilityChecker(baseAddress, ipv4, id)); }

        public Network() 
        {
            devices = new List<INetworkDevice>();
        }
    }
}
