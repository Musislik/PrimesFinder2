using System.Numerics;

namespace Primes.Communication
{
    class DUnit
    {
        public BigInteger Divisor, Dividend;
        public bool IsBig;
        DUnit(BigInteger divisor, BigInteger dividend, bool isBig)
        {
            this.IsBig = isBig;
            this.Dividend = dividend;
            this.Divisor = divisor;
        }
    }

    internal class DcConfiguration
    {
        public string baseAdress;
        public uint id;
        public byte[] ip4;

        public DcConfiguration(string baseAdress, uint id, byte[] ip4)
        {
            this.baseAdress = baseAdress;
            this.id = id;
            this.ip4 = ip4;
        }
        public DcConfiguration(uint id, byte[] ip4)
        {
            this.baseAdress = "http://" + ip4[0] + "." + ip4[1] + "." + ip4[2] + "." + ip4[3] + "/";
            this.id = id;
            this.ip4 = ip4;
        }
    }
    /*public class DivideTask
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
    */
}
