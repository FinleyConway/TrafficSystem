using System;

namespace TrafficSystem
{
    public class Timer
    {
        private float m_Timer = 0;
        private bool m_isTimerRunning = false;

        public event Action OnTimerFinish;

        public Timer(float duration)
        {
            m_Timer = duration;
            m_isTimerRunning = true;
        }

        public void TimerHandler(float delta)
        {
            if (!m_isTimerRunning) return;

            m_Timer -= delta;
            if (m_Timer <= 0)
            {
                m_isTimerRunning = false;
                OnTimerFinish?.Invoke();
            }
        }

        public void TimerStart(float duration)
        {
            m_Timer = duration;
            m_isTimerRunning = true;
        }
    }
}