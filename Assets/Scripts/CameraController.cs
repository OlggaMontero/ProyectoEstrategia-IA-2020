using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float scrollSpeed;
    public float topBarrier;
    public float botBarrier;
    public float leftBarrier;
    public float rightBarrier;
    public float topLimit; //14
    public float botLimit; //-10
    public float leftLimit; //-25
    public float rightLimit; //25
    private float position_x;
    private float position_y;

    void  Update() {
        GameObject camera = GameObject.Find("Main Camera");
        position_x = camera.transform.position.x;
        position_y = camera.transform.position.y;
        if(Input.mousePosition.y >= Screen.height * topBarrier && position_y < topLimit){
            transform.Translate(Vector3.up * Time.deltaTime * scrollSpeed, Space.World);
        }
        if(Input.mousePosition.y <= Screen.height * botBarrier && position_y > botLimit){
            transform.Translate(Vector3.down * Time.deltaTime * scrollSpeed, Space.World);
        }
        if(Input.mousePosition.x >= Screen.width * rightBarrier && position_x < rightLimit){
            transform.Translate(Vector3.right * Time.deltaTime * scrollSpeed, Space.World);
        }
        if(Input.mousePosition.x <= Screen.width * leftBarrier && position_x > leftLimit){
            transform.Translate(Vector3.left * Time.deltaTime * scrollSpeed, Space.World);
        }
    }
}