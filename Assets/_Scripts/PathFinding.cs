using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class PathFinding : MonoBehaviour
    {
        [SerializeField] private PathType m_PathType;

        public Anchor Start;
        public Anchor End;

        public SplinePath Follow;
        public Vehicle Vehicle;

        private void Awake()
        {
            FindPath(Start, End);
        }

        public void FindPath(Anchor startPosition, Anchor targetPosition)
        {
            DynamicHeap<Anchor> openSet = new DynamicHeap<Anchor>();
            HashSet<Anchor> closeSet = new HashSet<Anchor>();

            openSet.Add(startPosition);

            while (openSet.Count > 0)
            {
                Anchor currentNode = openSet.RemoveFirst();
                closeSet.Add(currentNode);

                if (currentNode == targetPosition)
                {
                    RetrancePath(startPosition, targetPosition);
                    return;
                }

                SplinePath path = currentNode.GetComponentInParent<SplinePath>();

                foreach (Anchor nearby in GetNearbyAnchors(currentNode, path))
                {
                    if (nearby == null) continue;

                    if (closeSet.Contains(nearby))
                    {
                        continue;
                    }

                    float newMovementCostToNearby = currentNode.GCost + (m_PathType == PathType.AStar ? path.GetSplineLengthTo(currentNode) - path.GetSplineLengthTo(nearby) : 0);

                    if (newMovementCostToNearby < nearby.GCost || !openSet.Contains(nearby))
                    {
                        nearby.GCost = newMovementCostToNearby;
                        nearby.HCost = path.GetSplineLengthTo(nearby) - path.GetSplineLengthTo(targetPosition);
                        nearby.Parent = currentNode;

                        if (!openSet.Contains(nearby))
                        {
                            openSet.Add(nearby);
                        }
                        else
                        {
                            openSet.UpdateItem(nearby);
                        }
                    }
                }
            }
        }

        private void RetrancePath(Anchor startNode, Anchor endNode)
        {
            Anchor currentNode = endNode;

            while (currentNode != startNode)
            {
                Follow.Anchors.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            // temp, will figure out a better way of storing the new path
            Follow.Anchors.Add(startNode);
            Follow.Anchors.Reverse();

            Follow.SetupPointList();
            Follow.SetDirty();
        }

        private List<Anchor> GetNearbyAnchors(Anchor anchor, SplinePath path)
        {
            List<Anchor> anchors = new List<Anchor>();

            for (int i = 0; i < anchor.Branches.Count; i++)
            {
                anchors.Add(anchor.Branches[i].ToAnchor);
            }

            anchors.Add(path.GetNextAnchor(anchor));
            anchors.Add(path.GetPreviousAnchor(anchor)); 
            return anchors;
        }

        private enum PathType { Dijkstra, AStar }
    }
}