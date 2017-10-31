using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using xLIB;
using System;

using ACTIVE_TIME;


namespace ACTIVE_TIME
{
    public class RemainTimer
    {
        protected long _TargetTime = 0;
        protected long _LastTick = 0;
        protected int _OldRemainTime = 0;
        protected System.Action<int, string> _callback;

        public string CurrentTime = string.Empty;
        public int RemainTime = 0;

        public void BeginRemainTime(System.Action<int, string> callback, long target_sec)
        {
            _callback = callback;
            _TargetTime = target_sec;
            RemainTime = (int)target_sec;
            _LastTick = (System.DateTime.UtcNow.Ticks / 10000000L);
        }

        public void Stop()
        {
            _callback = null;
            _TargetTime = 0;
            RemainTime = 0;
            _LastTick = 0;
        }

        public void UpdateTime(long curTimeTick)
        {
            if (RemainTime <= 0) return;

            long iTick = curTimeTick - _LastTick;
            RemainTime = (int)(_TargetTime - iTick);

            System.TimeSpan time = new TimeSpan(RemainTime * 10000000L);
            CurrentTime = string.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
            if (_callback != null) _callback(RemainTime, CurrentTime);
            // Update 로 호출시 초당 업데이트 처리
            if (RemainTime != _OldRemainTime)
            {
                _OldRemainTime = RemainTime;
            }
        }
    }


    public class AlramTimer
    {
        private System.Action _AlramCallback = null;
        private float _CurAlramTime = 0;
        private float _AlramTime = 0;
        private bool play = false;

        public void SetAlramEvent(System.Action callback, float fAlramTime, bool bNowPlay=false)
        {
            _AlramCallback = callback;
            _AlramTime = fAlramTime;
            _CurAlramTime = Time.time;
            if (bNowPlay) Play();
        }

        public void Play()
        {
            play = true;
            _CurAlramTime = Time.time;
        }
        public void Pause()
        {
            play = false;
        }
        public void Stop()
        {
            play = false;
            _AlramCallback = null;
        }

        public void UpdateTime()
        {
            if (play && _AlramCallback != null)
            {
                if ((Time.time - _CurAlramTime) >= _AlramTime)
                {
                    _CurAlramTime = Time.time;
                    _AlramCallback();
                }
            }
        }
    }

}




public class SYSTIMER : Singleton<SYSTIMER>
{
    public delegate void delegateDealUpdateTime(int val, string time);
    public delegateDealUpdateTime event_DealUpdateTime;

    private long server_time = 0;

    private System.Action callbackTimer = null;
    private float fCurCallbackTime = 0;
    private float fCheckCallbackTime = 0;

    private RemainTimer _Deal = new RemainTimer();
    private RemainTimer _Bonus = new RemainTimer();

    public AlramTimer BounusAlram = new AlramTimer();
    public AlramTimer ReadyAlram = new AlramTimer();
    public AlramTimer TopCastAlram = new AlramTimer();
    public AlramTimer ReConnectAlram = new AlramTimer();

    public AlramTimer SpinAlram = new AlramTimer();

    public void Initialize()
    {
        server_time = (System.DateTime.UtcNow.Ticks / 10000000L);
        StartCoroutine(coUpdateTime());
    }

    public void CheckTiemrCallback(float time, System.Action callback)
    {
        callbackTimer = callback;
        fCurCallbackTime = Time.time;
        fCheckCallbackTime = time;
    }

    static public long GetCurTime() { return (System.DateTime.UtcNow.Ticks / 10000000L); }

    public RemainTimer GetDeal() { return _Deal; }
    public RemainTimer GetBonus() { return _Bonus; }

    private IEnumerator coUpdateTime()
    {
        while(true)
        {
            if (server_time > 0)
            {
                long curTimeTick = (System.DateTime.UtcNow.Ticks / 10000000L);

                _Deal.UpdateTime(curTimeTick);
                _Bonus.UpdateTime(curTimeTick);

                BounusAlram.UpdateTime();
                ReadyAlram.UpdateTime();
                TopCastAlram.UpdateTime();
                ReConnectAlram.UpdateTime();
                SpinAlram.UpdateTime();

                if (callbackTimer != null && fCheckCallbackTime > 0)
                {
                    if ((Time.time - fCurCallbackTime) >= fCheckCallbackTime)
                    {
                        fCurCallbackTime = 0;
                        fCheckCallbackTime = 0;
                        callbackTimer();
                    }
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
}
