using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uiScript : MonoBehaviour
{
    public Slider health;
    public Slider MP;
    public float playerHealth;
    public float maxHealth;
    public float playerMP;
    public float maxMP;
    public int playerID;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        health.value = playerHealth / maxHealth;
        MP.value = playerMP / maxMP;
    }
}
