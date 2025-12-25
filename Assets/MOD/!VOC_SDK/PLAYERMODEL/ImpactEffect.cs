using UnityEngine;

//Code provided by yours truly: Ştefan Francesco Federico / A.K.A Federico / A.K.A Ştefan cel Mare
public class ImpactEffect : MonoBehaviour
{
    //Ok so the way this works is, upon any collision enter, some math is done and then an attempt will be done..
    //..the attempt basically tries to get the audiosource (that is attached alongside this script), if not existant..
    //..then nothing will happen

    //You should add it to a limb, that has a collider and a rigidbody, so when ragdoll is on you hear impact sounds

    [Header("More info in code, commented")]
    //Add as much stuff as you want in here (can be either nothing, or a quantillion sounds, as much as you wish!)
    public AudioClip[] impactSounds;

    //If this is set inside a playermodel (ex, a limb) then set this
    public Kit_ThirdPersonModernPlayerModel playerModel;

    //This works ONLY if 'playerModel' is not set
    public float impactAudioModifier = 1.0f;
}
