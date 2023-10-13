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

        // 创建 Stopwatch 对象
        Stopwatch stopwatch = new Stopwatch();

        // 开始计时
        stopwatch.Start();
        int ri;
        NyaList list = new NyaList();
        //NyaExpression ep=new NyaExpression()
        // 在这里执行需要测量性能的代码块
        for (int i = 0; i < 100; i++)
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
