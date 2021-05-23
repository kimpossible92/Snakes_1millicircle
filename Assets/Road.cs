using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{
    [SerializeField] Material[] mts;
    public Material GetMaterial() { return mts[pcolor]; }
    private int pcolor;
    [SerializeField]
    private float posX;
    [SerializeField]
    private float posY;
    [SerializeField] private offcorm[] Offcorms;
    [SerializeField] offcorm newcorm;
    [SerializeField] GameObject leftpart;
    [SerializeField] GameObject rightpart;
    [SerializeField]
    LayerMask MoveOnSightLayer;
    public int mycolor() { return pcolor; }
    // Start is called before the first frame update
    void Start()
    { 
        pcolor = Random.Range(0, Offcorms.Length);
        GameObject worm = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position+new Vector3(posX, -2.5f, 0), Quaternion.identity);
        GameObject worm2 = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position-new Vector3(posX, posY, 0), Quaternion.identity);
        GameObject worm3 = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position+ new Vector3(posX-1.5f, -2.5f, 0), Quaternion.identity);
        GameObject worm4 = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position+ new Vector3(posX+1.5f, -2.5f, 0), Quaternion.identity);
        GameObject worm5 = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position- new Vector3(posX+1.5f, posY, 0), Quaternion.identity);
        GameObject worm6 = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position- new Vector3(posX-1.5f, posY, 0), Quaternion.identity);
        GameObject worm7 = (GameObject)MonoBehaviour.Instantiate(newcorm.gameObject, transform.position + new Vector3(-posX - 1.5f, -2.5f, 0), Quaternion.identity);
        GameObject worm8 = (GameObject)MonoBehaviour.Instantiate(newcorm.gameObject, transform.position - new Vector3(-posX, posY, 0), Quaternion.identity);
        GameObject worm9 = (GameObject)MonoBehaviour.Instantiate(newcorm.gameObject, transform.position + new Vector3(-posX + 1.5f, -2.5f, 0), Quaternion.identity);
        leftpart.GetComponent<Renderer>().material = mts[pcolor];
        rightpart.GetComponent<Renderer>().material = mts[pcolor];

    }
    public void newStart()
    {
        GameObject worm = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position + new Vector3(posX, -2.5f, 0), Quaternion.identity);
        GameObject worm2 = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position - new Vector3(posX, posY, 0), Quaternion.identity);
        GameObject worm3 = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position + new Vector3(posX - 1.5f, -2.5f, 0), Quaternion.identity);
        GameObject worm4 = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position + new Vector3(posX + 1.5f, -2.5f, 0), Quaternion.identity);
        GameObject worm5 = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position - new Vector3(posX + 1.5f, posY, 0), Quaternion.identity);
        GameObject worm6 = (GameObject)MonoBehaviour.Instantiate(Offcorms[pcolor].gameObject, transform.position - new Vector3(posX - 1.5f, posY, 0), Quaternion.identity);
        GameObject worm7 = (GameObject)MonoBehaviour.Instantiate(newcorm.gameObject, transform.position + new Vector3(-posX - 1.5f, -2.5f, 0), Quaternion.identity);
        GameObject worm8 = (GameObject)MonoBehaviour.Instantiate(newcorm.gameObject, transform.position - new Vector3(-posX, posY, 0), Quaternion.identity);
        GameObject worm9 = (GameObject)MonoBehaviour.Instantiate(newcorm.gameObject, transform.position + new Vector3(-posX + 1.5f, -2.5f, 0), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if (MMDebug.Raycast3DBoolean(transform.position - new Vector3(9, 0, 0.1f), Vector3.down, 9.35f, MoveOnSightLayer, Color.red, true))
        {
            SamSapiel.instnce.getWorm().GetComponent<WormCircle>().plusindex(pcolor);
        }
    }
}
