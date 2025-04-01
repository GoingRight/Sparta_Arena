using System;
using System.Collections;
using UnityEngine;

namespace Akasha
{
    public static class RxTimer
    {
        public static void Every(float interval, MonoBehaviour runner, Action callback)
        {
            runner.StartCoroutine(TimerCoroutine(interval, callback));
        }

        private static IEnumerator TimerCoroutine(float interval, Action callback)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                callback?.Invoke();
            }
        }
    }
}
