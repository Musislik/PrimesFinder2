﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Primes.Communication;
using System.Net.Http.Headers;
using System.Numerics;
using System.Net.Http.Json;

namespace Primes.Networking
{
    public class DivisibilityChecker : INetworkDevice
    {
        //Private

        private uint id;
        private byte[] ipv4, ipv6;
        private const DeviceType devType = DeviceType.DivisibilityChecker;
        private bool isBusy = false;
        public string baseAddress;
        private HttpClient client = new HttpClient();


        //Public

        public bool Online()
        {
            try
            {
                var dc = GetDCState();
                return dc.Result.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public bool IsBusy { get { return isBusy; } set { isBusy = value; } }
        public uint Id { get { return id; } }
        public byte[] Ipv4 { get { return ipv4; } }
        public byte[] Ipv6 { get { return ipv6; } }
        public DeviceType DevType { get { return devType; } }

        //Konstrukor

        public DivisibilityChecker(string baseAddress, byte[] ipv4, uint id)
        {
            this.baseAddress = baseAddress;
            this.ipv4 = ipv4;
            this.id = id;

            this.client.BaseAddress = new Uri(this.baseAddress);
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        //DC metody
        public async Task<HttpResponseMessage> StartQuery(BigInteger Divisor, BigInteger Dividend, bool isBig)
        {
            return await client.PostAsJsonAsync("divisibility", new List<byte[]> { Divisor.ToByteArray(), Dividend.ToByteArray() });
        }
        public async Task<HttpResponseMessage> GetDCState()
        {
            //client.Timeout = TimeSpan.FromMilliseconds(100);
            return await client.GetAsync("state");
        }
        public async Task<HttpResponseMessage> Setup()
        {
            return await client.PostAsJsonAsync("setup", new DcConfiguration(this.baseAddress, this.id, this.ipv4));
        }
        public static bool DCExists(string baseAdress, byte[] ip4)
        {
            var dc = new DivisibilityChecker(baseAdress, ip4, 0);
            return dc.Online();
        }
    }
}
