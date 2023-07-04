namespace SFramework
{
    using UnityEngine;

    public class WaitForEndOfAnimationState : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                if (!animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName))
                {
                    return true;
                }
                else
                {
                    if (animator.GetCurrentAnimatorStateInfo(layer).normalizedTime <1 )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private Animator animator;
        private string stateName;
        private int layer;

        public WaitForEndOfAnimationState(Animator animator, string stateName, int layer)
        {
            this.animator = animator;
            this.stateName = stateName;
            this.layer = layer;
        }
    }
}