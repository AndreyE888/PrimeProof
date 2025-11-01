using System.Numerics;

namespace PrimeProof.Utilities
{
    /// <summary>
    /// Расширения для работы с BigInteger
    /// </summary>
    public static class BigIntegerExtensions
    {
        /// <summary>
        /// Проверяет, является ли число вероятно простым (базовая проверка)
        /// </summary>
        public static bool IsProbablyPrime(this BigInteger value, int certainty = 5)
        {
            if (value < 2) return false;
            if (value == 2 || value == 3) return true;
            if (value.IsEven) return false;

            // Проверка на маленькие простые числа
            if (value <= 1000)
            {
                return IsSmallPrime(value);
            }

            // Базовая проверка Миллера-Рабина для маленькой certainty
            return MillerRabinQuickCheck(value, certainty);
        }

        /// <summary>
        /// Вычисляет квадратный корень из BigInteger
        /// </summary>
        public static BigInteger Sqrt(this BigInteger n)
        {
            if (n == 0) return 0;
            if (n > 0)
            {
                int bitLength = Convert.ToInt32(Math.Ceiling(BigInteger.Log(n, 2)));
                BigInteger root = BigInteger.One << (bitLength / 2);

                while (!IsSqrt(n, root))
                {
                    root += n / root;
                    root /= 2;
                }

                return root;
            }

            throw new ArithmeticException("NaN");
        }

        /// <summary>
        /// Проверяет, является ли root квадратным корнем из n
        /// </summary>
        private static bool IsSqrt(BigInteger n, BigInteger root)
        {
            BigInteger lowerBound = root * root;
            BigInteger upperBound = (root + 1) * (root + 1);
            return n >= lowerBound && n < upperBound;
        }

        /// <summary>
        /// Генерирует случайное BigInteger в диапазоне [min, max]
        /// </summary>
        public static BigInteger NextBigInteger(this Random random, BigInteger min, BigInteger max)
        {
            if (min > max) throw new ArgumentException("Min cannot be greater than max");

            BigInteger range = max - min;
            byte[] bytes = range.ToByteArray();
            byte[] buffer = new byte[bytes.Length];

            // Генерируем случайное число
            random.NextBytes(buffer);
            buffer[bytes.Length - 1] &= 0x7F; // Ensure positive

            BigInteger result = new BigInteger(buffer);
            result %= range + 1;
            return min + result;
        }

        /// <summary>
        /// Вычисляет НОД двух чисел
        /// </summary>
        public static BigInteger GCD(this BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                BigInteger temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// Вычисляет модульное обратное a^(-1) mod n
        /// </summary>
        public static BigInteger ModInverse(this BigInteger a, BigInteger n)
        {
            BigInteger t = 0, newT = 1;
            BigInteger r = n, newR = a;

            while (newR != 0)
            {
                BigInteger quotient = r / newR;
                (t, newT) = (newT, t - quotient * newT);
                (r, newR) = (newR, r - quotient * newR);
            }

            if (r > 1) throw new ArithmeticException("Число не имеет обратного по модулю");
            if (t < 0) t += n;
            return t;
        }

        /// <summary>
        /// Быстрая проверка на простоту для маленьких чисел
        /// </summary>
        private static bool IsSmallPrime(BigInteger n)
        {
            if (n < 2) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            BigInteger limit = n.Sqrt();
            for (BigInteger i = 3; i <= limit; i += 2)
            {
                if (n % i == 0) return false;
            }
            return true;
        }

        /// <summary>
        /// Быстрая проверка Миллера-Рабина
        /// </summary>
        private static bool MillerRabinQuickCheck(BigInteger n, int k)
        {
            if (n < 2) return false;
            if (n == 2 || n == 3) return true;
            if (n % 2 == 0) return false;

            // Разложение n-1 = 2^s * d
            BigInteger d = n - 1;
            int s = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                s++;
            }

            Random rng = new Random();
            for (int i = 0; i < k; i++)
            {
                BigInteger a = rng.NextBigInteger(2, n - 2);
                BigInteger x = BigInteger.ModPow(a, d, n);

                if (x == 1 || x == n - 1)
                    continue;

                for (int j = 0; j < s - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == n - 1)
                        break;
                    if (x == 1)
                        return false;
                }

                if (x != n - 1)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Форматирует BigInteger для красивого отображения
        /// </summary>
        public static string ToFormattedString(this BigInteger number)
        {
            string str = number.ToString();
            if (str.Length <= 15) return str;

            // Для больших чисел показываем начало и конец
            return $"{str.Substring(0, 10)}...{str.Substring(str.Length - 5)}";
        }
    }
}