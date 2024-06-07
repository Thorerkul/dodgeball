using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ballSpawner : MonoBehaviour
{
    public GameObject referenceBall;

    public PlayerInput input;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSpawn(InputAction.CallbackContext value)
    {
        //Debug.Log(value.ReadValue<bool>());
        GameObject newball = Instantiate(referenceBall);
        newball.transform.position = transform.position;
    }
}
