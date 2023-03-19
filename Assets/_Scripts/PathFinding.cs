using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class PathFinding : MonoBehaviour
    {
        public Anchor Start;
        public Anchor End;

        public SplinePath Follow;

        private void Awake()
        {
            FindPath(Start, End);
        }

        public void FindPath(Anchor startPosition, Anchor targetPosition)
        {
            Anchor startNode = startPosition;
            Anchor endNode = targetPosition;

            List<Anchor> openSet = new List<Anchor>();
            HashSet<Anchor> closeSet = new HashSet<Anchor>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Anchor currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < currentNode.FCost || openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closeSet.Add(currentNode);

                if (currentNode == targetPosition)
                {
                    RetrancePath(startNode, endNode);
                    return;
                }

                SplinePath path = currentNode.GetComponentInParent<SplinePath>();

                foreach (Anchor nearby in GetNearbyAnchors(currentNode, path))
                {
                    if (closeSet.Contains(nearby))
                    {
                        continue;
                    }
                    float distance = path.GetSplineLengthTo(currentNode) - path.GetSplineLengthTo(nearby);
                    float newMovementCostToNearby = currentNode.GCost + distance;

                    print(nearby);

                    if (nearby == null) continue;
                    if (newMovementCostToNearby < nearby.GCost || !openSet.Contains(nearby))
                    {
                        nearby.GCost = newMovementCostToNearby;
                        nearby.HCost = path.GetSplineLengthTo(nearby) - path.GetSplineLengthTo(targetPosition);
                        nearby.Parent = currentNode;

                        if (!openSet.Contains(nearby))
                        {
                            openSet.Add(nearby);
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
    }
}

/*
 * unoptimized but ill fix it when im not tired
*/ 