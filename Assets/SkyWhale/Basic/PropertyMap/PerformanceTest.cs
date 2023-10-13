using org.mariuszgromada.math.mxparser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PerformanceTest : MonoBehaviour
{
    void Start()
    {
        NyaExpression expression = new NyaExpression("a+b-c");
        NyaList nyaList = new NyaList();
        nyaList.Add(new NyaFloat(1.9f));
        nyaList.Add(new NyaFloat(3.2f));
        nyaList.Add(new NyaFloat(3.8f));
        print(expression.Calculate(nyaList));

        // ���� Stopwatch ����
        Stopwatch stopwatch = new Stopwatch();

        // ��ʼ��ʱ
        stopwatch.Start();
        int ri;
        NyaList list = new NyaList();
        //NyaExpression ep=new NyaExpression()
        // ������ִ����Ҫ�������ܵĴ����
        for (int i = 0; i < 100; i++)
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
