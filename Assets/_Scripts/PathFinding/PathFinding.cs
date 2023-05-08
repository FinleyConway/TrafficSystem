using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class PathFinding : MonoBehaviour
    {
        private PathRequestManager m_RequestManager;

        private void Awake()
        {
            m_RequestManager = GetComponent<PathRequestManager>();
        }

        public enum PathType { Dijkstra, AStar }

        public void StartFindPath(Anchor pathStart, Anchor pathEnd)
        {
            StartCoroutine(FindPath(pathStart, pathEnd, PathType.AStar)); 
        }

        private IEnumerator FindPath(Anchor startPosition, Anchor targetPosition, PathType pathType)
        {
            Anchor[] wayPoints = new Anchor[0];
            bool pathSuccess = false;

            DynamicHeap<Anchor> openSet = new DynamicHeap<Anchor>();
            HashSet<Anchor> closeSet = new HashSet<Anchor>();

            openSet.Add(startPosition);

            while (openSet.Count > 0)
            {
                Anchor currentNode = openSet.RemoveFirst();
                closeSet.Add(currentNode);

                if (currentNode == targetPosition)
                {
                    pathSuccess = true;
                    break;
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
            yield return null;

            if  (pathSuccess)
            {
                wayPoints = RetrancePath(startPosition, targetPosition);
            }
            m_RequestManager.FinishedProcessingPath(wayPoints, pathSuccess);
        }

        private Anchor[] RetrancePath(Anchor startNode, Anchor endNode)
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

            return path.ToArray();
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
    }
}