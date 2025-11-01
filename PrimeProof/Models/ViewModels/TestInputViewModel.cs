using System.ComponentModel.DataAnnotations;

namespace PrimeProof.Models.ViewModels
{
    /// <summary>
    /// Модель для ввода данных тестирования простоты числа
    /// </summary>
    public class TestInputViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите число для проверки")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Введите целое положительное число")]
        [Display(Name = "Число для проверки")]
        public string NumberToTest { get; set; }

        [Range(1, 100, ErrorMessage = "Количество раундов должно быть от 1 до 100")]
        [Display(Name = "Количество раундов")]
        public int Rounds { get; set; } = 10;

        [Required(ErrorMessage = "Пожалуйста, выберите тест")]
        [Display(Name = "Выбранный тест")]
        public string SelectedTest { get; set; }

        [Display(Name = "Сравнить все тесты")]
        public bool CompareAll { get; set; }
    }
}