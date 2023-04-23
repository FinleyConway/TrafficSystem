using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class PathFinding
    {
        public List<Anchor> FindPath(Anchor startPosition, Anchor targetPosition, PathType pathType)
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
                    return RetrancePath(startPosition, targetPosition);
                }

                foreach (Anchor nearbyNode in GetNearbyAnchors(currentNode))
                {
                    if (nearbyNode == null) continue;

                    if (closeSet.Contains(nearbyNode))
                    {
                        continue;
                    }

                    float newMovementCostToNearby = currentNode.GCost + (pathType == PathType.AStar ? Vector3.Distance(currentNode.transform.position, nearbyNode.transform.position) : 0);
       
                    if (newMovementCostToNearby < nearbyNode.GCost || !openSet.Contains(nearbyNode))
                    {
                        nearbyNode.GCost = newMovementCostToNearby;
                        nearbyNode.HCost = Vector3.Distance(nearbyNode.transform.position, targetPosition.transform.position);
                        nearbyNode.Parent = currentNode;

                        if (!openSet.Contains(nearbyNode))
                        {
                            openSet.Add(nearbyNode);
                        }
                        else
                        {
                            openSet.UpdateItem(nearbyNode);
                        }
                    }
                }
            }
            return null;
        }

        private List<Anchor> RetrancePath(Anchor startNode, Anchor endNode)
        {
            List<Anchor> path = new List<Anchor>();
            Anchor currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Add(startNode);
            path.Reverse();

            return path;
        }

        private List<Anchor> GetNearbyAnchors(Anchor anchor)
        {
            List<Anchor> anchors = new List<Anchor>();

            for (int i = 0; i < anchor.Branches.Count; i++)
            {
                anchors.Add(anchor.Branches[i].ToAnchor);
            }

            anchors.Add(anchor.NextAnchor);

            return anchors;
        }

        public enum PathType { Dijkstra, AStar }
    }
}