using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NyaCalc
{
    
}

public class NyaExpression
{
    float value;
    NyaList list;
    UnityEvent<NyaList> self;
    public NyaExpression(NyaList expression)
    {
        self = new UnityEvent<NyaList>();
        list = expression;
        for (int i = 0; i < expression.List.Count; i++)
        {
            if (expression.List[i].String == "+")
            {
                self.AddListener(delegate (NyaList nyaList)
                {
                    int index = i;
                    value += expression.List[index].Float;
                });
                continue;
            }
            else
            {
                value = expression.List[0].Float;
            }
        }
    }

    public NyaExpression(string expression)
    {
        string[] slices = expression.Split('[', ']');
        for(int i = 0; i < slices.Length; i++)
        {

        }

    }

    public float Calculate()
    {
        self.Invoke(list);
        return value;
    }
}