using org.mariuszgromada.math.mxparser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;

public class PerformanceTest : MonoBehaviour
{
    
    void Start()
    {
        string inputString = "{health}+20*3";
        string pattern = @"\{[^}]+\}|\d+";

        MatchCollection matches = Regex.Matches(inputString, pattern);

        char currentLetter = 'a';
        foreach (Match match in matches)
        {
            string matchedText = match.Value;
            if (Regex.IsMatch(matchedText, @"\d+"))
            {
                // 如果匹配到数字，替换为字母
                inputString = inputString.Replace(matchedText, currentLetter.ToString());
            }
            else
            {
                // 如果匹配到大括号内的文本，也替换为字母
                inputString = inputString.Replace(matchedText, currentLetter.ToString());
            }
            currentLetter++;
        }

        print(inputString);

        //NyaExpression expression = new NyaExpression("a+b*a");
        //NyaList nyaList = new NyaList();
        //nyaList.Add(new NyaFloat(2f));
        //nyaList.Add(new NyaFloat(4f));
        //print(expression.Calculate(nyaList));

        // 创建 Stopwatch 对象
        Stopwatch stopwatch = new Stopwatch();

        // 开始计时
        stopwatch.Start();
        int ri;
        NyaList list = new NyaList();
        //NyaExpression ep=new NyaExpression()
        // 在这里执行需要测量性能的代码块
        for (int i = 0; i < 10000; i++)
        {
            
        }

        // 停止计时
        stopwatch.Stop();

        // 获取经过的时间
        TimeSpan elapsed = stopwatch.Elapsed;

        // 输出经过的时间（毫秒）
        print("Elapsed time in milliseconds: " + elapsed.TotalMilliseconds);

        // 输出经过的时间（毫秒）
        print("Elapsed time in ticks: " + elapsed.Ticks);
    }
}
