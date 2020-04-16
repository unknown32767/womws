using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SimpleColorSetter : MonoBehaviour
{
    public Color color;

    // Update is called once per frame
    void Update()
    {
        GetComponent<MeshRenderer>().material.color = color;
    }
}
