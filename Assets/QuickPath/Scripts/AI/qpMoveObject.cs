using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[AddComponentMenu("QuickPath/AI/Basic Move Object")]

public class qpMoveObject : MonoBehaviour {

    public bool Moving = false;
    public bool CanMoveDiagonally = true;

    public float SpillDistance = .1f;   // The radius around from each node that this object will go to. If higher it will walk further away from nodes, if lower it will walk closer to nodes.
    public float Speed = 10f;

    public Vector3 Offset = new Vector3(0, 0, 0);   //If (0,0,0) is not the correct anchor, you can specify a new anchor here.

    public bool DrawPathInEditor = false;   /// If true the path of this object will be drawn in editor with lines.

    protected List<qpNode> Path = new List<qpNode>();
    protected qpNode PreviousNode;
    protected qpNode NextNode;
    protected qpNode DestinationNode = null;
    protected Vector3 DestinationCoordinate = Vector3.zero;

    public GameObject destinationMarker;
    private int _moveCounter = 0;

	public void Start () {
        FindClosestNode();
	}

    protected void FixedUpdate()
    {
        _verifyNodes();       
        _move();

        if (DrawPathInEditor && Path.Count > 0) {
            for (int i = 1; i < Path.Count; i++)
            {
                qpNode prevNode = Path[i - 1];
                Debug.DrawLine(prevNode.GetCoordinates() + Offset, Path[i].GetCoordinates() + Offset, Color.red, 0, false);
            }
        }
    }

    public void CreatePathStartMoving(qpNode destination)    // Creates a path to the desired node, and begins walking the path
    {
        SetPath(AStar(_getNearNode(), destination));
    }

    public void CreatePathStartMoving(Vector3 destination)
    {
        SetPath(AStar(_getNearNode(), qpManager.Instance.FindNodeClosestTo(destination)));
    }

    public void SetPath(List<qpNode> path)  // Hands the object a new path, which it will immediately begin to walk
    {
        if (path.Count > 0) {
            
            if (((_moveCounter + 1) < Path.Count) && (path.Count > 1)) {  //check to see if already moving and if so determine which node are closest.
                if (Path[_moveCounter].GetCoordinates() == path[1].GetCoordinates()) {
                    path.RemoveAt(0);
                }
            }
            _moveCounter = 0;
            DestinationCoordinate = path[path.Count - 1].GetCoordinates();
            Path = path;
            if (DrawPathInEditor) {
                _drawDestination();
            }
        }
    }

    public void SetPath(List<Vector3> path)
    {
        if (path.Count > 0 && qpManager.Instance.nodes.Count>0)
        {
            _moveCounter = 0;
            DestinationCoordinate = path[path.Count - 1];
            List<qpNode> _list = new List<qpNode>();
            for (int i = 0; i < path.Count; i++) {
                _list.Add(qpManager.Instance.FindNodeClosestTo(path[i]));
            }
            Path = _list;

            if (DrawPathInEditor) {
                _drawDestination();
            }
        }
    }

    public virtual void FinishedPath()  // Called whenever this object reaches its destination. This method is empty and intended to be overriden
    {
        if (destinationMarker != null) {
            GameObject.DestroyImmediate(destinationMarker);
        }
        destinationMarker = null;
    }

    public void FindClosestNode()
    {
        PreviousNode = qpManager.Instance.FindNodeClosestTo(this.transform.position);
    }

