using System;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("Введите первое число (или 'e' для выхода):");
            string input = Console.ReadLine();

            if (input == "e") 
            {
                break;
            }
            double a = 0;
            double b = 0;

            try
            {
                a = Convert.ToDouble(input);
            }
            catch
            {
                Console.WriteLine("Не число");
                Console.WriteLine();
                continue;
            }

            Console.WriteLine("Введите второе число:");
            string secondInput = Console.ReadLine();

            try
            {
                b = Convert.ToDouble(secondInput);
            }
            catch
            {
                Console.WriteLine("Не число");
                Console.WriteLine();
                continue;
            }

            Console.WriteLine("Выберите операцию (+, -, *, /):");
            string op = Console.ReadLine();

            double result = 0;

            if (op == "+") 
            {
                result = a + b;
            }
            else if (op == "-") 
            {
                result = a - b;
            }
            else if (op == "*") 
            {
                result = a * b;
            }
            else if (op == "/") 
            {
                if (b == 0)
                {
                    Console.WriteLine("Деление на ноль");
                    Console.WriteLine();
                    continue;
                }
                result = a / b;
            }

            if (op == "+" || op == "-" || op == "*" || op == "/")
            {
                Console.WriteLine("Результат: " + result);
            }
            else
            {
                Console.WriteLine("Неверная операция");
            }
            Console.WriteLine();
        }
    }
}