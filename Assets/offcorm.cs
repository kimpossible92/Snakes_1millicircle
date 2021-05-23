using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class offcorm : MonoBehaviour
{
    [SerializeField] int color;

    public int Color { get => color; set => color = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision coll)
    {
        if (gameObject.tag != "razor")
        {
            if (coll.gameObject.tag == "w")
            {
                Destroy(gameObject);
            }
            if (coll.gameObject.tag == "server")
            {
                Destroy(gameObject);
            }
        }
        if (gameObject.tag == "razor")
        {
            if (coll.gameObject.tag == "w"&& coll.gameObject.GetComponent<WormCircle>().Load5sec)
            {
                Destroy(gameObject);
            }
        }
    }
}
