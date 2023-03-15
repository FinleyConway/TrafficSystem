using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class Anchor : MonoBehaviour
    {
        public Vector3 HandleAPosition = new Vector3(1, 0 , 0);
        public Vector3 HandleBPosition = new Vector3(-1, 0 , 0);

        public List<Anchor> Branches = new List<Anchor>();
        public List<SplinePath> NewSpline = new List<SplinePath>();
    }
}