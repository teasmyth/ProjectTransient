using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class BehaviourTreeRunner : MonoBehaviour
{
    public BehaviourTree tree;
    
    public void SetupTree()
    {
        tree = tree.Clone();
        tree.Bind(GetComponent<Enemy>());
    }
    
    public Node.State RunTree()
    {
        return tree.Update();
    }
}