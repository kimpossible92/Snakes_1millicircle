using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamSapiel : MonoBehaviour
{
    
    [SerializeField]
    private GameObject goStart;
    GameObject wormis;
    public static SamSapiel instnce;
    [SerializeField] GameObject @objectlisght;
    public GameObject getWorm()
    {
        return wormis;
    }
    public void loadScene(string load)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(load);
    }
    // Start is called before the first frame update
    void Start()
    {
        instnce = this;
        InitPref();
        if(@objectlisght!=null)DontDestroyOnLoad(@objectlisght);
    }
    protected void InitPref()
    {
        GameObject worm = (GameObject)MonoBehaviour.Instantiate(goStart, transform.position, Quaternion.identity);
        wormis = worm;
        //worm.transform.SetParent(transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
