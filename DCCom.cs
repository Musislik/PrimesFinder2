using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Numerics;

namespace Primes.Communication
{
    //public class DCCom
    //{
    //    private HttpClient client;

    //    public DCCom(HttpClient client)
    //    {
    //        this.client = client;
    //    }

    //    public async Task<HttpResponseMessage> StartQuery(DUnit res)
    //    {
    //        return await client.PostAsJsonAsync("divisibility", res);
    //    }
    //    public async Task<HttpResponseMessage> GetDCState()
    //    {
    //        return await client.GetAsync("state");
    //    }
       
    //}
    //public class DUnit
    //{
    //    public BigInteger Divisor, Dividend;
    //    public bool IsBig;
    //    DUnit(BigInteger divisor, BigInteger dividend, bool isBig)
    //    {
    //        this.IsBig = isBig;
    //        this.Dividend = dividend;
    //        this.Divisor = divisor;
    //    }
    //}
}
