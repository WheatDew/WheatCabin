using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public enum AnimatorAddonEventType { Start,End,Trigger}
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

    private UnityEvent<PropertyData> startEvent = new UnityEvent<PropertyData>();
    private UnityEvent<PropertyData> endEvent = new UnityEvent<PropertyData>();
    private UnityEvent<PropertyData> triggerEvent = new UnityEvent<PropertyData>();
    #endregion


    #region 系统函数
    private void Start()
    {
        self = GetComponent<Entity>();
        animator = this.GetComponent<Animator>();
        clips = animator.runtimeAnimatorController.animationClips;

        if (self.propertyData.IsStringExist("TriggerEventAnimation"))
        {
            if (self.propertyData.IsStringExist("TriggerEventType"))
            {
                triggerEvent.AddListener(FunctionMap.s.map[self.propertyData.GetString("TriggerEventType")]);
            }

            if (self.propertyData.IsFloatExist("TriggerEventTime"))
            {
                AddAnimationEvent(self.propertyData.GetString("TriggerEventAnimation"), "TriggerEvent", (float)self.propertyData.GetFloat("TriggerEventTime"));
            }
            else
            {
                AddAnimationEvent(self.propertyData.GetString("TriggerEvent"), "TriggerEvent", 0);
            }
        }
    }



    private void OnDestroy()
    {
        CleanAllEvent();
    }
    #endregion

    #region --自定义函数

    public void StartEvent()
    {
        startEvent.Invoke(self.propertyData);
    }

    public void EndEvent()
    {
        endEvent.Invoke(self.propertyData);
    }

    public void TriggerEvent()
    {
        triggerEvent.Invoke(self.propertyData);
    }

    public void AddStartEvent(string clipName,float time, UnityAction<PropertyData> targetEvent)
    {
        AddAnimationEvent(clipName, "StartEvent", time);
    }

    public void AddEndEvent(string clipName,float time, UnityAction<PropertyData> targetEvent)
    {
        AddAnimationEvent(clipName, "EndEvent", time);
    }

    public void AddTriggerEvent(string clipName, float time, UnityAction<PropertyData> targetEvent)
    {
        AddAnimationEvent(clipName, "TriggerEvent", time);
    }

    public void AddAnimationEvent(string clipName,string eventFunctionName,float time)
    {
        AnimationClip[] _clips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < _clips.Length; i++)
        {
            if (_clips[i].name == clipName)
            {
                AnimationEvent _event = new AnimationEvent();
                _event.functionName = eventFunctionName;
                _event.time = _clips[i].length * time;
                _clips[i].AddEvent(_event);
                break;
            }
        }
        animator.Rebind();
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
