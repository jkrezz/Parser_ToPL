using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;

internal class Program
{
    public static int _err;
    private static void Main(string[] args)
    {
        while (true)
        {
            _err = 0;
            check();
            Console.WriteLine();
        }
    }

    private static void check()
    {
        Console.WriteLine("Введите программу: \n");
        string? key = Console.ReadLine();
        if (key == null || !key.Split(" ")[0].Trim().Equals("VAR"))
        {
            Console.WriteLine("Не опознана команда инициализации переменных");
            return;
        }
        if (!key.Split(":")[1].Trim().Equals("LOGICAL;"))
        {
            Console.WriteLine("Неправильный тип переменной");
            return;
        }
        string[] tvp = key.Split(":")[0].Split(" ");
        string a = "";
        for (int i = 1; i < tvp.Length; i++)
        {
            a = a + tvp[i];
        }
        string[] aa = a.Trim().Split(",");
        for (int i = 0; i < aa.Length; i++)
        {
            for (int j = i + 1; j < aa.Length; j++)
            {
                if (aa[i].Equals(aa[j]))
                {
                    Console.WriteLine("Повторяющиеся названия переменных");
                    return;
                }
            }
            if (!IsLatinOnly(aa[i]))
            {
                Console.WriteLine("содержит не только латинские символы");
                return;
            }
            if (aa[i].Length >= 11)
            {
                Console.WriteLine("слишком длинный идентификатор");
                return;
            }
        }

        key = Console.ReadLine();
        if (key == null || key.Equals(""))
        {
            Console.WriteLine("Строка пуста");
            return;
        }

        // Проверка начала и конца блока BEGIN END
        string[] keys_tmp = key.Split(" ");
        if (!keys_tmp[0].Equals("BEGIN") || !keys_tmp[keys_tmp.Length - 1].Equals("END"))
        {
            Console.WriteLine("Неверное выражение");
            return;
        }

        // Извлечение содержимого между BEGIN и END
        key = "";
        for (int i = 1; i < keys_tmp.Length - 1; i++)
        {
            key = key + keys_tmp[i];
        }

        string[] exp = key.Split(";");
        for (int i = 0; i < exp.Length - 1; i++)
        {
            if (exp[i].Contains("IF") && exp[i].Contains("THEN") && exp[i + 1].Contains("ELSE") && exp[i + 2].Equals("END_IF"))
            {
                ProcessIf(exp[i], exp[i + 1], aa);
                i += 2;
            }
            
            if (exp[i].Contains("write"))
            {
                ProcessWrite(exp[i], aa);
            }
            else if (exp[i].Contains("read"))
            {
                ProcessRead(exp[i], aa);
            }
            else if (exp[i].Contains("="))
            {
                // Обработка логического выражения
                ProcessLogic(exp[i], aa);
            }
            else if (exp[i].Equals("END_IF"))
            {
                continue;
            }
            else
            {
                // Если команда не соответствует логическому выражению
                Console.WriteLine("Неизвестная команда или лишние слова: " + exp[i].Trim());
                _err = 1;
                return;
            }
        }
        if (_err == 1)
        {
            return;
        }
        Console.WriteLine("Выполнено успешно");
    }

    // BEGIN IF A.and.B THEN write(C); ELSE read(C); END_IF; END
    private static void ProcessIf(string exp, string exp2, string[] aa)
    {
        string variables = exp.Replace("IF", "").Trim();
        string[] parts1 = variables.Split(new string[] { "THEN" }, StringSplitOptions.None);

        // До THEN
        if ((parts1[0].Contains(".and.") || parts1[0].Contains(".or.") || parts1[0].Contains(".equ.") || parts1[0].Contains(".not.")) && !parts1[0].Contains("="))
        {
            // Обработка логического выражения
            ProcessLogic(parts1[0], aa);
        }
        else
        {
            Console.WriteLine("Блок IF задан некорректно");
            _err = 1;
            return;
        }
        string[] parts = new string[parts1.Length - 1];
        Array.Copy(parts1, 1, parts, 0, parts.Length);
        foreach (string part in parts)
        {
            if (part.Contains("write"))
            {
                ProcessWrite(part, aa);
            }
            else if (part.Contains("read"))
            {
                ProcessRead(part, aa);
            }
           
            else
            {
                Console.WriteLine("Блок THEN задан некорректно");
                _err = 1;
                return;
            }
        }
        
        // Блок ELSE

        string variables2 = exp2.Replace("ELSE", "").Trim();

        if (variables2.Contains("write"))
        {
            ProcessWrite(variables2, aa);
        }
        else if (variables2.Contains("read"))
        {
            ProcessRead(variables2, aa);
        }
        else if (variables2.Contains("="))
        {
            // Обработка логического выражения
            ProcessLogic(variables2, aa);
        }
        else
        {
            Console.WriteLine("Блок ELSE задан некорректно");
            _err = 1;
            return;
        }


    }
    private static void ProcessWrite(string exp, string[] aa)
    {
        // Проверка синтаксиса write(A, B, C)
        string variables = exp.Replace("write", "").Trim();
        variables = variables.Trim('(', ')');
        string[] vars = variables.Split(",");
        foreach (string var in vars)
        {
            string va = var.Trim();
            if (Array.Exists(aa, element => element == va))
            {
                continue;
            }
            else
            {
                Console.WriteLine($"Переменная {va} не определена");
                _err = 1;
                return;
            }
        }
    }

    private static void ProcessRead(string exp, string[] aa)
    {
        // Проверка синтаксиса read(A, B, C)
        string variables = exp.Replace("read", "").Trim();
        variables = variables.Trim('(', ')');
        string[] vars = variables.Split(",");
        foreach (string var in vars)
        {
            string trimmedVar = var.Trim();
            if (Array.Exists(aa, element => element == trimmedVar))
            {
                continue;
            }
            else
            {
                Console.WriteLine($"Переменная {trimmedVar} не определена");
                _err = 1;
                return;
            }
        }
    }

    private static void ProcessLogic(string exp, string[] aa)
    {
        // Обработка логических выражений
        string exp_tmp = exp.Replace("=", "").Trim();

        foreach (string var in aa)
        {
            exp_tmp = exp_tmp.Replace(var, "");
        }

        // Удаление логических операторов
        exp_tmp = exp_tmp.Replace(".and.", "").Replace(".or.", "").Replace(".equ.", "").Replace(".not.", "").Replace("0", "").Replace("1", "");
        exp_tmp = exp_tmp.Replace("(", "").Replace(")", "");

        if (!exp_tmp.Equals(""))
        {
            _err = 1;
            Console.WriteLine($"Переменная {exp_tmp} не определена");
            return;
        }
    }

    static bool IsLatinOnly(string input)
    {
        Regex regex = new Regex("^[a-zA-Z]+$");
        return regex.IsMatch(input);
    }
}


