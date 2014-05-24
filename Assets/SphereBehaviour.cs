using UnityEngine;
using System.Collections;

public class SphereBehaviour : MonoBehaviour {

    delegate void TraverseDelegate(ref qpNode node);
    private TraverseDelegate SetTraverseState;
    private qpManager _manager;

    public void Awake()
    {
        _manager = qpManager.Instance;
    }

    private void SetTrue(ref qpNode node)
    {
        node.traversable = true;
    }

    private void SetFalse(ref qpNode node)
    {
        node.traversable = false;
    }

    private void UpdateObstacleInfo()
    {
        if (SetTraverseState != null) {
            
            _manager._gridMutex.WaitOne();

            Bounds bounds = renderer.bounds;

            Vector3 min = _manager.getNodeOffsetCoordinate(bounds.min);
            min.x -= _manager.collisionDistance; min.y -= _manager.collisionDistance; min.z -= _manager.collisionDistance;

            Vector3 max = _manager.getNodeOffsetCoordinate(bounds.max);
            max.x += _manager.collisionDistance; max.y += _manager.collisionDistance; max.z += _manager.collisionDistance;

            Debug.Log("Updating obstacle information: from " + min + " to " + max);

            for (int y = Mathf.FloorToInt(min.y); y <= Mathf.FloorToInt(max.y); y++) {
                for (int z = Mathf.FloorToInt(min.z); z <= Mathf.FloorToInt(max.z); z++) {
                    for (int x = Mathf.FloorToInt(min.x); x <= Mathf.FloorToInt(max.x); x++) {

                        qpNode node = _manager.nodes[(y * _manager.numOfNodesInX * _manager.numOfNodesInZ) + (z * _manager.numOfNodesInX + x)];
                        if (node != null) {
                            SetTraverseState(ref node);
                            node._DirectionVector = new Vector3(0, 0, 0);
                            //node.outdated = true;
                        }
                    }
                }
            }

            _manager.gridWasUpdated = true;
            _manager._gridMutex.ReleaseMutex();
        }
    }


	void OnEnable() {
		Debug.Log("Sphere was enabled");
        SetTraverseState = SetFalse;

        if (_manager.nodes.Count > 0) {
            UpdateObstacleInfo();
        }
	}

    void OnDisable()
    {
        Debug.Log("Sphere was disabled");
        SetTraverseState = SetTrue;
        if (_manager.nodes.Count > 0) {
            UpdateObstacleInfo();
        }
    }

}
