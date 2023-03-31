using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace WorldSpaceTransitions
{
    public class WaitForJobCompleted : CustomYieldInstruction
    {
        private readonly bool _isUsingTempJobAllocator;
        private readonly int _forceJobCompletionFrameCount;
        private JobHandle _jobHandle;

        public override bool keepWaiting
        {
            get
            {
                if (_isUsingTempJobAllocator && Time.frameCount >= _forceJobCompletionFrameCount)
                    _jobHandle.Complete();
                if (_jobHandle.IsCompleted) _jobHandle.Complete();
                return _jobHandle.IsCompleted == false;
            }
        }

        //By default, job completion is forced after 4 frames to prevent TempJob allocators from complaining. 
        //Pass false as the second parameter if you have a long-running job with a Persistent allocator.
        public WaitForJobCompleted(JobHandle jobHandle, bool isUsingTempJobAllocator = true)
        {
            _jobHandle = jobHandle;
            _isUsingTempJobAllocator = isUsingTempJobAllocator;

            // force completion before running into native Allocator.TempJob's lifetime limit of 4 frames
            _forceJobCompletionFrameCount = Time.frameCount + 4;

        }
    }
}
