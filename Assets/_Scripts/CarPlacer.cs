using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class CarPlacer : MonoBehaviour
    {
        [SerializeField] private Transform m_SplinePosition;
        [SerializeField] private Transform m_CarPrefab;

        private readonly List<Anchor> m_Anchors = new List<Anchor>();

        public static CarPlacer instance;

        private void Start()
        {
            instance = this;

            Anchor[] anchors = FindObjectsOfType<Anchor>();
            for (int i = 0; i < anchors.Length; i++)
            {
                m_Anchors.Add(anchors[i]);
            }

            for (int i = 0; i < m_Anchors.Count / 3; i++)
            {
                Transform car = Instantiate(m_CarPrefab);

                Vehicle veh = car.GetComponentInChildren<Vehicle>();
                veh.StartAnchor = m_Anchors[i];
                veh.EndAnchor = GetRandomAnchor();

                veh.Init();
            }
        }

        public Anchor GetRandomAnchor()
        {
            return m_Anchors[Random.Range(0, m_Anchors.Count - 1)];
        }
    }
}
