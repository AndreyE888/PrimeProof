using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using PrimeProof.Models.ViewModels;
using PrimeProof.Services.Interfaces;
using PrimeProof.Services.Implementations;
using PrimeProof.Utilities;

namespace PrimeProof.Services
{
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

        public TestResultViewModel RunTest(string testType, BigInteger number, int rounds)
        {
            if (!_tests.ContainsKey(testType))
            {
                throw new ArgumentException($"Неизвестный тип теста: {testType}");
            }

            var test = _tests[testType];
            
            // Проверяем тривиальные случаи ДО выполнения теста
            var trivialResult = CheckTrivialCases(number);
            if (trivialResult.HasValue)
            {
                return CreateTrivialResult(test, number, trivialResult.Value, rounds);
            }

            // Проверяем применимость теста
            if (!test.IsApplicable(number))
            {
                return new TestResultViewModel
                {
                    Number = number.ToString(),
                    IsPrime = false,
                    TestName = test.TestName,
                    ExecutionTime = TimeSpan.FromMicroseconds(1),
                    Message = $"Тест не применим к числу {number}",
                    Probability = 0.0,
                    Iterations = 0
                };
            }

            // Измеряем время выполнения
            _stopwatch.Restart();
            var result = test.IsPrime(number, rounds, out var details);
            _stopwatch.Stop();

            // Гарантируем минимальное время для избежания 0,0000 мс
            TimeSpan executionTime = _stopwatch.Elapsed.TotalMicroseconds < 1 
                ? TimeSpan.FromMicroseconds(1) 
                : _stopwatch.Elapsed;

            double probability = CalculateCorrectProbability(test, result, rounds);
            int actualIterations = GetActualIterations(test, result, rounds, details);

            return new TestResultViewModel
            {
                Number = number.ToString(),
                IsPrime = result,
                TestName = test.TestName,
                ExecutionTime = executionTime,
                Iterations = actualIterations,
                Details = details,
                Probability = probability,
                Message = result ? "Тест пройден успешно" : "Найдено свидетельство составности"
            };
        }

        /// <summary>
        /// Проверяет тривиальные случаи (четные числа, маленькие числа)
        /// </summary>
        private bool? CheckTrivialCases(BigInteger number)
        {
            if (number < 2) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;
            return null;
        }

        /// <summary>
        /// Создает результат для тривиальных случаев
        /// </summary>
        private TestResultViewModel CreateTrivialResult(IPrimalityTest test, BigInteger number, bool isPrime, int requestedRounds)
        {
            string message = isPrime ? 
                "Тривиальный случай: простое число" : 
                "Тривиальный случай: составное число";

            int iterations = test.IsDeterministic ? 1 : 0;

            return new TestResultViewModel
            {
                Number = number.ToString(),
                IsPrime = isPrime,
                TestName = test.TestName,
                ExecutionTime = TimeSpan.FromMicroseconds(1),
                Message = message,
                Probability = 100.0,
                Iterations = iterations
            };
        }

        /// <summary>
        /// Правильно рассчитывает вероятность для разных типов тестов
        /// </summary>
        private double CalculateCorrectProbability(IPrimalityTest test, bool isPrime, int rounds)
        {
            // Для детерминированных тестов всегда 100%
            if (test.IsDeterministic)
            {
                return 100.0;
            }

            // Для вероятностных тестов
            if (isPrime)
            {
                // Если тест говорит "простое" - используем confidence
                string testType = test.GetType().Name.ToLower();
                if (testType.Contains("fermat"))
                    return (1 - ProbabilityCalculator.FermatErrorProbability(rounds)) * 100;
                else if (testType.Contains("miller"))
                    return (1 - ProbabilityCalculator.MillerRabinErrorProbability(rounds)) * 100;
                else
                    return 99.9;
            }
            else
            {
                // Если тест говорит "составное" - 100% уверенность
                return 100.0;
            }
        }

        /// <summary>
        /// Получает реальное количество выполненных итераций
        /// </summary>
        private int GetActualIterations(IPrimalityTest test, bool isPrime, int requestedRounds, List<string> details)
        {
            // Для детерминированных тестов всегда показываем 1 итерацию
            if (test.IsDeterministic)
            {
                return 1;
            }

            // Для вероятностных тестов, если нашли составное - ищем реальное количество
            if (!isPrime && details != null && details.Count > 0)
            {
                foreach (var detail in details)
                {
                    if (detail.Contains("итерация") || detail.Contains("раунд"))
                    {
                        var numbers = System.Text.RegularExpressions.Regex.Matches(detail, @"\d+");
                        if (numbers.Count > 0 && int.TryParse(numbers[0].Value, out int actualIterations))
                        {
                            return actualIterations;
                        }
                    }
                }
                return 1; // Если нашли составное быстро - минимум 1 итерация
            }

            // Для вероятностных тестов, которые прошли все раунды
            return requestedRounds;
        }

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
                        ExecutionTime = TimeSpan.FromMicroseconds(1),
                        Message = $"Ошибка выполнения: {ex.Message}",
                        Probability = 0,
                        Iterations = 0
                    });
                }
            }

            totalStopwatch.Stop();
            comparison.TotalExecutionTime = totalStopwatch.Elapsed.TotalMicroseconds < 1 
                ? TimeSpan.FromMicroseconds(1) 
                : totalStopwatch.Elapsed;

            return comparison;
        }

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

        public bool IsTestSupported(string testType)
        {
            return _tests.ContainsKey(testType);
        }

        public int GetRecommendedRounds(string testType)
        {
            return testType switch
            {
                "trial" => 1,
                "aks" => 1,
                "fermat" => 20,
                "miller-rabin" => 40,
                _ => 10
            };
        }
    }

    public class TestInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeterministic { get; set; }
    }
}