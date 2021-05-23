using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WirmCircle : MonoBehaviour
{
    protected WormCircle wormCircle;
    [SerializeField]
    LayerMask DeadLayer;
    public void getworm(WormCircle worm)
    {
        wormCircle = worm;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (wormCircle.Load5sec == false)
        {
            if (MMDebug.Raycast3DBoolean(transform.position, Vector3.right, 0.45f, DeadLayer, Color.red, true)
                || MMDebug.Raycast3DBoolean(transform.position, Vector3.left, 0.45f, DeadLayer, Color.red, true)
                || MMDebug.Raycast3DBoolean(transform.position, Vector3.down, 0.45f, DeadLayer, Color.red, true)
                || MMDebug.Raycast3DBoolean(transform.position, Vector3.up, 0.45f, DeadLayer, Color.red, true))
            {
                wormCircle.nazad();
            }
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (wormCircle.Load5sec == false)
        {
            if (col.gameObject.tag == "razor")
            {
                wormCircle.nazad();
            }
        }
    }
}
