using System;
using System.Collections.Generic;
using UnityEngine;


namespace Ex
{
    public static class ExAniClipEvent
    {
        public static void BindStart(Animator animator, string aniClipName, string functionName)
        {
            Bind(animator, aniClipName, 0, functionName);
        }

        public static void BindEnd(Animator animator, string aniClipName, string functionName)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (string.Equals(clips[i].name, aniClipName))
                {
                    BindImplement(clips[i], clips[i].length, functionName);
                    break;
                }
            }
            animator.Rebind();
        }


        public static void Bind(Animator animator, string aniClipName, float time, string functionName)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (string.Equals(clips[i].name, aniClipName))
                {
                    BindImplement(clips[i], time, functionName);
                    break;
                }
            }

            animator.Rebind();
        }

        private static void BindImplement(AnimationClip aniClip, float time, string functionName)
        {
            AnimationEvent events = new AnimationEvent();
            events.functionName = functionName;
            events.time = time;
            aniClip.AddEvent(events);
        }

        public static void UnBind(Animator animator, string aniClipName, string functionName)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (string.Equals(clips[i].name, aniClipName))
                {
                    AnimationEvent[] events = clips[i].events;
                    List<AnimationEvent> newEvents = new List<AnimationEvent>();
                    foreach (AnimationEvent evt in events)
                    {
                        if (evt.functionName != functionName)
                        {
                            newEvents.Add(evt);
                        }
                    }

                    clips[i].events = newEvents.ToArray();
                    break;
                }
            }

            animator.Rebind();
        }

        public static void UnBindAll(Animator animator, string aniClipName)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (string.Equals(clips[i].name, aniClipName))
                {
                    clips[i].events = null;
                    break;
                }
            }

            animator.Rebind();
        }
    }
}