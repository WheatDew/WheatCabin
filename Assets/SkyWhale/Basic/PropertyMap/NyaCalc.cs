using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class NyaCalc
{
    public static Dictionary<string, int> pairs = new Dictionary<string, int> { { "a", 0 }, { "b", 1 }, { "c", 2 }, { "d", 3 } };
    public static HashSet<char> endToken = new HashSet<char> { '+', '-', '*', '/' };
    public static Dictionary<string, NyaExpression> map = new Dictionary<string, NyaExpression>();
    public static void Add(string name,string expression)
    {
        map.Add(name, new NyaExpression(expression));
    }
    public static void Calculate(string expressionName,INya data,int offset)
    {
        map[expressionName].Calculate(data);
    }

    public static bool IsEndToken(char c)
    {
        return endToken.Contains(c);
    }
    public static int GetPairId(string s)
    {
        return pairs[s];
    }
}



public class NyaExpression
{
    UnityAction ua;
    int resultIndex = -1;
    INya expression;

    List<NyaPlaceholder> paramers;
    List<int> paramersSequence;

    public NyaExpression(string expression)
    {
        this.expression = new NyaList();
        paramers = new List<NyaPlaceholder>();
        paramersSequence = new List<int>();
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
                paramersSequence.Add(NyaCalc.GetPairId(placeholder.Placeholder));
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
    public float Calculate(INya data,int offset=0)
    {
        for(int i = 0; i < paramers.Count; i++)
        {
            paramers[i].Value = data.List[paramersSequence[i] +offset].Float;
        }
        ua();
        Debug.Log("计算后的值为" + expression.List[resultIndex].Value);
        LogSystem.s.WriteLog("计算后的值为" + expression.List[resultIndex].Value);
        return expression.List[resultIndex].Value;
    }
}
