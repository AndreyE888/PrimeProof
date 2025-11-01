using System;
using System.Collections.Generic;
using System.Numerics;
using PrimeProof.Services.Interfaces;

namespace PrimeProof.Services.Implementations
{
    /// <summary>
    /// Метод пробных делений (детерминированный тест)
    /// Самый простой, но медленный метод проверки простоты
    /// </summary>
    public class TrialDivisionTest : IPrimalityTest
    {
        public string TestName => "Метод пробных делений";
        
        public string TestDescription => "Детерминированный тест, проверяющий делимость числа на все простые числа до его квадратного корня. Медленный, но гарантирует точный результат.";
        
        public bool IsDeterministic => true;

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

            details.Add($"Начинаем проверку делением на нечетные числа до √{number}");
            
            BigInteger limit = Sqrt(number);
            details.Add($"Проверяем делители до: {limit}");
            
            int iterations = 0;
            
            // Проверяем нечетные делители от 3 до limit
            for (BigInteger divisor = 3; divisor <= limit; divisor += 2)
            {
                iterations++;
                
                if (number % divisor == 0)
                {
                    details.Add($"Найден делитель: {divisor}");
                    details.Add($"Число составное! Количество проверок: {iterations}");
                    return false;
                }
                
                // Добавляем детали каждые 1000 итераций для больших чисел
                if (iterations % 1000 == 0)
                {
                    details.Add($"Проверено {iterations} делителей... текущий: {divisor}");
                }
            }
            
            details.Add($"Делители не найдены! Число простое.");
            details.Add($"Всего проверок: {iterations}");
            return true;
        }

        public double GetProbability(int rounds)
        {
            // Детерминированный тест - всегда 100%
            return 1.0;
        }

        public bool IsApplicable(BigInteger number)
        {
            // Метод применим ко всем положительным числам
            return number > 0;
        }

        /// <summary>
        /// Вычисляет квадратный корень из BigInteger
        /// </summary>
        private BigInteger Sqrt(BigInteger n)
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

        private bool IsSqrt(BigInteger n, BigInteger root)
        {
            BigInteger lowerBound = root * root;
            BigInteger upperBound = (root + 1) * (root + 1);
            return (n >= lowerBound && n < upperBound);
        }
    }
}