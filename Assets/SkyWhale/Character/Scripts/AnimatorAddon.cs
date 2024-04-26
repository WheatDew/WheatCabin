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


    #region ��������
    private Animator animator;
    private AnimationClip[] clips;
    private WDEntity self;

    List<INya> animationEventDatas = new List<INya>();

    #endregion

    private string animationEventKey = "AnimationEvent";


    #region ϵͳ����
    private void Start()
    {
        self = GetComponent<WDEntity>();
        animator = this.GetComponent<Animator>();
        clips = animator.runtimeAnimatorController.animationClips;

        Debug.Log("��ʼ��AnimationAddon");
        if (self.data.Map.ContainsKey(animationEventKey))
        {

            var list = self.data.Map[animationEventKey].List;

            for(int i = 0; i < list.Count; i++)
            {
                int index = i;
                animationEventDatas.Add(list[index].Data);
                AddAnimationEvent(index);
            }

        }
    }



    private void OnDestroy()
    {
        CleanAllEvent();
    }
    #endregion

    #region --�Զ��庯��



    public void TriggerEvent(int i)
    {
        FunctionMap.map[animationEventDatas[i].List[1].String](animationEventDatas[i].List[2].Data);
    }


    public void AddAnimationEvent(int index)
    {
        string clipName;
        float[] time;

        clipName = animationEventDatas[index].List[0].String;
        time = new float[animationEventDatas[index].List.Count-3];
        for(int i = 0,j=3; j < animationEventDatas[index].List.Count; i++,j++)
        {
            time[i] = animationEventDatas[index].List[j].Float;

        }

        AnimationClip[] _clips = animator.runtimeAnimatorController.animationClips;

        for (int i = 0; i < _clips.Length; i++)
        {
            if (_clips[i].name == clipName)
            {
                for(int n = 0; n < time.Length; n++)
                {
                    AddAnimationEvent(_clips[i], time[n], index);
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


    /// <summary>
    /// ��Ӷ����¼�
    /// </summary>
    /// <param name="_animator"></param>
    /// <param name="_clipName">��������</param>
    /// <param name="_eventFunctionName">�¼���������</param>
    /// <param name="_time">����¼�ʱ�䡣��λ����</param>
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
    /// ��������¼�
    /// </summary>
    private void CleanAllEvent()
    {
        for (int i = 0; i < clips.Length; i++)
        {
            clips[i].events = default(AnimationEvent[]);
        }
        Debug.Log("��������¼�");
    }
    #endregion
}
