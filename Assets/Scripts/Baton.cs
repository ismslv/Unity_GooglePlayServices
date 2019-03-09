using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baton : MonoBehaviour
{
    private Transform root;
    private Animator animator;

    void Start() {
        root = transform.parent;
        animator = transform.GetComponentInParent<Animator>();
    }

    void OnMouseDown() {
        root.eulerAngles = new Vector3(0, Random.Range(-10f, 10f), 0);
        animator.SetTrigger("Push");
        Core.a.OnBatonClicked();
    }
}
