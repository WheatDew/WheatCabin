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

        // 创建 Stopwatch 对象
        Stopwatch stopwatch = new Stopwatch();

        // 开始计时
        stopwatch.Start();
        NyaList list = new NyaList();

        //NyaExpression ep=new NyaExpression()
        // 在这里执行需要测量性能的代码块
        for (int i = 0; i < 1000000; i++)
        {
             float dis = Vector3.Distance(new Vector3(10, 20, 30), new Vector3(40, 10, 20));
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
