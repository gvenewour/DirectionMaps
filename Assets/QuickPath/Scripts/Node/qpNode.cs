using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class qpNode  {

    public List<qpNode> ContactedNodes = new List<qpNode>();
    public List<qpNode> NonDiagonalContactedNodes = new List<qpNode>();
    public bool traversable = true;
    public bool outdated = false;

    private float _g = 1;
    private float _h;
    private float _total;
    
    private qpNode _parent;
    private Vector3 _coordinates;


    //DIRECTION MAPS #start
    public Vector3 _DirectionVector;
    //DIRECTION MAPS #end

    public Vector3 GetCoordinates()
    {
        return _coordinates;
    }

    public void SetMutualConnection(qpNode otherNode, bool diagonal = false)
    {
        if (otherNode != null) {
            if (!ContactedNodes.Contains(otherNode)) {
                ContactedNodes.Add(otherNode);
            }

            if (!otherNode.ContactedNodes.Contains(this)) {
                otherNode.ContactedNodes.Add(this);
            }
            
            if (!diagonal) {
                if (!NonDiagonalContactedNodes.Contains(otherNode)) {
                    NonDiagonalContactedNodes.Add(otherNode);
                }
                
                if(!otherNode.NonDiagonalContactedNodes.Contains(this)) {
                    otherNode.NonDiagonalContactedNodes.Add(this);
                }
            }
        }
    }

    public void SetConnection(qpNode node)
    {
        if (node != null) {
            if (!ContactedNodes.Contains(node)) {
                ContactedNodes.Add(node);
            }
        }
    }
    
    public void RemoveMutualConnection(qpNode otherNode)
    {
        if (otherNode != null) {
            if (ContactedNodes.Contains(otherNode)) {
                ContactedNodes.Remove(otherNode);
            }

            if(NonDiagonalContactedNodes.Contains(otherNode)) {
                NonDiagonalContactedNodes.Remove(otherNode);
            }

            if (otherNode.ContactedNodes.Contains(this)) {
                otherNode.ContactedNodes.Remove(this);
            }

            if (otherNode.NonDiagonalContactedNodes.Contains(this)) {
                otherNode.NonDiagonalContactedNodes.Add(this);
            }
        }
    }

    public void RemoveAllMutualConnections()
    {
        if (ContactedNodes != null) {
            foreach(qpNode neighbour in ContactedNodes) {
                RemoveMutualConnection(neighbour);
            }
        }

        if (NonDiagonalContactedNodes != null) {
            foreach (qpNode neighbour in NonDiagonalContactedNodes) {
                RemoveMutualConnection(neighbour);
            }
        }
    }

    protected void SetCoordinate(Vector3 newCoordinate)
    {
        _coordinates = newCoordinate;
    }

    #region Pathfinding

    public float CalculateTotal(qpNode start, qpNode end)
    {
        _h = CalculateH(end);
        if (_parent != null) {
            _g = CalculateG(_parent) + _parent.GetG();
        } else {
            _g = 1;
        }

        _total = _g + _h;
        
        return _total;
    }

    public float CalculateH(qpNode end)
    {
        return Vector3.Distance(_coordinates, end.GetCoordinates());
    }

    public float CalculateG(qpNode parent)
    {
        return Vector3.Distance(_coordinates, parent.GetCoordinates());
    }

    public float GetG()
    {
        return _g;
    }

    public float GetTotal()
    {
        return _total;
    }

    public void SetParent(qpNode parent)
    {
        _parent = parent;
    }

    public qpNode GetParent()
    {
        return _parent;
    }

    #endregion
}
