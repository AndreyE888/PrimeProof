using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using PrimeProof.Services.Interfaces;

namespace PrimeProof.Services.Implementations
{
    /// <summary>
    /// Тест Миллера-Рабина - вероятностный тест простоты
    /// Промышленный стандарт в криптографии
    /// </summary>
    public class MillerRabinTest : IPrimalityTest
    {
        private static readonly Random random = new Random();
        
        // Небольшие простые числа для предварительной проверки
        private static readonly int[] smallPrimes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };

        public string TestName => "Тест Миллера-Рабина";

        public string TestDescription => "Вероятностный тест, промышленный стандарт в криптографии. Надежнее теста Ферма, не обманывается на числа Кармайкла.";

        public bool IsDeterministic => false;

        public bool IsPrime(BigInteger number, int rounds, out List<string> details)
        {
            details = new List<string>();

            // Базовые проверки
            if (number < 2)
            {
                details.Add($"Число {number} меньше 2 - не является простым");
                return false;
            }

            if (number == 2)
            {
                details.Add("Число 2 - простое");
                return true;
            }

            if (number.IsEven)
            {
                details.Add($"Число {number} четное (кроме 2) - составное");
                return false;
            }

            details.Add($"Начинаем тест Миллера-Рабина с {rounds} раундами");
            details.Add("Алгоритм: представляем n-1 = 2^s * d, затем проверяем a^d mod n и a^(2^r * d) mod n");

            // Предварительная проверка на маленькие простые делители
            if (CheckSmallPrimes(number, out var smallPrime))
            {
                details.Add($"🔍 Найден маленький простой делитель: {smallPrime}");
                details.Add($"Число {number} - СОСТАВНОЕ");
                return false;
            }

            // Разлагаем n-1 = 2^s * d
            BigInteger nMinusOne = number - 1;
            int s = 0;
            BigInteger d = nMinusOne;

            while (d.IsEven)
            {
                s++;
                d /= 2;
            }

            details.Add($"Разложение {number}-1 = 2^{s} * {d}");

            for (int i = 0; i < rounds; i++)
            {
                details.Add($"\n--- Раунд {i + 1} ---");

                // Генерируем случайное основание
                BigInteger a = GenerateRandomBase(number);
                details.Add($"Основание a = {a}");

                // Проверяем a^d mod n
                BigInteger x = BigInteger.ModPow(a, d, number);
                details.Add($"Вычисляем a^d mod n = {a}^{d} mod {number} = {x}");

                if (x == 1 || x == nMinusOne)
                {
                    details.Add($"✓ Условие выполнено: {x} ≡ 1 или {x} ≡ {nMinusOne} (mod {number})");
                    continue;
                }

                // Проверяем a^(2^r * d) mod n для r = 1..s-1
                bool foundWitness = false;
                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, number);
                    details.Add($"Вычисляем x^{2} mod n = {x} (r = {r})");

                    if (x == nMinusOne)
                    {
                        details.Add($"✓ Условие выполнено: {x} ≡ {nMinusOne} (mod {number})");
                        foundWitness = true;
                        break;
                    }

                    if (x == 1)
                    {
                        details.Add($"❌ Найдено свидетельство составности: {x} ≡ 1 (mod {number})");
                        details.Add($"Число {number} - СОСТАВНОЕ");
                        return false;
                    }
                }

                if (!foundWitness)
                {
                    details.Add($"❌ Найдено свидетельство составности: цепочка не пришла к {nMinusOne}");
                    details.Add($"Число {number} - СОСТАВНОЕ");
                    details.Add($"Основание {a} является свидетелем Миллера-Рабина");
                    return false;
                }
            }

            details.Add($"\n✅ Все {rounds} раундов пройдены успешно");
            details.Add($"Число {number} - ВЕРОЯТНО ПРОСТОЕ");
            details.Add($"Вероятность ошибки: ≤ 4^(-{rounds}) = {Math.Pow(4, -rounds):E2}");
            return true;
        }

        public double GetProbability(int rounds)
        {
            // Для теста Миллера-Рабина вероятность ошибки <= 4^(-k)
            return 1 - Math.Pow(4, -rounds);
        }

        public bool IsApplicable(BigInteger number)
        {
            // Тест применим ко всем нечетным числам > 2
            return number > 2 && !number.IsEven;
        }

        /// <summary>
        /// Проверяет маленькие простые делители
        /// </summary>
        private bool CheckSmallPrimes(BigInteger number, out int divisor)
        {
            foreach (int prime in smallPrimes)
            {
                if (number == prime)
                {
                    divisor = prime;
                    return false; // Число само является маленьким простым
                }

                if (number % prime == 0)
                {
                    divisor = prime;
                    return true; // Найден делитель
                }
            }
            divisor = 0;
            return false;
        }

        /// <summary>
        /// Генерирует случайное основание a в диапазоне [2, n-2]
        /// </summary>
        private BigInteger GenerateRandomBase(BigInteger n)
        {
            byte[] bytes = n.ToByteArray();
            BigInteger result;

            do
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= 0x7F; // Обеспечиваем положительное число
                result = new BigInteger(bytes);
            }
            while (result < 2 || result >= n - 1);

            return result;
        }
    }
}