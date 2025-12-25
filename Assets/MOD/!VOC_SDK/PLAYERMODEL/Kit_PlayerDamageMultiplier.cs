using UnityEngine;

//Code provided by yours truly: Ştefan Francesco Federico / A.K.A Federico / A.K.A Ştefan cel Mare
public class Kit_PlayerDamageMultiplier : MonoBehaviour
{
    //This is hitbox code, so add it to any hitboxes you want (needs to have a collider too)

    [Header("More info in code, commented")]
    //This is the most important, it is what determines a hit box's damage (it's hard coded for now)
    // - Make sure ABSOLUTELY to have the same hitbox / ragdoll / collision counts as the base playermodels do (depending on which you're working on)
    // - ^ If this is not the case, you will have problems going on when a player dies / spawns
    // - I want you to follow the same hitbox IDs as the base playermodel do, I'd be happy if you follow that rule :D
    public int ragdollId;

    //Necessary, otherwise how are you going to get the playermodel when hit?!
    public Kit_ThirdPersonModernPlayerModel tp;

    //Gibbing / killfeed purposes
    public bool head = false;
    public bool isFrontal;
    public bool isJaw;
    public bool isBack;
}
