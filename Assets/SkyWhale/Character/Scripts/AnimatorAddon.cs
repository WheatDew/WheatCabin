using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorAddon : MonoBehaviour
{
    /*
    0 GhostSamurai_Common_StrafeWalkB_Root
    1 GhostSamurai_Common_Idle_Root
    2 GhostSamurai_Common_Walk_Start_Root
    3 GhostSamurai_Common_Walk_Loop_Root
    4 GhostSamurai_Common_Run_Loop_Root
    5 GhostSamurai_Common_Run_Start_Root
    6 GhostSamurai_Common_Run_End_Root
    7 GhostSamurai_Common_Run_End_Root
    8 GhostSamurai_Common_Run_Start_Root
    9 GhostSamurai_APose_Strafe_Walk_B_Root
    10 GhostSamurai_APose_Idle
    11 GhostSamurai_APose_Strafe_Walk_F_Start_Root
    12 GhostSamurai_APose_Strafe_Walk_F_Loop_Root
    13 GhostSamurai_Common_Run_Loop_Root
    14 GhostSamurai_APose_Equip01_Root
    15 GhostSamurai_APose_Unarm_3_Root
    16 GhostSamurai_APose_Attack01_1_ALL_Root
    17 GhostSamurai_APose_Attack01_2_Root
    18 GhostSamurai_APose_Attack01_3_Root
    19 GhostSamurai_APose_Attack01_4_Root
     */


    #region 变量定义
    private Animator animator;
    private AnimationClip[] clips;
    private Entity self;

    List<Property> animationEventDatas = new List<Property>();
    List<Property> animationPersistEventDatas = new List<Property>();

    #endregion

    private string animationEventKey = "AnimationEvent";
    private string animationPersistEventKey = "AnimationPersistEvent";


    #region 系统函数
    private void Start()
    {
        self = GetComponent<Entity>();
        animator = this.GetComponent<Animator>();
        clips = animator.runtimeAnimatorController.animationClips;

        if (self.data.ContainsKey(animationEventKey))
        {
            Debug.Log("初始化动画触发事件");

            var list = self.data.GetData(animationEventKey).GetDatas();

            for(int i = 0; i < list.Count; i++)
            {
                int index = i;
                animationEventDatas.Add(list[index].GetData());
                AddAnimationEvent(index);
            }

        }

        if (self.data.ContainsKey(animationPersistEventKey))
        {
            Debug.Log("初始化动画持续事件");

            var list = self.data.GetData(animationPersistEventKey).GetDatas();

            for (int i = 0; i < list.Count; i++)
            {
                int index = i;
                Property originData = list[index].GetData().Add(-1);

                animationPersistEventDatas.Add((new Property(originData)).Set(6, 0));
                AddAnimationPersistEvent(index*3);
                animationPersistEventDatas.Add((new Property(originData)).Set(6, 1));
                AddAnimationPersistEvent(index*3+1);
                animationPersistEventDatas.Add((new Property(originData)).Set(6, 2));
                AddAnimationPersistEvent(index*3+2);

            }

        }

    }



    private void OnDestroy()
    {
        CleanAllEvent();
    }
    #endregion

    #region --自定义函数



    public void TriggerEvent(int i)
    {
        FunctionMap.map[animationEventDatas[i].GetString(0)](animationEventDatas[i].GetData(1));
    }

    public void TriggerPersistEvent(int i)
    {
        FunctionMap.map[animationEventDatas[i].GetString(0)](animationEventDatas[i].GetData(1));
    }


    public void AddAnimationEvent(int index)
    {
        string clipName;
        float time;

        clipName = animationEventDatas[index].GetString(2);
        time = animationEventDatas[index].GetFloat(3);

        AnimationClip[] _clips = animator.runtimeAnimatorController.animationClips;

        for (int i = 0; i < _clips.Length; i++)
        {
            if (_clips[i].name == clipName)
            {
                AddAnimationEvent(_clips[i], time, index);
                break;
            }
        }
        animator.Rebind();
    }

    public void AddAnimationPersistEvent(int index)
    {

        string clipName;
        float startTime, endTime;
        float interpolation;

        clipName = animationEventDatas[index].GetString(2);
        startTime = animationEventDatas[index].GetFloat(3);
        endTime = animationEventDatas[index].GetFloat(4);
        interpolation = animationEventDatas[index].GetFloat(5);

        AnimationClip[] _clips = animator.runtimeAnimatorController.animationClips;

        for (int i = 0; i < _clips.Length; i++)
        {
            if (_clips[i].name == clipName)
            {
                int timer = 0;
                for (float time = startTime+interpolation; time < endTime; time += interpolation)
                {
                    if(time==startTime+interpolation)
                    {
                        AddAnimationEvent(_clips[i], time, index);
                    }
                    AddAnimationEvent(_clips[i], time, index);
                    timer++;
                    if (timer > 50)
                    {
                        Debug.LogError("插值次数过多");
                        break;
                    }
                }
                break;
            }
        }
        animator.Rebind();
    }

    public void AddAnimationEvent(AnimationClip clip,float time,int param)
    {
        AnimationEvent _event = new AnimationEvent();
        _event.functionName = "TriggerEvent";
        _event.intParameter = param;
        _event.time = clip.length * time;
        clip.AddEvent(_event);
    }

    public void AddPersistAnimationEvent(AnimationClip clip, float time, int param)
    {
        AnimationEvent _event = new AnimationEvent();
        _event.functionName = "AddAnimationPersistEvent";
        _event.intParameter = param;
        _event.time = clip.length * time;
        clip.AddEvent(_event);
    }

    /// <summary>
    /// 添加动画事件
    /// </summary>
    /// <param name="_animator"></param>
    /// <param name="_clipName">动画名称</param>
    /// <param name="_eventFunctionName">事件方法名称</param>
    /// <param name="_time">添加事件时间。单位：秒</param>
    private void AddAnimationEvent(Animator _animator, string _clipName, string _eventFunctionName, float _time)
    {
        AnimationClip[] _clips = _animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < _clips.Length; i++)
        {
            if (_clips[i].name == _clipName)
            {
                AnimationEvent _event = new AnimationEvent();
                _event.functionName = _eventFunctionName;
                _event.time = _time;
                _clips[i].AddEvent(_event);
                break;
            }
        }
        _animator.Rebind();
    }
    /// <summary>
    /// 清除所有事件
    /// </summary>
    private void CleanAllEvent()
    {
        for (int i = 0; i < clips.Length; i++)
        {
            clips[i].events = default(AnimationEvent[]);
        }
        Debug.Log("清除所有事件");
    }
    #endregion
}
