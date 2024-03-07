using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;

public class PerformanceTest : MonoBehaviour
{

    public void Start()
    {
        test();
    }

    void test()
    {

        // ���� Stopwatch ����
        Stopwatch stopwatch = new Stopwatch();

        // ��ʼ��ʱ
        stopwatch.Start();
        NyaList list = new NyaList();

        //NyaExpression ep=new NyaExpression()
        // ������ִ����Ҫ�������ܵĴ����
        for (int i = 0; i < 1000000; i++)
        {
             float dis = Vector3.Distance(new Vector3(10, 20, 30), new Vector3(40, 10, 20));
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
