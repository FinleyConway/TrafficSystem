using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class PathRequestManager : MonoBehaviour
    {
        private readonly Queue<PathRequest> m_PathRequests = new Queue<PathRequest>();
        private PathRequest m_CurrentPathRequest;

        private PathFinding m_PathFinding;
        private bool m_IsProcessingPath;

        private static PathRequestManager m_Instance;

        private void Awake()
        {
            m_Instance = this;
            m_PathFinding = GetComponent<PathFinding>();
        }

        public static void RequestPath(Anchor pathStart, Anchor pathEnd, Action<Anchor[], bool> callback)
        {
            PathRequest pathRequest = new PathRequest(pathStart, pathEnd, callback);
            m_Instance.m_PathRequests.Enqueue(pathRequest);
            m_Instance.TryProcessNext();
        }

        public void FinishedProcessingPath(Anchor[] path, bool success)
        {
            m_CurrentPathRequest.Callback(path, success);
            m_IsProcessingPath = false;
            TryProcessNext();
        }

        private void TryProcessNext()
        {
            if (!m_IsProcessingPath && m_PathRequests.Count > 0)
            {
                m_CurrentPathRequest = m_PathRequests.Dequeue();
                m_IsProcessingPath = true;
                m_PathFinding.StartFindPath(m_CurrentPathRequest.PathStart, m_CurrentPathRequest.PathEnd);
            }
        }

        private struct PathRequest
        {
            public Anchor PathStart;
            public Anchor PathEnd;
            public Action<Anchor[], bool> Callback;

            public PathRequest(Anchor start, Anchor end, Action<Anchor[], bool> callback)
            {
                PathStart = start;
                PathEnd = end;
                Callback = callback;
            }
        }
    }
}
