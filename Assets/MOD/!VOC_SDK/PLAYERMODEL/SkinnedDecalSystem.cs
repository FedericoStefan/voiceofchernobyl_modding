using UnityEngine;

namespace SkinnedDecals
{
    //Code provided by yours truly: Ştefan Francesco Federico / A.K.A Federico / A.K.A Ştefan cel Mare
    public class SkinnedDecalSystem : MonoBehaviour
    {
        //Upon a player getting damaged, and is valid then this code will be called containing hit information..
        //..and thus, blood decals will be shown on the playermodel (skinned meshes only, so it looks natural)

        [Header("More info in code, commented")]

        //Contains all skinned meshes to have decals
        public SkinnedMeshRenderer[] skinnedMeshes = new SkinnedMeshRenderer[0];
    }
}
