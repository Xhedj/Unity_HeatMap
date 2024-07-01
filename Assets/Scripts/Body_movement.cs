using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body_movement : MonoBehaviour
{
    [SerializeField] GameObject character;
    [SerializeField] CharacterController controller;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float gravity = -1f;
    [SerializeField] private float JumpHeight = 1.5f;
    Vector3 height = new Vector3(0f,0f,0f);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);
        if (Input.GetKey("space"))
        {
            height.y = transform.up.y * 0.25f;
            character.transform.position += height;
        }
        

        if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl"))
        {
            height.y = -1 * transform.up.y * 0.25f;
            character.transform.position += height;

        }
    }
}
