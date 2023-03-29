using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class Anchor : MonoBehaviour, IHeapItem<Anchor>
    {
        private void Awake()
        {
            if (TryGetComponent(out BoxCollider box)) Destroy(box);
        }

        // spline
        [field: SerializeField] public Vector3 HandleAPosition { get; set; }
        [field: SerializeField] public Vector3 HandleBPosition { get; set; }
        [field: SerializeField] public List<Branch> Branches { get; private set; } = new List<Branch>();
        [field: SerializeField, HideInInspector] public Anchor NextAnchor { get; set; }

        // path finding
        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost { get { return GCost + HCost; } }
        public Anchor Parent { get; set; }
        public int HeapIndex { get; set; }

        public int CompareTo(Anchor anchor)
        {
            int compare = FCost.CompareTo(anchor.FCost);
            if (compare == 0)
            {
                compare = HCost.CompareTo(anchor.HCost);
            }
            return -compare;
        }

        [Serializable]
        public struct Branch
        {
            public SplinePath NextPath;
            public Anchor ToAnchor;
        }
    }
}