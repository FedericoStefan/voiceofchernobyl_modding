using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class C_Hands
{
    public Transform Arm;
    public Transform Forearm;
    public Transform Palm;
    [Header("Thumb")]
    public Transform Thumb_01;
    public Transform Thumb_02;
    public Transform Thumb_03;
    [Header("Index")]
    public Transform Index_01;
    public Transform Index_02;
    public Transform Index_03;
    [Header("Middle")]
    public Transform Middle_01;
    public Transform Middle_02;
    public Transform Middle_03;
    [Header("Ring")]
    public Transform Ring_01;
    public Transform Ring_02;
    public Transform Ring_03;
    [Header("Pinky")]
    public Transform Pinky_01;
    public Transform Pinky_02;
    public Transform Pinky_03;
}

public class InitializeC_Hands : MonoBehaviour
{
    public SkinnedMeshRenderer Heavy_Jacket;
    public SkinnedMeshRenderer Shirtless;
    public SkinnedMeshRenderer T_Shirt;
    public SkinnedMeshRenderer Jacket;
    public SkinnedMeshRenderer Closed_Coat;
    public SkinnedMeshRenderer Trench_Coat;
    public SkinnedMeshRenderer King;
    public SkinnedMeshRenderer Tuxedo;
    public SkinnedMeshRenderer Hands;
    public SkinnedMeshRenderer Full_Glove1;
    public SkinnedMeshRenderer Full_Glove2;
    public SkinnedMeshRenderer ShortGlove;
    public SkinnedMeshRenderer BIG_Hand;
    public MeshRenderer Bandaged;
    public C_Hands L_hand;
    public C_Hands R_hand;
}