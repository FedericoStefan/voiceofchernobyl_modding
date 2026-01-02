using System.Collections.Generic;
using UnityEngine;

//Code provided by yours truly: Ştefan Francesco Federico / A.K.A Federico / A.K.A Ştefan cel Mare
public class Kit_ThirdPersonModernPlayerModel : MonoBehaviour
{
    //This is the chaotic roots of the playermodel, and you shall know everything..
    //..this needs to be added most top possible, it shouldn't be hidden..

    //If you have a scheme let's say: 'PLAYERMODEL' - 'BODY' (ANIMATOR) - 'ARMATURE'
    //..then this should be added to 'PLAYERMODEL', this scheme above is how base playermodels work

    //90% of the stuff in here needs to be assigned

    [Header("More info in code, commented")]

    [Header("Important information")]
    //All these should be assigned otherwise nothing will work

    //Look into their code for more info
    public SkinnedDecals.SkinnedDecalSystem DecalSystem;
    public Kit_ThirdPersonModernIKRelay ikSystem;

    //Used for head hitboxes (helmet)
    public Kit_PlayerDamageMultiplier pdm;
    public Kit_PlayerDamageMultiplier pdm2;
    public Kit_PlayerDamageMultiplier pdm3;

    //Outlines for team modes
    public Outline outlinner;

    //Animator should be Humanoid, Apply Root Motion - 'Off', Update Mode - 'Normal', Culling Mode - 'Cull Update Transforms'
    //..don't worry about the Controller because that is set automatically by voc, you could also 100% mod the Controller
    //..if you want a custom character with custom animations, or just the latter
    public Animator anim;

    //Used for infected in infection
    public string animRunTimeController = "";

    [Header("Skinned Meshes / Meshes / Materials")]
    //Necessary as is used for hiding and showing renders in first / third person, add only skinned meshes in here..
    //..as for things that get spawned (like weapons, or other cosmetics) are added automatically
    public List<Renderer> fpShadowOnlyRenderers = new List<Renderer>();

    //Used for TDM (optional)
    public SkinnedMeshRenderer vestmesh;

    //Used for setting up certain cosmetics (necessary)
    public SkinnedMeshRenderer Cosmetics_HANDS, Cosmetics_SHOES, Cosmetics_PANTS, Cosmetics_SHIRTS, Cosmetics_BACKPACK;

    //If you want different base bodies for different sexes (optional)
    public SkinnedMeshRenderer baseBody;
    public Mesh maleBaseBody, femaleBaseBody;
    public Material maleBaseBodyMaterial, femaleBaseBodyMaterial;

    //If you want different heads for different sexes, how base VoC does (optional)
    public SkinnedMeshRenderer headmesh;
    public Material headdead, headdeadFemale;
    public Material headalive, headAliveFemale;

    //Used for hand made exoskeleton (optional)
    public SkinnedMeshRenderer exoskeleton1;

    [Header("Collisions / Physics")]
    //100% Necessary
    public Collider[] raycastColliders;
    public Rigidbody[] rigidbodies;

    //Please follow these schemes otherwise things will look wrong
    // Raycast Colliders (13 in total)
    //  -0 = hips
    //  -1 = left up leg / left thigh
    //  -2 = left leg / left calf
    //  -3 = right up leg / right thigh
    //  -4 = right leg / right calf
    //  -5 = spine 1
    //  -6 = left arm
    //  -7 = left forearm
    //  -8 = head frontal lobe
    //  -9 = head jaw
    //  -10 = head back
    //  -11 = right arm
    //  -12 = right forearm
    //  -13 = left foot
    //  -14 = right foot
    //  -15 = left hand
    //  -16 = right hand
    //  -17 = spine0
    //
    // Rigidbody (11 in total)
    //  -0 = hips
    //  -1 = left up leg / left thigh
    //  -2 = left leg / left calf
    //  -3 = right up leg / right thigh
    //  -4 = right leg / right calf
    //  -5 = spine 1
    //  -6 = left arm
    //  -7 = left forearm
    //  -8 = right arm
    //  -9 = right forearm
    //  -10 = head
    //  -11 = spine0

    [Header("Limbs")]
    //All of this is necessary except for Spine2, because for looking around, normally if everything is assigned then
    // Spine0 - Spine1 - Spine2 - Head
    //....if spine 2 is non existant, then
    // Spine0 - Spine1 - Neck - Head
    //....but you should assign everything when possible, otherwise things wont look / work well
    public Transform rarm;
    public Transform larm;
    public Transform rleg;
    public Transform lleg;
    public Transform rknee;
    public Transform lknee;
    public Transform rshoulder;
    public Transform lshoulder;
    public Transform head;
    public Transform neck;
    public Transform spine0;
    public Transform spine1;
    public Transform spine2;
    public Transform hips;
    public Transform armature;

    //Should be inside the right hand of the playermodel
    public Transform weaponsInHandsGo;

    [Header("Size Changes")]
    public bool skipAdditionalSizeDifferences;
    //If skipAdditionalSizeDifferences is true, then nothing regarding playermodel changes.
    //If skipAdditionalSizeDifferences is false, then playermodel will change some limb sizes depending on sex.
    public float armatureSizeMale = 1, armatureSizeFemale = 1;
    public float hipsSizeMale = 1, hipsSizeFemale = 0.96586f;
    public float headSizeMale = 0.7842726f, headSizeFemale = 0.7842726f;
    public float neckSizeMale = 1.181471f, neckSizeFemale = 1.181471f;

    [Header("Sound Sources")]
    //Necessary, how else would you play audio?!
    public AudioSource soundFire;
    public AudioSource soundReload;
    public AudioSource soundOther;
    public AudioSource soundVoice;
    public AudioSource deadvoice;

    [Header("Additional Transforms")]
    //All of these are necessary
    
    //Used for fp death
    public Transform fpCamDeath;

    //Used for spectating when dead in survival
    public Transform tauntPOS;

    //Used for showing names when looking at someone / in a team with them
    public Transform enemyNameAboveHeadPos;

    //Used for 'head' cosmetics, like hats, facial accessories, beards, ect..
    public Transform accessories;

    //Used for when a weapon is not equipped and is holstered
    public Transform meleePos;
    public Transform pistolPos;
    public Transform riflePos;

    [Header("Gibbing Transforms")]
    //Not obbligated to set these, if you don't set these you just don't get gibbing of what you did not assign
    public Transform GIB_Knife_Spawn;
    public Transform GIB_Hammer_Spawn;
    public Transform GIB_Head_Spawn;
    public Transform GIB_LeftArm_Spawn;
    public Transform GIB_RightArm_Spawn;
    public Transform GIB_LeftLeg_Spawn;
    public Transform GIB_RightLeg_Spawn;

    //[Header("Sounds")]
    //public AudioClip[] headshotdie_male;
    //public AudioClip[] headshotdie_female;
    //public AudioClip TruckMurder;
    //public AudioClip screamfalldmg;
    //public AudioClip explosiondmgsound;

    [Header("Lights")]
    public Light torchLight;
}
