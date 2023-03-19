using UnityEngine;

namespace TrafficSystem
{
    public class Vehicle : MonoBehaviour
    {
        public SplinePath m_Spline;
        public Anchor m_NextAnchor;
        [SerializeField] private float m_Speed = 1;

        public float m_MoveAmount;
        public float m_MaxMoveAmount = 1f;

        private void Start()
        {
            m_MaxMoveAmount = m_Spline.GetSplineLength();
        }

        private void Update()
        {
            m_MoveAmount = (m_MoveAmount + (Time.deltaTime * m_Speed)) % m_MaxMoveAmount;

            SplinePath.SplineInfo splinePath = m_Spline.GetPositionAtUnits(m_MoveAmount);

            transform.position = splinePath.Position;
            transform.forward = m_Spline.GetForwardAtUnits(m_MoveAmount);
        }

        private void OnDrawGizmos()
        {
            if (m_NextAnchor != null)
                Gizmos.DrawSphere(m_NextAnchor.transform.position, 0.5f);
        }
    }
}