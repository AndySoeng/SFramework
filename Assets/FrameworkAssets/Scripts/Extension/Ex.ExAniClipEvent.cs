using System;
using System.Collections.Generic;
using UnityEngine;


namespace Ex
{
    public static class ExAniClipEvent
    {
        public static void Add(Animator animator, string aniClipName, float time, string functionName, string stringParameter = null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (string.Equals(clips[i].name, aniClipName))
                {
                    AnimationEvent events = new AnimationEvent();
                    events.functionName = functionName;
                    events.time = time;
                    //events.stringParameter = string.IsNullOrEmpty(stringParameter) ? String.Empty : stringParameter;
                    clips[i].AddEvent(events);
                    break;
                }
            }

            animator.Rebind();
        }

        public static void Remove(Animator animator, string aniClipName, string functionName)
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

        public static void RemoveAll(Animator animator, string aniClipName)
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