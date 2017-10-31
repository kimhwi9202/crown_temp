using UnityEngine;
using System.Collections;
using System;
/*
 *  필요한 타임을 채크할때마다 선언해서 사용하는게 귀찮아서 만들었다.
 *  특징 : 필요한 많큼만 채크하고 불필요하게 타임 계산을 한지 않는다.
 *         이 함수는 Update() 함수 계열에서 사용해라..
 */
namespace xLIB
{
    public class xTimer
    {
        protected xTimer _instance = null;
        protected bool m_bActive = false;
        protected int m_iCount = 0;
        protected float m_fAccumTimeAterUpdate = 0;

        // 타임채크 활성여부..
        public void SetActive(bool _active)
        {
            m_bActive = _active;
            m_iCount = 0;
            m_fAccumTimeAterUpdate = 0;
        }

        // 주어진 카운터 만큼만 타임 채크..(0:무한채크)
        public bool Check(float _time, int _count = 0)
        {
            if (!m_bActive) return false;

            m_fAccumTimeAterUpdate += Time.deltaTime;
            if (m_fAccumTimeAterUpdate >= _time)
            {
                m_fAccumTimeAterUpdate = 0;
                if (_count == 0) return true;

                ++m_iCount;
                if (m_iCount == _count)
                {
                    m_bActive = false;
                    return true;
                }
            }
            return false;
        }
    }

// 2014 - Pixelnest Studio
    public class xTimer2
    {
        /// <summary>
        /// Simple timer, no reference, wait and then execute something
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IEnumerator Start(float duration, Action callback)
        {
            return Start(duration, false, callback);
        }

        /// <summary>
        /// Simple timer, no reference, wait and then execute something
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="repeat"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IEnumerator Start(float duration, bool repeat, Action callback)
        {
            do
            {
                yield return new WaitForSeconds(duration);

                if (callback != null)
                    callback();

            } while (repeat);
        }

        public static IEnumerator StartRealtime(float time, System.Action callback)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < start + time)
            {
                yield return null;
            }

            if (callback != null) callback();
        }

        public static IEnumerator NextFrame(Action callback)
        {
            yield return new WaitForEndOfFrame();

            if (callback != null)
                callback();
        }
    }

    /*
    TimerExample.cs
    const float duration = 3f;

    // Simple creation: can't be stopped even if lopping
    //--------------------------------------------
    StartCoroutine(Timer.Start(duration, true, () =>
    {
      // Do something at the end of the 3 seconds (duration)
      //...
    }));

    // Launch the timer
    StartCoroutine(t.Start());

    // Ask to stop it next frame
    t.Stop();
    */
}