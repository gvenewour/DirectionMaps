using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("QuickPath/WayPointScript")]

public class qpNodeScript : MonoBehaviour
{
    public qpPointNode Node = new qpPointNode();
    public List<qpNodeScript> connections = new List<qpNodeScript>();

	void Awake () 
    {
        Node.Init(this, connections);
        
        if (renderer != null) {
            renderer.enabled = false;
            if (renderer.material != null)
            {
                renderer.material.color = new Color(0, 1, 0);
            }
        }
	}
}
