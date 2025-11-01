namespace PrimeProof.Utilities
{
    /// <summary>
    /// Калькулятор вероятностей для тестов простоты
    /// </summary>
    public static class ProbabilityCalculator
    {
        /// <summary>
        /// Вычисляет вероятность ошибки для теста Ферма
        /// </summary>
        public static double FermatErrorProbability(int rounds)
        {
            // Для теста Ферма вероятность ошибки <= 1/2^k
            // Но на практике может быть хуже из-за чисел Кармайкла
            return Math.Pow(0.5, rounds);
        }

        /// <summary>
        /// Вычисляет вероятность ошибки для теста Миллера-Рабина
        /// </summary>
        public static double MillerRabinErrorProbability(int rounds)
        {
            // Для теста Миллера-Рабина вероятность ошибки <= 4^(-k)
            return Math.Pow(4, -rounds);
        }

        /// <summary>
        /// Вычисляет надежность теста в процентах
        /// </summary>
        public static double CalculateReliability(int rounds, string testType)
        {
            double errorProb = testType.ToLower() switch
            {
                "fermat" => FermatErrorProbability(rounds),
                "miller-rabin" => MillerRabinErrorProbability(rounds),
                _ => 0.0 // Детерминированные тесты
            };

            return (1 - errorProb) * 100;
        }

        /// <summary>
        /// Рекомендует количество раундов для достижения заданной надежности
        /// </summary>
        public static int RecommendRounds(double targetReliability, string testType)
        {
            if (testType.ToLower() == "trial" || testType.ToLower() == "aks")
                return 1; // Детерминированные тесты

            double targetError = (100 - targetReliability) / 100.0;
            int rounds = 1;

            while (true)
            {
                double currentError = testType.ToLower() switch
                {
                    "fermat" => FermatErrorProbability(rounds),
                    "miller-rabin" => MillerRabinErrorProbability(rounds),
                    _ => 0.0
                };

                if (currentError <= targetError)
                    return rounds;

                rounds++;
                
                // Защита от бесконечного цикла
                if (rounds > 1000)
                    return 100;
            }
        }

        /// <summary>
        /// Форматирует вероятность в читаемый вид
        /// </summary>
        public static string FormatProbability(double probability)
        {
            if (probability >= 0.99999)
                return "> 99.999%";

            if (probability >= 0.9999)
                return "> 99.99%";

            if (probability < 0.0001)
                return $"{(probability * 100):E2}%";

            return $"{(probability * 100):F4}%";
        }
    }
}