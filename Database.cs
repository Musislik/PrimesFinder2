using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Primes.Communication;

namespace Primes.Networking
{
    public class Database : INetworkDevice
    {
        private bool online;
        private uint id;
        private byte[] ipv4, ipv6;
        private const DeviceType devType = DeviceType.Database;
        private string connString;
        private MySqlCom sql;

        public bool Online() 
        { 
           sql = new MySqlCom(connString);
           return sql.State;
            
        }
        public uint Id { get { return id; } }
        public byte[] Ipv4 { get { return ipv4; } }
        public byte[] Ipv6 { get { return ipv6; } }
        public DeviceType DevType { get { return devType; } }

        public Database(string connString, byte[] ipv4, uint id)
        {
            this.connString = connString;
            this.id = id;
            if(ipv4.Length == 4) this.ipv4 = ipv4;
        }
    }
}
