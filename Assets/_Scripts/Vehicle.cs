using UnityEngine;

namespace TrafficSystem
{
    public class Vehicle : MonoBehaviour
    {
        public SplinePath m_Spline;
        [SerializeField] private float m_Speed = 1;

        public float m_MoveAmount;
        public float m_MaxMoveAmount = 1f;

        private void Start ()
        {
            m_MaxMoveAmount = m_Spline.GetSplineLength();
        }

        private void Update()
        {
            m_MoveAmount = (m_MoveAmount + (Time.deltaTime * m_Speed)) % m_MaxMoveAmount;

            SplinePath.SplineInfo splinePath = m_Spline.GetPositionAtUnits(m_MoveAmount);

            print((Mathf.Abs(m_MoveAmount - m_MaxMoveAmount) < 0.1f));

            if (Mathf.Abs(m_MoveAmount - m_MaxMoveAmount) < 0.1f)
            {
                Anchor nextAnchor = splinePath.NextAnchor;
                if (nextAnchor.Branches.Count > 0 && !m_Spline.IsLoopClosed() && nextAnchor != null)
                {
                    m_MoveAmount = 0;
                    m_Spline = ChooseRandomBranch(nextAnchor);
                    m_MaxMoveAmount = m_Spline.GetSplineLength();
                }
            }

            transform.position = splinePath.Position;
            transform.forward = m_Spline.GetForwardAtUnits(m_MoveAmount);
        }

        private SplinePath ChooseRandomBranch(Anchor path)
        {
            int choice = Random.Range(0, path.Branches.Count);
            return path.NewSpline[choice];
        }
    }
}