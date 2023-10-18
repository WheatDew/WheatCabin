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
                // ���ƥ�䵽���֣��滻Ϊ��ĸ
                inputString = inputString.Replace(matchedText, currentLetter.ToString());
            }
            else
            {
                // ���ƥ�䵽�������ڵ��ı���Ҳ�滻Ϊ��ĸ
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

        // ���� Stopwatch ����
        Stopwatch stopwatch = new Stopwatch();

        // ��ʼ��ʱ
        stopwatch.Start();
        int ri;
        NyaList list = new NyaList();
        //NyaExpression ep=new NyaExpression()
        // ������ִ����Ҫ�������ܵĴ����
        for (int i = 0; i < 10000; i++)
        {
            
        }

        // ֹͣ��ʱ
        stopwatch.Stop();

        // ��ȡ������ʱ��
        TimeSpan elapsed = stopwatch.Elapsed;

        // ���������ʱ�䣨���룩
        print("Elapsed time in milliseconds: " + elapsed.TotalMilliseconds);

        // ���������ʱ�䣨���룩
        print("Elapsed time in ticks: " + elapsed.Ticks);
    }
}
