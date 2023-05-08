using System;
using UnityEngine;

namespace TrafficSystem
{
    public class TrafficLight : MonoBehaviour
    {
        [Header("Sequence")]
        [SerializeField] private LightControl[] m_LightSequence;
        private int m_LightIndex = 0;
        private LightControl m_LastLightSequence;
        private Timer m_Timer;

        private void Awake()
        {
            LightControl current = m_LightSequence[0];
            m_LastLightSequence = current;

            m_Timer = new Timer(current.Duration);
            m_Timer.OnTimerFinish += ChangeSequence;

            for (int i = 0; i < m_LightSequence.Length; i++)
            {
                ToggleLightState(m_LightSequence[i], false);
            }
            ToggleLightState(current, true);
        }

        private void Update()
        {
            m_Timer.TimerHandler(Time.deltaTime);
        }

        private void ChangeSequence()
        {
            m_LightIndex++;

            if (m_LightIndex > m_LightSequence.Length - 1)
            {
                m_LightIndex = 0;
            }

            LightControl current = m_LightSequence[m_LightIndex];

            ToggleLightState(m_LastLightSequence, false);
            ToggleLightState(current, true);

            m_LastLightSequence = current;

            m_Timer.TimerStart(current.Duration);
        }

        private void ToggleLightState(LightControl current, bool toggle)
        {
            if (current == null) return;

            foreach (GiveWay stopper in current.Stoppers)
            {
                stopper.ShouldGo = toggle;
            }

            foreach (GameObject light in current.Lights)
            {
                light.SetActive(toggle);
            }
        }

        [Serializable]
        public class LightControl
        {
            public GiveWay[] Stoppers;
            public GameObject[] Lights;
            public float Duration = 0;
        }
    }
}