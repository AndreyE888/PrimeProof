using System;
using System.Collections.Generic;

namespace PrimeProof.Models.ViewModels
{
    /// <summary>
    /// Модель для отображения результатов тестирования простоты
    /// </summary>
    public class TestResultViewModel
    {
        public string Number { get; set; }
        public bool IsPrime { get; set; }
        public string TestName { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public int Iterations { get; set; }
        public List<string> Details { get; set; } = new List<string>();
        public double Probability { get; set; }
        public string Message { get; set; }

        // Вычисляемое свойство для отображения результата
        public string ResultText => IsPrime ? "ВЕРОЯТНО ПРОСТОЕ" : "СОСТАВНОЕ";
        
        // Вычисляемое свойство для CSS класса
        public string ResultClass => IsPrime ? "text-success" : "text-danger";
        
        // Форматированное время выполнения
        public string FormattedExecutionTime => $"{ExecutionTime.TotalMilliseconds:F4} мс";
        
        // Форматированная вероятность
        public string FormattedProbability => Probability >= 0.9999 ? "> 99.99%" : $"{Probability:P2}";
    }

    /// <summary>
    /// Модель для сравнения результатов нескольких тестов
    /// </summary>
    public class TestComparisonViewModel
    {
        public string Number { get; set; }
        public List<TestResultViewModel> Results { get; set; } = new List<TestResultViewModel>();
        public TimeSpan TotalExecutionTime { get; set; }

        // Вычисляемое свойство для общего времени
        public string FormattedTotalTime => $"{TotalExecutionTime.TotalMilliseconds:F4} мс";

        // Конструктор
        public TestComparisonViewModel()
        {
            Results = new List<TestResultViewModel>();
        }
    }
    
    
}