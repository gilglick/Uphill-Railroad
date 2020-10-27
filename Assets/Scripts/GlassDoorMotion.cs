using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassDoorMotion : MonoBehaviour
{
    Animator animator;
    bool isOpen;
    // Start is called before the first frame update
    void Start()
    {
        isOpen = false;
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            isOpen = true;
            animator.SetTrigger("GlassOpen");

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (isOpen)
        {
            isOpen = false;
            animator.SetTrigger("GlassClose");
        }
    }
}