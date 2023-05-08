using System.Collections;
using UnityEngine;

namespace TrafficSystem
{
    public interface IControlCar
    {
        public Vehicle CurrentCar { get; set; }
    }
}