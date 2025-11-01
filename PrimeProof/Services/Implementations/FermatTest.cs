using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using PrimeProof.Services.Interfaces;

namespace PrimeProof.Services.Implementations
{
    /// <summary>
    /// Тест Ферма - вероятностный тест простоты
    /// Основан на Малой теореме Ферма
    /// </summary>
    public class FermatTest : IPrimalityTest
    {
        private static readonly Random random = new Random();

        public string TestName => "Тест Ферма";

        public string TestDescription => "Вероятностный тест, основанный на Малой теореме Ферма. Быстрый, но имеет числа-обманщики (числа Кармайкла).";

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

            details.Add($"Начинаем тест Ферма с {rounds} раундами");
            details.Add($"Основано на Малой теореме Ферма: если p простое, то a^(p-1) ≡ 1 (mod p) для 1 < a < p");

            // Известные небольшие числа Кармайкла для демонстрации
            BigInteger[] smallCarmichaelNumbers = { 561, 1105, 1729, 2465, 2821, 6601, 8911 };
            if (smallCarmichaelNumbers.Contains(number))
            {
                details.Add($"⚠️ ВНИМАНИЕ: {number} - известное число Кармайкла (может обмануть тест Ферма)");
            }

            for (int i = 0; i < rounds; i++)
            {
                // Генерируем случайное основание a в диапазоне [2, n-2]
                BigInteger a = GenerateRandomBase(number);
                details.Add($"\n--- Раунд {i + 1} ---");
                details.Add($"Проверяем с основанием a = {a}");

                // Вычисляем a^(n-1) mod n
                BigInteger result = BigInteger.ModPow(a, number - 1, number);
                details.Add($"Вычисляем {a}^({number}-1) mod {number} = {result}");

                if (result != 1)
                {
                    details.Add($"❌ Найдено свидетельство составности: {result} ≠ 1");
                    details.Add($"Число {number} - СОСТАВНОЕ");
                    details.Add($"Основание {a} является свидетелем Ферма");
                    return false;
                }
                else
                {
                    details.Add($"✓ Условие выполнено: {result} ≡ 1 (mod {number})");
                }
            }

            details.Add($"\n✅ Все {rounds} раундов пройдены успешно");
            details.Add($"Число {number} - ВЕРОЯТНО ПРОСТОЕ");
            details.Add($"⚠️ Предупреждение: возможны числа-обманщики (Кармайкла)");
            return true;
        }

        public double GetProbability(int rounds)
        {
            // Для теста Ферма вероятность ошибки <= 1/2^k
            // Но на практике может быть хуже из-за чисел Кармайкла
            return 1 - Math.Pow(0.5, rounds);
        }

        public bool IsApplicable(BigInteger number)
        {
            // Тест Ферма применим ко всем нечетным числам > 2
            return number > 2 && !number.IsEven;
        }

        /// <summary>
        /// Генерирует случайное основание a в диапазоне [2, n-2]
        /// </summary>
        private BigInteger GenerateRandomBase(BigInteger n)
        {
            // Генерируем случайное число в диапазоне [2, n-2]
            byte[] bytes = n.ToByteArray();
            BigInteger result;

            do
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= 0x7F; 
                result = new BigInteger(bytes);
            }
            while (result < 2 || result >= n - 1);

            return result;
        }
    }
}