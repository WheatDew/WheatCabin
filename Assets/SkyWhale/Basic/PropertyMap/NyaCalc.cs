using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    UnityAction ua;
    int resultIndex = -1;
    INya expression;
    List<NyaPlaceholder> paramers;
    public NyaExpression(string expression)
    {
        this.expression = new NyaList();
        paramers = new List<NyaPlaceholder>();
        for (int i = 0; i < expression.Length; i++)
        {
            char token = expression[i];

            if (char.IsWhiteSpace(token))
            {
                continue; // 忽略空格
            }
            else if (char.IsLetter(token))
            {
                var placeholder = new NyaPlaceholder(token.ToString());
                this.expression.Add(placeholder);
                paramers.Add(placeholder);
            }
            else if (IsOperator(token))
            {
                this.expression.Add(new NyaPlaceholder(token.ToString()));
            }
        }
        for (int p = 5; p >= 0; p--)
        {
            for (int i = 0; i < this.expression.List.Count; i++)
            {
                if (IsOperator(this.expression.List[i].Placeholder))
                {
                    int priority = -1;
                    if (IsOperator(this.expression.List[i].Placeholder, out priority) && priority == p)
                    {
                        int leftIndex = i - 1;
                        int rightIndex = i + 1;
                        while (leftIndex>=0)
                        {
                            if (this.expression.List[leftIndex].Placeholder != "#")
                                break;
                            else
                                leftIndex--;
                        }
                        while (rightIndex <= this.expression.List.Count)
                        {
                            Debug.LogFormat("{0} {1}", rightIndex, this.expression.List.Count);
                            if (this.expression.List[rightIndex].Placeholder != "#")
                                break;
                            else
                                rightIndex++;
                        }
                        AddAction(this.expression.List[i].Placeholder, i, leftIndex, rightIndex);
                        this.expression.List[leftIndex].Placeholder = "#";
                        this.expression.List[rightIndex].Placeholder = "#";
                    }
                }

            }

        }
        for(int i = 0; i < this.expression.List.Count; i++)
        {
            if (this.expression.List[i].Placeholder != "#")
            {
                resultIndex = i;
                break;
            }
        }
    }

    private bool IsOperator(char c)
    {
        return c == '+' || c == '-' || c == '*' || c == '/';

    }
    private bool IsOperator(string c)
    {
        return c == "+" || c == "-" || c == "*" || c == "/";

    }
    private bool IsOperator(string s,out int priority)
    {
        priority = -1;
        if(s == "+" || s == "-")
        {
            priority = 0;
            return true;
        }
        else if(s == "*" || s == "/")
        {
            priority = 1;
            return true;
        }
        return false;

    }

    private void AddAction(string s,int currentIndex,int liftIndex,int rightIndex)
    {
        var ep = expression.List;
        switch (s)
        {
            case "+":
                ua += delegate { ep[currentIndex].Value = ep[liftIndex].Value + ep[rightIndex].Value; };
                break;
            case "-":
                ua += delegate { ep[currentIndex].Value = ep[liftIndex].Value - ep[rightIndex].Value; };
                break;
            case "*":
                ua += delegate { ep[currentIndex].Value = ep[liftIndex].Value * ep[rightIndex].Value; };
                break;
            case "/":
                ua += delegate { ep[currentIndex].Value = ep[liftIndex].Value / ep[rightIndex].Value; };
                break;
        }
        
    }
    public float Calculate(INya data)
    {
        for(int i = 0; i < data.List.Count; i++)
        {
            paramers[i].Value = data.List[i].Float;
        }
        ua();
        return expression.List[resultIndex].Value;
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