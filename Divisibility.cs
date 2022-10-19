using System.Numerics;

namespace Primes.Divisibility
{
    class BasicDivisibility
    {
        public static int DigitSum(BigInteger input)
        {
            //ciferný součet využívaný v metodách pro dělitelnost
            ulong digitSum = 0;
            for (int i = 0; i < input.ToString().Length; i++)
            {
                digitSum += Convert.ToUInt64(input.ToString()[i] - 48);
            }
            return Convert.ToInt32(digitSum);
        }
        public static bool DivisibleByTwo(BigInteger input)
        {
            //dělitelnost two
            return input.IsEven;
        }
        public static bool DivisibleByThree(BigInteger input)
        {
            //dělitelnost třema
            while (input.ToString().Length > 1)
            {
                input = DigitSum(input);
            }
            return (input % 3 == 0);
        }
        public static bool DivisibleByFive(BigInteger input)
        {
            //dělitelnost pěti
            return ((input % 10 == 0) || (input % 10 == 5));
        }
        public static bool DivisibleByNine(BigInteger input)
        {
            //dělitelnost devíti
            while (input.ToString().Length > 1) input = DigitSum(input);

            return (input % 9 == 0);
        }
        public static bool DivisibleByBasic(BigInteger input)
        {
            //kombinace dělitelností, výstupní funkce
            return (DivisibleByTwo(input) || DivisibleByThree(input) || DivisibleByFive(input)) ? true : false;
        }
    }

        public class AdvancedDivisibility
        {
            private BigInteger divisor;

            public AdvancedDivisibility(BigInteger Divisor)
            {
                this.divisor = Divisor;
            }
            public bool IsDivisible(BigInteger Dividend)
            {
                byte b = (byte)(Dividend % 10);                 //posledni cifra
                BigInteger a = (Dividend - b) / 10;             //zbytek, jeste zkusit - posledni cifra, /10
                BigInteger k;                                   //konstanta k
                switch ((byte)(divisor % 10))
                {
                    case 1:
                        k = (divisor * 1 - 1) / 10;
                        break;
                    case 3:
                        k = (divisor * 7 - 1) / 10;
                        break;
                    case 7:
                        k = (divisor * 3 - 1) / 10;
                        break;
                    case 9:
                        k = (divisor * 9 - 1) / 10;
                        break;
                    default:
                        Console.WriteLine("Chyba v divisor % 10- divisor nesmí být soudělný s deseti! divisor je {0} a dividend je: {1}", divisor, Dividend);
                        return false;
                }

                BigInteger n2 = Dividend;
                //k++;
                while (a > k * b)
                {
                    n2 = a - k * b;
                    b = (byte)(n2 % 10);
                    a = (n2) / 10;
                    Console.WriteLine("a: {0}, b: {1}, k{2}, n2, {3}", a, b, k, n2);
                }
                //n2 += b;
                if (n2 % divisor == 0) return true;
                else return false;
            }

        }
    }
