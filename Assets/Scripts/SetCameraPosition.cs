using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCameraPosition : MonoBehaviour
{
    [SerializeField]
    Transform cubeTransform;

    // Update is called once per frame
    void Update()
    {
        Vector3 camPosition = cubeTransform.position + new Vector3(0, 10, -10);
        Vector3 targetPosition = cubeTransform.position;
        this.transform.position = camPosition;
        this.transform.LookAt(targetPosition);
    }
}
