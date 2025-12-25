using UnityEngine;

//Code provided by yours truly: Ştefan Francesco Federico / A.K.A Federico / A.K.A Ştefan cel Mare
public class RagdollOptimization : MonoBehaviour
{
    //When a player is dead in survival this is feature is used for "optimization" of ragdolls, which disables physics from a distance..
    //..then enables it when near

    //Should be added alongside the Kit_ThirdPersonModernPlayerModel

    [Header("More info in code, commented")]
    //Required, else how would you know about about the playermodel?!
    public Kit_ThirdPersonModernPlayerModel thirdPersonPlayerModel;
}
