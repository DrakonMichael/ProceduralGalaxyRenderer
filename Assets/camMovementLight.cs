using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camMovementLight : MonoBehaviour
{
    public float speedC = 1f;

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + new Vector3(0.025f * speedC * Time.deltaTime, 0, 0);
    }
}
