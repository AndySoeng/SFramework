using System;
using System.Collections;
using UnityEngine;


namespace Ex
{
    public static class ExCoroutine
    {
        public static IEnumerator WaitSecond(float second, Action callBack)
        {
            yield return new WaitForSeconds(second);
            callBack?.Invoke();
        }
    }
}
