using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownProjectile : MonoBehaviour
{
    [HideInInspector]
    public float damage;
    [HideInInspector]
    public string myDamageType;
    [HideInInspector]
    public GameObject myPickUp;
    [HideInInspector]
    public NeedSounds SoundManager;

    [HideInInspector]
    public Interactable interactable;
    [HideInInspector]
    public Outline outline;
    [HideInInspector]
    public SphereCollider sphereCollider;
    [HideInInspector]
    public bool isInPickUpState;
    [HideInInspector]
    public Rigidbody rb;


    private Transform[] AllChildren;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
        outline = GetComponent<Outline>();
        sphereCollider = GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();

        if (outline != null)
        {
            outline.OutlineColor = new Color(outline.OutlineColor.r, outline.OutlineColor.g, outline.OutlineColor.b, 0);
            outline.OutlineMode = Outline.Mode.OutlineVisible;
        }

        AllChildren = GetComponentsInChildren<Transform>();

        PickUpSetActive(true);
    }
    /*
    private void Start()
    {
        print("started");
        SoundManager = GetComponent<NeedSounds>();
        
    }

    private void LateStart()
    {
        SoundManager.Play("throw");
    }
    */

    public void PickUpSetActive(bool value)
    {
        isInPickUpState = value;

        if (interactable != null)
            interactable.enabled = value;

        if (sphereCollider != null)
            sphereCollider.enabled = value;

        if (value)
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        else
            gameObject.layer = LayerMask.NameToLayer("NoPlayerCollision");

        
        foreach (Transform child in AllChildren)
            child.gameObject.layer = value ? LayerMask.NameToLayer("Interactable") : LayerMask.NameToLayer("NoPlayerCollision");

    }
}
