using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[AddComponentMenu("QuickPath/AI/Patrol Object")]
/// <summary>
/// MoveObject that patrols a given path.
/// </summary>
public class qpPatrolObject : qpMoveObject {

    /// <summary>
    /// The path the object will move, when it reaches its end it will start over.
    /// </summary>
    public List<Vector3> PointsToPatrol = new List<Vector3>();

    /// <summary>
    /// Should the object reverse the path and walk it, when it finishes the path?
    /// </summary>
    public bool PingPong = true;
    private bool _forward = true;

    /// <summary>
    /// Should the object perform an A* search algorithm between each point in the patrol path?
    /// </summary>
    public bool PathfindingBetweenPoints = false;

    /// <summary>
    /// Inherited method from MoveObject, called whenever destination has been reached.
    /// </summary>
    public override void FinishedPath()
    {
        if (PingPong) {
            if (PointsToPatrol.Count > 0) {
                Vector3 destination = _forward ? PointsToPatrol[PointsToPatrol.Count - 1] : PointsToPatrol[0];
                _forward = !_forward;

                base.Start();
                List<qpNode> prePath = AStar(PreviousNode, qpManager.Instance.FindNodeClosestTo(destination));
                SetPath(prePath);
            }
        }
    }

    /// <summary>
    /// Use this for initialization
    /// </summary>
	protected void Start () {
        base.Start();

        if (!PathfindingBetweenPoints) {
            SetPath(PointsToPatrol);
        } else  {
            List<qpNode> _pathfindingPath = new List<qpNode>();
            for (int i = 0; i < PointsToPatrol.Count - 1; i++) {
                _pathfindingPath.AddRange(AStar(qpManager.Instance.FindNodeClosestTo(PointsToPatrol[i]), qpManager.Instance.FindNodeClosestTo(PointsToPatrol[i + 1])));
            }
            SetPath(_pathfindingPath);
        }
	}
    
}
