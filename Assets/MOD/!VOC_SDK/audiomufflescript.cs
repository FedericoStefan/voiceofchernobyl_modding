using UnityEngine;

//Code provided by yours truly: Ştefan Francesco Federico / A.K.A Federico / A.K.A Ştefan cel Mare
public class audiomufflescript : MonoBehaviour
{
    //Used for muffling audio at a certain distance

    //if you assign ALPF it will use that
    //if not it will try to find it by doing get component
    //if it still doesn't exist then it adds one.
    public AudioLowPassFilter alpf;

    public float CloseCutOff = 21931f;
    public float FarCutOff = 274;
    public float CloseDistance = 6f;
    public float FarDistance = 32f;
}