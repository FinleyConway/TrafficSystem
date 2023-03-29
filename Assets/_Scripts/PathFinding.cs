﻿using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class PathFinding : MonoBehaviour
    {
        [SerializeField] private PathType m_PathType;

        public Anchor Start;
        public Anchor End;

        public SplinePath Follow;
        public Vehicle Vehicle; // temp

        public List<Anchor> Anchors = new List<Anchor>(); // temp

        private void Awake()
        {
            // temp
            Anchor[] anchors = FindObjectsOfType<Anchor>();
            for (int i = 0;i < anchors.Length; i++)
            {
                Anchors.Add(anchors[i]);
            }

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

                foreach (Anchor nearby in GetNearbyAnchors(currentNode))
                {
                    if (nearby == null) continue;

                    if (closeSet.Contains(nearby))
                    {
                        continue;
                    }

                    float newMovementCostToNearby = currentNode.GCost + (m_PathType == PathType.AStar ? Vector3.Distance(currentNode.transform.position, nearby.transform.position) : 0);
       
                    if (newMovementCostToNearby < nearby.GCost || !openSet.Contains(nearby))
                    {
                        nearby.GCost = newMovementCostToNearby;
                        nearby.HCost = Vector3.Distance(nearby.transform.position, targetPosition.transform.position);
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

        private enum PathType { Dijkstra, AStar }
    }
}