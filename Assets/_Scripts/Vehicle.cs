using UnityEngine;

namespace TrafficSystem
{
    public class Vehicle : MonoBehaviour
    {
        [SerializeField] private SplinePath m_Spline;
        [SerializeField] private float m_Speed = 1;

        private float m_MoveAmount;
        private float m_MaxMoveAmount = 1f;

        private void Start ()
        {
            m_MaxMoveAmount = m_Spline.GetSplineLength();
        }

        private void Update()
        {
            m_MoveAmount = (m_MoveAmount + (Time.deltaTime * m_Speed)) % m_MaxMoveAmount;
            transform.position = m_Spline.GetPositionAtUnits(m_MoveAmount);
        }
    }
}