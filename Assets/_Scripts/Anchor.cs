using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class Anchor : MonoBehaviour
    {
        [field: SerializeField] public Vector3 HandleAPosition { get; set; }
        [field: SerializeField] public Vector3 HandleBPosition { get; set; }
        [field: SerializeField] public List<Branch> Branches { get; private set; } = new List<Branch>();

        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost { get { return GCost + HCost; } }
        public Anchor Parent { get; set; }
    }

    [Serializable]
    public struct Branch
    {
        public SplinePath NextPath;
        public Anchor ToAnchor;
    }
}