using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;

// The Manager contains the most current nodes. Singleton.

public sealed class qpManager :UnityEngine.Object
{
    public float collisionDistance = 1;
    public Vector3 startCoordinates = new Vector3(-50, -50, -50);      // Start position from which to ray cast for terrain suitable for nodes.
    public Vector3 endCoordinates = new Vector3(50, 50, 50);          // End position from which to ray cast for terrain suitable for nodes.
    public int numOfNodesInX;
    public int numOfNodesInY;
    public int numOfNodesInZ;

    public bool gridWasUpdated = false;

    public Mutex _gridMutex = new Mutex();


    public List<qpNode> nodes = new List<qpNode>();

    private qpManager()
    {
        nodes = Array.ConvertAll<UnityEngine.Object, qpNode>(GameObject.FindObjectsOfType(typeof(qpNodeScript)), delegate(UnityEngine.Object i)
        {
            return (i as qpNodeScript).Node;
        }).ToList();
    }

    private static readonly qpManager _instance = new qpManager();
    public static qpManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public void RegisterNodes(List<qpNode> selection)   // Registers nodes(normally sent from qpGrid)
    {
        if (nodes == null) {
            nodes = new List<qpNode>();
        }
        nodes.AddRange(selection);
    }

    public void RegisterNode(qpNode selection) // Registers a single node(normally sent from qp(qpPointNode)
    {
        nodes.Add(selection);
    }

    public void DelistNodes(List<qpNode> selection) // Deregisters nodes
    {
        foreach (qpNode node in selection) {
            node.outdated = true;
            nodes.Remove(node);
        }
    }

    public void DelistNode(qpNode node)  // Deregisters a single node
    {
        node.outdated = true;
        nodes.Remove(node);
    }

    public qpNode FindNodeClosestTo(Vector3 givenPoint) // Finds the node closest to the given point
    {
        if (qpManager.Instance.nodes.Count == 0) {
            Debug.LogError("Object can't move because there are no nodes(you haven't baked a qpGrid or instantiated any qpWayPoints)");
            return null;
        } else {
            qpNode returnNode;
            returnNode = nodes[nodes.Count - 1];

            float distance = Vector3.Distance(returnNode.GetCoordinates(), givenPoint);

            for (int i = nodes.Count - 2; i >= 0; i--) {
                if (nodes[i] != null) {
                    if (Vector3.Distance(nodes[i].GetCoordinates(), givenPoint) < distance) {
                        distance = Vector3.Distance(nodes[i].GetCoordinates(), givenPoint);
                        returnNode = nodes[i];
                    }
                }
            }
            return returnNode;
        }

    }

    private List<qpNode> _findNearestNodes(qpNode target, float radius)
    {
        List<qpNode> retList = new List<qpNode>();

        foreach (qpNode node in nodes) {
            if (target != node && Vector3.Distance(target.GetCoordinates(), node.GetCoordinates()) < radius)
            {
                //Debug.Log("node found at:" + Vector3.Distance(target.transform.position, node.transform.position));
                retList.Add(node);
            }
        }
        return retList;
    }

    public Vector3 getNodeOffsetCoordinate(Vector3 globalCoordinates)
    {
        Vector3 nodeIndexes = new Vector3(globalCoordinates.x, globalCoordinates.y, globalCoordinates.z);
        globalCoordinates.x -= startCoordinates.x;
        globalCoordinates.y -= startCoordinates.y;
        globalCoordinates.z -= startCoordinates.z;

        return nodeIndexes;
    }
   
}
