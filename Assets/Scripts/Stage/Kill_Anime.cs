using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kill_Anime : MonoBehaviour
{
    public Animator animator;
    
    public void Kill_End()
    {
        Destroy(transform.root.gameObject);
    }
}
