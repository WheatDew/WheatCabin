using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NyaCalc
{
    
}

public interface INyaExpression
{
    
}

public class NyaExpression:INyaExpression
{
    UnityAction<INya> ua;
    float result;
    int dataIndex = 0;
    public NyaExpression(string expression)
    {
        ua += delegate (INya nya) { result = nya.List[0].Float; };
        for (int i = 0; i < expression.Length-1; i++)
        {
            char token = expression[i];
            
            if (char.IsWhiteSpace(token))
            {
                continue; // 忽略空格
            }
            else if (char.IsLetter(token))
            {
                dataIndex++;
                continue;
            }
            else if (IsOperator(token))
            {
                AddAction(token);
            }
        }
        

            
    }

    private bool IsOperator(char c)
    {
        return c == '+' || c == '-' || c == '*' || c == '/';
    }

    private void AddAction(char c)
    {
        Debug.LogFormat("{0} {1}",dataIndex,c);
        switch (c)
        {
            case '+':
                Debug.Log("添加加号运算");
                ua += delegate (INya nya) { int index = dataIndex; result += nya.List[index].Float; };
                break;
            case '-':
                Debug.Log("添加减号运算");
                ua += delegate (INya nya) { int index = dataIndex; result -= nya.List[index].Float; };
                break;
        }
    }
    public float Calculate(INya data)
    {

        result = 0;
        ua(data);
        return result;
    }
}

public class SimpleExpressionParser
{
    private static Dictionary<char, int> precedence = new Dictionary<char, int>
    {
        { '+', 1 },
        { '-', 1 },
        { '*', 2 },
        { '/', 2 }
    };

    public static double Parse(string expression, Dictionary<char, double> variables)
    {
        Stack<double> valueStack = new Stack<double>();
        Stack<char> operatorStack = new Stack<char>();

        for (int i = 0; i < expression.Length; i++)
        {
            char token = expression[i];

            if (char.IsWhiteSpace(token))
            {
                continue; // 忽略空格
            }
            else if (char.IsLetter(token))
            {
                // 如果标记是字母，表示变量
                string variable = token.ToString();
                while (i + 1 < expression.Length && char.IsLetterOrDigit(expression[i + 1]))
                {
                    i++;
                    variable += expression[i];
                }

                if (variables.ContainsKey(variable[0]))
                {
                    valueStack.Push(variables[variable[0]]);
                }
                else
                {
                    throw new ArgumentException("未定义的变量: " + variable);
                }
            }
            else if (IsOperator(token))
            {
                while (operatorStack.Count > 0 && IsOperator(operatorStack.Peek()) &&
                       precedence[operatorStack.Peek()] >= precedence[token])
                {
                    PerformOperation(operatorStack.Pop(), valueStack);
                }
                operatorStack.Push(token);
            }
        }

        while (operatorStack.Count > 0)
        {
            PerformOperation(operatorStack.Pop(), valueStack);
        }

        if (valueStack.Count != 1)
        {
            throw new ArgumentException("表达式格式不正确");
        }

        return valueStack.Pop();
    }

    private static bool IsOperator(char c)
    {
        return c == '+' || c == '-' || c == '*' || c == '/';
    }

    private static void PerformOperation(char operation, Stack<double> values)
    {
        if (values.Count < 2)
        {
            throw new ArgumentException("表达式格式不正确");
        }

        double b = values.Pop();
        double a = values.Pop();

        double result;
        switch (operation)
        {
            case '+':
                result = a + b;
                break;
            case '-':
                result = a - b;
                break;
            case '*':
                result = a * b;
                break;
            case '/':
                if (b == 0)
                {
                    throw new DivideByZeroException("除数为零");
                }
                result = a / b;
                break;
            default:
                throw new ArgumentException("不支持的操作符: " + operation);
        }

        values.Push(result);
    }
}