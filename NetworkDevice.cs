using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Primes.Networking
{
    public interface INetworkDevice
    {
        bool Online { get; }
        uint Id { get; }
        byte[] Ipv4 { get; }
        byte[] Ipv6 { get; }
        DeviceType DevType { get; }

        void ping() { Console.WriteLine(Ipv4); }
    }
    public enum DeviceType
    {
        Database, DivisibilityChecker
    }

}
