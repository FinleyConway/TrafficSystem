using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TrafficSystem
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private float m_StopDistance = 0.1f;
        [SerializeField] private float m_RotateSpeed = 1;
        [SerializeField] private float m_MovementSpeed = 1;
        private Vector3 m_Destination;
        public bool ReachedDestination;

        private void Update()
        {
            Vector3 destinationDirection = m_Destination - transform.position;
            destinationDirection.y = 0;

            float destinationDistance = destinationDirection.magnitude;

            if (destinationDistance >= m_StopDistance)
            {
                ReachedDestination = false;
                Quaternion targetRotation = Quaternion.LookRotation(destinationDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_RotateSpeed * Time.deltaTime);
                transform.Translate(Vector3.forward * m_MovementSpeed * Time.deltaTime);
            }
            else
            {
                ReachedDestination = true;
            }
        }

        public void SetDestination(Vector3 destination)
        {
            m_Destination = destination;
            ReachedDestination = false;
        }
    }
}
