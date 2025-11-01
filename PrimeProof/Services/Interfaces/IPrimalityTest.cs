using System.Collections.Generic;
using System.Numerics;

namespace PrimeProof.Services.Interfaces
{
    /// <summary>
    /// Интерфейс для всех тестов простоты чисел
    /// </summary>
    public interface IPrimalityTest
    {
        /// <summary>
        /// Название теста
        /// </summary>
        string TestName { get; }
        
        /// <summary>
        /// Описание теста
        /// </summary>
        string TestDescription { get; }
        
        /// <summary>
        /// Является ли тест детерминированным
        /// </summary>
        bool IsDeterministic { get; }
        
        /// <summary>
        /// Проверяет, является ли число простым
        /// </summary>
        /// <param name="number">Число для проверки</param>
        /// <param name="rounds">Количество раундов (для вероятностных тестов)</param>
        /// <param name="details">Детали выполнения теста</param>
        /// <returns>True если число вероятно простое, false если составное</returns>
        bool IsPrime(BigInteger number, int rounds, out List<string> details);
        
        /// <summary>
        /// Возвращает вероятность правильного результата
        /// </summary>
        /// <param name="rounds">Количество раундов</param>
        /// <returns>Вероятность от 0 до 1</returns>
        double GetProbability(int rounds);
        
        /// <summary>
        /// Проверяет, подходит ли число для данного теста
        /// </summary>
        /// <param name="number">Число для проверки</param>
        /// <returns>True если тест применим к числу</returns>
        bool IsApplicable(BigInteger number);
    }
}