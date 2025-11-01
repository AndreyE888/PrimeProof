using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using PrimeProof.Models.ViewModels;
using PrimeProof.Services.Interfaces;
using PrimeProof.Services.Implementations;

namespace PrimeProof.Services
{
    /// <summary>
    /// Сервис для запуска и управления тестами простоты
    /// </summary>
    public class TestRunnerService
    {
        private readonly Dictionary<string, IPrimalityTest> _tests;
        private readonly Stopwatch _stopwatch;

        public TestRunnerService()
        {
            _stopwatch = new Stopwatch();
            _tests = new Dictionary<string, IPrimalityTest>
            {
                ["trial"] = new TrialDivisionTest(),
                ["fermat"] = new FermatTest(),
                ["miller-rabin"] = new MillerRabinTest(),
                ["aks"] = new AKSTest()
            };
        }

        /// <summary>
        /// Запускает один конкретный тест
        /// </summary>
        public TestResultViewModel RunTest(string testType, BigInteger number, int rounds)
        {
            if (!_tests.ContainsKey(testType))
            {
                throw new ArgumentException($"Неизвестный тип теста: {testType}");
            }

            var test = _tests[testType];
            
            // Проверяем применимость теста
            if (!test.IsApplicable(number))
            {
                return new TestResultViewModel
                {
                    Number = number.ToString(),
                    IsPrime = false,
                    TestName = test.TestName,
                    ExecutionTime = TimeSpan.Zero,
                    Message = $"Тест не применим к числу {number}",
                    Probability = 0
                };
            }

            _stopwatch.Restart();
            var result = test.IsPrime(number, rounds, out var details);
            _stopwatch.Stop();

            return new TestResultViewModel
            {
                Number = number.ToString(),
                IsPrime = result,
                TestName = test.TestName,
                ExecutionTime = _stopwatch.Elapsed,
                Iterations = rounds,
                Details = details,
                Probability = test.GetProbability(rounds),
                Message = result ? "Тест пройден успешно" : "Найдено свидетельство составности"
            };
        }

        /// <summary>
        /// Запускает все тесты для сравнения
        /// </summary>
        public TestComparisonViewModel RunAllTests(BigInteger number, int rounds)
        {
            var comparison = new TestComparisonViewModel 
            { 
                Number = number.ToString() 
            };

            var totalStopwatch = Stopwatch.StartNew();

            foreach (var test in _tests)
            {
                try
                {
                    var result = RunTest(test.Key, number, rounds);
                    comparison.Results.Add(result);
                }
                catch (Exception ex)
                {
                    comparison.Results.Add(new TestResultViewModel
                    {
                        Number = number.ToString(),
                        IsPrime = false,
                        TestName = test.Value.TestName,
                        ExecutionTime = TimeSpan.Zero,
                        Message = $"Ошибка выполнения: {ex.Message}",
                        Probability = 0
                    });
                }
            }

            totalStopwatch.Stop();
            comparison.TotalExecutionTime = totalStopwatch.Elapsed;

            return comparison;
        }

        /// <summary>
        /// Получает информацию о всех доступных тестах
        /// </summary>
        public List<TestInfo> GetAvailableTests()
        {
            return _tests.Select(t => new TestInfo
            {
                Id = t.Key,
                Name = t.Value.TestName,
                Description = t.Value.TestDescription,
                IsDeterministic = t.Value.IsDeterministic
            }).ToList();
        }

        /// <summary>
        /// Проверяет, поддерживается ли указанный тест
        /// </summary>
        public bool IsTestSupported(string testType)
        {
            return _tests.ContainsKey(testType);
        }

        /// <summary>
        /// Получает рекомендуемое количество раундов для теста
        /// </summary>
        public int GetRecommendedRounds(string testType)
        {
            return testType switch
            {
                "trial" => 1,    // Детерминированный
                "aks" => 1,      // Детерминированный
                "fermat" => 20,  // Вероятностный
                "miller-rabin" => 40, // Вероятностный (высокая надежность)
                _ => 10
            };
        }
    }

    /// <summary>
    /// Информация о тесте
    /// </summary>
    public class TestInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeterministic { get; set; }
    }
}