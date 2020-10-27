using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour
{

    private float _angularSpeed;
    private float _rotationAngle;
    private float _speed;
    private CharacterController _character;

    // Start is called before the first frame update
    void Start()
    {
        _speed = 0f;
        _rotationAngle = 0f;
        _angularSpeed = 0.5f;
        _character = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float mouse_x = Input.GetAxis("Mouse X");

        if (Input.GetKey(KeyCode.W))
            _speed += 0.05f;
        else if (Input.GetKey(KeyCode.S))
            _speed -= 0.05f;

        //Sets sight firection by means of transform.Rotate
        _rotationAngle += mouse_x * _angularSpeed * Time.deltaTime;
        //transform.Rotate(new Vector3(0, _rotationAngle, 0));
        transform.Rotate(0, _rotationAngle, 0);

        //Translate is one of transformatioins that uses Vector3
        //transform.Translate(Vector3.forward * Time.deltaTime * _speed);

        //We shall use CharacterController to move and to stop if camera collides with another object
        Vector3 direction = transform.TransformDirection(Vector3.forward * Time.deltaTime * _speed);
        _character.Move(direction);
    }
}