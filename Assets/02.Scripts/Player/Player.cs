using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    [field: Header("Animations")]
    [field: SerializeField] public PlayerAnimationData animationData { get; private set; }

    public Animator animator { get; private set; }
    public PlayerController input { get; private set; }
    public Rigidbody controller { get; private set; }

    private void Awake()
    {
        animationData.Initialize();
        animator = GetComponent<Animator>();
        input = GetComponent<PlayerController>();
        controller = GetComponent<Rigidbody>();
    }


}