    protected List<qpNode> AStar(qpNode start, qpNode end)
    {
        //Debug.Log("astar from "+start.GetCoordinate()+" to "+end.GetCoordinate());
        List<qpNode> path = new List<qpNode>();         
        bool searchComplete = (end == null || start == null) ? true : false;                                  //Regulates the main while loop of the algorithm
        List<qpNode> closedList = new List<qpNode>();   //Closed list for the best candidates.
        List<qpNode> openList = new List<qpNode>();     //Open list for all candidates(A home for all).
        qpNode candidate = start;                           //The current node candidate which is being analyzed in the algorithm.
        
        if (start == null || end == null) {
            return null;
        }

        openList.Add(start);

        int astarSteps = 0;
        while (openList.Count > 0 && !searchComplete)      //ALGORITHM STARTS HERE.
        {
            if (!searchComplete) {
                astarSteps++;
                if (candidate == end) {                //If current candidate is end, the algorithm has been completed and the path can be built.
                    DestinationNode = end;
                    searchComplete = true;
                    bool pathComplete = false;
                    qpNode node = end;
                    while (!pathComplete) {
                        path.Add(node);
                        if (node == start) {
                            pathComplete = true;
                        }
                        node = node.GetParent();
                    }
                }

                List<qpNode> allNodes = (CanMoveDiagonally) ? candidate.ContactedNodes : candidate.NonDiagonalContactedNodes;
                List<qpNode> potentialNodes = new List<qpNode>();

                foreach (qpNode n in allNodes) {
                    if (n.traversable) {
                        potentialNodes.Add(n);
                    }
                }

                foreach (qpNode n in potentialNodes) {
                    bool inClosed = closedList.Contains(n);
                    bool inOpen = openList.Contains(n);

                    if (!inClosed && !inOpen) {             //Mark candidate as parent if not in open nor closed.
                        n.SetParent(candidate);
                        openList.Add(n);
                    } else if (inOpen) {                    //But if in open, then calculate which is the better parent: Candidate or current parent.
                        float math2 = n.GetParent().GetG();
                        float math1 = candidate.GetG();
                        if (math2 > math1) {                //candidate is the better parent as it has a lower combined g value.
                            n.SetParent(candidate);
                        }
                    }
                }

                //Calculate h, g and total
                if (openList.Count == 0) {
                    break;
                }

                openList.RemoveAt(0);

                if (openList.Count == 0) {
                    break;
                }

                for (int i = 0; i < openList.Count; i++) {  //the below one-lined for loop,if conditional and method call updates all nodes in openlist.
                    openList[i].CalculateTotal(start, end);
                }

                openList.Sort(delegate(qpNode node1, qpNode node2) {
                    return node1.GetTotal().CompareTo(node2.GetTotal());
                });

                candidate = openList[0];
                closedList.Add(candidate);
            } 
        }



        //Debug.Log("astar completed in " + astarSteps + " steps. Path found:"+complete);
        path.Reverse();
        return path;
    }

    private void _verifyNodes()
    {
        bool outdated = false;

        if (_getNearNode() != null) {
            if (_getNearNode().outdated) {
                outdated = true;
            }

            if (DestinationNode != null) {
                if (DestinationNode.outdated) {
                    outdated = true;
                }
            }

            if (_moveCounter < Path.Count) {
                foreach (qpNode node in Path) {
                    if (node.outdated) {
                        outdated = true;
                        break;
                    }
                }
            }

            qpManager.Instance._gridMutex.WaitOne();
                if (qpManager.Instance.gridWasUpdated) {
                    qpManager.Instance.gridWasUpdated = false;
                    outdated = true;
                }
            qpManager.Instance._gridMutex.ReleaseMutex();

            if (outdated) {
                Debug.Log("OUTDATED, creating new path");
                CreatePathStartMoving(DestinationCoordinate);
            }
        }
        
    }

    private void _move()
    {
        if (Path != null) {
            if (_moveCounter < Path.Count) {
                Moving = true;
                transform.position = Vector3.MoveTowards(transform.position, Path[_moveCounter].GetCoordinates() + Offset, Time.deltaTime * Speed);
                if (Vector3.Distance(transform.position, Path[_moveCounter].GetCoordinates() + Offset) < SpillDistance)
                {
                    PreviousNode = Path[_moveCounter];
                    _moveCounter++;
                    if (_moveCounter < Path.Count) {
                        NextNode = Path[_moveCounter];
                    } else {
                        FinishedPath();
                    }
                }
            } else  {
                Moving = false;
            }
        }
    }
    
    private qpNode _getNearNode()
    {
        qpNode result = (NextNode == null ||!NextNode.outdated) ? PreviousNode : NextNode;
        
        if (result != null) {
            if (result.outdated) {
                FindClosestNode();
                return (PreviousNode);
            }
        }
        return result;
    }
    
    
    private void _drawDestination()
    {
        if (!destinationMarker) {
            destinationMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        destinationMarker.renderer.material.color = new Color(1, 0, 0);
        
        if (Path.Count > 0) {
            destinationMarker.transform.position = Path[Path.Count - 1].GetCoordinates() + Offset;
        } 
    }
}