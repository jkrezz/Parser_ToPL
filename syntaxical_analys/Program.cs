﻿using System.Collections.Generic;
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

        string? updatedKey = key;

        string pattern = @"IF[\s\S]*?END_IF;";

        Match match = Regex.Match(key, pattern);

        string extractedBlock = string.Empty; // Переменная для сохранения блока
        
        // Проверка наличия условного оператора
        
        if (key.Contains("IF") || key.Contains("THEN") || key.Contains("ELSE") || key.Contains("END_IF"))
        {
            if (match.Success)
            {
                // Сохраняем найденный блок в переменную
                extractedBlock = match.Value;
                updatedKey = Regex.Replace(key, pattern, string.Empty, RegexOptions.Singleline);
            }
            else
            {
                _err = 1;
                Console.WriteLine("Неверное выражение");
                return;
            }
        }
        
        string[] exp0 = updatedKey.Split(";");

        string[] exp = new string[exp0.Length];
        Array.Copy(exp0, exp, exp0.Length);
        exp[exp.Length - 1] = extractedBlock.Trim();

        // BEGIN IF A.and.B THEN write(C); ELSE read(C); END_IF; END
        // BEGIN write(B); IF A.and.B THEN write(C); write(A); ELSE read(C); END_IF; A = A.and.C; END
        for (int i = 0; i < exp.Length; i++)
        {

            if (exp[i].Contains("IF") && exp[i].Contains("THEN") && exp[i].Contains("ELSE") && exp[i].Contains("END_IF"))
            {
                ProcessIf(exp[i], aa);
            }
            else if (exp[i].Contains("write"))
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
    private static void ProcessIf(string exp, string[] aa)
    {
        string variables = exp.Replace("IF", "").Trim();
        string[] parts1 = variables.Split(new string[] { "THEN" }, StringSplitOptions.None);
        string[] parts2 = parts1[1].Split(new string[] { "ELSE" }, StringSplitOptions.None);
        string[] parts3 = parts2[1].Split(new string[] { "END_" }, StringSplitOptions.None);
        
        string[] thenCommands = parts2[0].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries); // Без пустых строк в конце
        string[] elseCommands = parts3[0].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        // parts1[0] - условие
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
        // До ELSE

        foreach (string part in thenCommands)
        {
            if (part.Contains("write"))
            {
                ProcessWrite(part, aa);
            }
            else if (part.Contains("read"))
            {
                ProcessRead(part, aa);
            }
            else if (part.Contains("="))
            {
                ProcessLogic(part, aa);
            }    
            else
            {
                Console.WriteLine("Блок THEN задан некорректно");
                _err = 1;
                return;
            }
        }
        // До END_IF
        foreach (string part in elseCommands)
        {
            if (part.Contains("write"))
            {
                ProcessWrite(part, aa);
            }
            else if (part.Contains("read"))
            {
                ProcessRead(part, aa);
            }
            else if (part.Contains("="))
            {
                ProcessLogic(part, aa);
            }
            else
            {
                Console.WriteLine("Блок ELSE задан некорректно");
                _err = 1;
                return;
            }
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


