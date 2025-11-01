using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
            
            // ОСНОВНОЕ ИСПРАВЛЕНИЕ: Проверяем тривиальные случаи ДО выполнения теста
            var trivialResult = CheckTrivialCases(number);
            if (trivialResult.HasValue)
            {
                return CreateTrivialResult(test, number, trivialResult.Value);
            }

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
                    Probability = 0.0,
                    Iterations = 0
                };
            }

            _stopwatch.Restart();
            var result = test.IsPrime(number, rounds, out var details);
            _stopwatch.Stop();

            double probability = CalculateCorrectProbability(test, result, rounds, number);

            return new TestResultViewModel
            {
                Number = number.ToString(),
                IsPrime = result,
                TestName = test.TestName,
                ExecutionTime = _stopwatch.Elapsed,
                Iterations = result ? rounds : GetActualIterations(details),
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
            // Числа меньше 2 не являются простыми
            if (number < 2) return false;
            
            // 2 - простое число
            if (number == 2) return true;
            
            // Все четные числа больше 2 - составные
            if (number % 2 == 0) return false;
            
            // Для остальных случаев - нужны тесты
            return null;
        }

        /// <summary>
        /// Создает результат для тривиальных случаев
        /// </summary>
        private TestResultViewModel CreateTrivialResult(IPrimalityTest test, BigInteger number, bool isPrime)
        {
            string message = isPrime ? 
                "Тривиальный случай: простое число" : 
                "Тривиальный случай: составное число";

            // Для тривиальных случаев ВСЕ тесты дают 100% точность
            double probability = 100.0;

            // Для тривиальных случаев показываем 1 итерацию (минимальную)
            int iterations = 1;

            return new TestResultViewModel
            {
                Number = number.ToString(),
                IsPrime = isPrime,
                TestName = test.TestName,
                ExecutionTime = TimeSpan.Zero, // Тривиальные случаи выполняются мгновенно
                Message = message,
                Probability = probability,
                Iterations = iterations
            };
        }

        /// <summary>
        /// Правильно рассчитывает вероятность для разных типов тестов
        /// </summary>
        /// <summary>
        /// Правильно рассчитывает вероятность для разных типов тестов
        /// </summary>
        private double CalculateCorrectProbability(IPrimalityTest test, bool isPrime, int rounds, BigInteger number)
        {
            // Для детерминированных тестов
            if (test.IsDeterministic)
            {
                return 100.0; // Всегда 100% точность
            }

            // Для вероятностных тестов
            if (isPrime)
            {
                // Если тест говорит "простое" - используем confidence
                string testType = test.GetType().Name.ToLower().Replace("test", "");
                return ProbabilityCalculator.CalculateReliability(rounds, testType, isPrime);
            }
            else
            {
                // Если тест говорит "составное" - 100% уверенность
                return 100.0;
            }
        }

        private int GetActualIterations(List<string> details)
        {
            if (details == null || details.Count == 0)
                return 1;

            var iterationDetail = details.FirstOrDefault(d => 
                d.Contains("итерация") || d.Contains("iteration") || d.Contains("раунд"));
            
            if (iterationDetail != null)
            {
                var numbers = System.Text.RegularExpressions.Regex.Matches(iterationDetail, @"\d+");
                if (numbers.Count > 0 && int.TryParse(numbers[0].Value, out int actualIterations))
                {
                    return actualIterations;
                }
            }

            return 1;
        }

        // Остальные методы без изменений...
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
                        Probability = 0,
                        Iterations = 0
                    });
                }
            }

            totalStopwatch.Stop();
            comparison.TotalExecutionTime = totalStopwatch.Elapsed;

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