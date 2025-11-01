using System;
using System.Numerics;
using Microsoft.AspNetCore.Mvc;
using PrimeProof.Models.ViewModels;
using PrimeProof.Services;

namespace PrimeProof.Controllers
{
    public class TestsController : Controller
    {
        private readonly TestRunnerService _testRunner;

        public TestsController()
        {
            _testRunner = new TestRunnerService();
        }

        /// <summary>
        /// Главная страница тестирования
        /// </summary>
        public IActionResult Index()
        {
            var model = new TestInputViewModel
            {
                Rounds = 10 // Значение по умолчанию
            };

            ViewBag.AvailableTests = _testRunner.GetAvailableTests();
            return View(model);
        }

        /// <summary>
        /// Запуск одного теста
        /// </summary>
        [HttpPost]
        public IActionResult SingleTest(TestInputViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AvailableTests = _testRunner.GetAvailableTests();
                return View("Index", model);
            }

            try
            {
                // Парсим число
                if (!BigInteger.TryParse(model.NumberToTest, out BigInteger number))
                {
                    ModelState.AddModelError("NumberToTest", "Неверный формат числа");
                    ViewBag.AvailableTests = _testRunner.GetAvailableTests();
                    return View("Index", model);
                }

                // Проверяем, что число положительное
                if (number <= 0)
                {
                    ModelState.AddModelError("NumberToTest", "Число должно быть положительным");
                    ViewBag.AvailableTests = _testRunner.GetAvailableTests();
                    return View("Index", model);
                }

                // Проверяем поддержку теста
                if (!_testRunner.IsTestSupported(model.SelectedTest))
                {
                    ModelState.AddModelError("SelectedTest", "Выбранный тест не поддерживается");
                    ViewBag.AvailableTests = _testRunner.GetAvailableTests();
                    return View("Index", model);
                }

                // Запускаем тест
                var result = _testRunner.RunTest(model.SelectedTest, number, model.Rounds);
                
                // Создаем модель сравнения с одним результатом для единообразного отображения
                var comparisonModel = new TestComparisonViewModel 
                { 
                    Number = model.NumberToTest,
                    Results = { result }
                };

                return View("Results", comparisonModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при выполнении теста: {ex.Message}");
                ViewBag.AvailableTests = _testRunner.GetAvailableTests();
                return View("Index", model);
            }
        }

        /// <summary>
        /// Сравнение всех тестов
        /// </summary>
        [HttpPost]
        public IActionResult CompareTests(TestInputViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AvailableTests = _testRunner.GetAvailableTests();
                return View("Index", model);
            }

            try
            {
                // Парсим число
                if (!BigInteger.TryParse(model.NumberToTest, out BigInteger number))
                {
                    ModelState.AddModelError("NumberToTest", "Неверный формат числа");
                    ViewBag.AvailableTests = _testRunner.GetAvailableTests();
                    return View("Index", model);
                }

                // Проверяем, что число положительное
                if (number <= 0)
                {
                    ModelState.AddModelError("NumberToTest", "Число должно быть положительным");
                    ViewBag.AvailableTests = _testRunner.GetAvailableTests();
                    return View("Index", model);
                }

                // Запускаем все тесты
                var results = _testRunner.RunAllTests(number, model.Rounds);
                return View("Results", results);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при сравнении тестов: {ex.Message}");
                ViewBag.AvailableTests = _testRunner.GetAvailableTests();
                return View("Index", model);
            }
        }

        /// <summary>
        /// Страница с информацией о тестах
        /// </summary>
        public IActionResult About()
        {
            ViewBag.AvailableTests = _testRunner.GetAvailableTests();
            return View();
        }

        /// <summary>
        /// API метод для быстрой проверки (опционально)
        /// </summary>
        [HttpGet]
        public JsonResult QuickCheck(string number, string testType = "miller-rabin")
        {
            try
            {
                if (string.IsNullOrEmpty(number) || !BigInteger.TryParse(number, out BigInteger n))
                {
                    return Json(new { success = false, error = "Неверный формат числа" });
                }

                if (n <= 0)
                {
                    return Json(new { success = false, error = "Число должно быть положительным" });
                }

                if (!_testRunner.IsTestSupported(testType))
                {
                    return Json(new { success = false, error = "Тест не поддерживается" });
                }

                int rounds = _testRunner.GetRecommendedRounds(testType);
                var result = _testRunner.RunTest(testType, n, rounds);

                return Json(new 
                { 
                    success = true,
                    number = result.Number,
                    isPrime = result.IsPrime,
                    testName = result.TestName,
                    executionTime = result.FormattedExecutionTime,
                    probability = result.FormattedProbability
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}