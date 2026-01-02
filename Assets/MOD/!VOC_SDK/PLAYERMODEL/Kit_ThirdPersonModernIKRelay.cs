/*using UnityEngine;

//Code provided by yours truly: Ştefan Francesco Federico / A.K.A Federico / A.K.A Ştefan cel Mare
public class Kit_ThirdPersonModernIKRelay : MonoBehaviour
{
    //There is nothing inside here because its all handled by VoC
    //This code handles IK (like left hand for guns and certain melees) and also the new looking system added (which is handled by VoC too)
    //Add this alongside the animator component
}

*/
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct AimOffsetBone
{
    public Transform bone;
    public Vector2 maxAngle;
    public Vector2 Angle0;
    public Vector2 Angle1;
}

[Serializable]
public struct AimOffset
{
    public List<AimOffsetBone> bones;
    public int indexOffset;

    [NonSerialized] public List<Vector2> angles;

    public void Init()
    {
        if (angles == null)
        {
            angles = new List<Vector2>();
        }
        else
        {
            angles.Clear();
        }

        bones ??= new List<AimOffsetBone>();

        for (int i = 0; i < bones.Count - indexOffset; i++)
        {
            var bone = bones[i];
            angles.Add(bone.maxAngle);
        }
    }

    public bool IsValid()
    {
        return bones != null && angles != null;
    }

    public bool IsChanged()
    {
        return bones.Count != angles.Count;
    }
}

[Serializable]
public struct DynamicBone
{
    public Transform target;
    public Transform hintTarget;
    public GameObject obj;

    public void Retarget()
    {
        if (target == null)
        {
            return;
        }

        obj.transform.position = target.position;
        obj.transform.rotation = target.rotation;
    }
}

// Used for detecting zero-frames
[Serializable]
public struct CachedBones
{
    public (Vector3, Quaternion) pelvis;
    public (Vector3, Quaternion) rightFoot;
    public (Vector3, Quaternion) leftFoot;

    public List<Quaternion> lookUp;
}

public class Kit_ThirdPersonModernIKRelay : MonoBehaviour
{
    //0 = move spine like rifle or pistol
    //1 = move neck and head and slightly spine like unarmed
    public int currentAngleSlot;
    public Kit_ThirdPersonModernPlayerModel pm;

    public void OnAnimatorIK()
    {
        Debug.Log("OnAnimatorIK");
    }

    [Header("Layer Blending")]
    [SerializeField, Range(0f, 1f)]
    private float layerAlpha;

    [SerializeField] private float layerInterpSpeed;
    [SerializeField, Range(0f, 1f)] private float handsLayerAlpha;
    [SerializeField] private float handsLerpSpeed;

    [SerializeField, Range(0f, 1f)] private float pelvisAlpha;
    [SerializeField] private float pelvisLerpSpeed;
    private float _smoothPelvisAlpha;

    [Header("Rig")]
    [SerializeField] public Transform pelvis;
    [SerializeField] private DynamicBone rightHand;
    [SerializeField] private DynamicBone leftHand;
    [SerializeField] private DynamicBone rightFoot;
    [SerializeField] private DynamicBone leftFoot;
    [SerializeField] private bool useIK = true;

    [Tooltip("Used for mesh space calculations")]
    [SerializeField]
    public Transform rootBone;

    [Header("Offsets")][SerializeField] private Vector3 pelvisOffset;
    [SerializeField] private Vector3 handsOffset;

    [Header("Aim Offset")]

    [SerializeField]
    public AimOffset lookUpOffset;

    [SerializeField]
    public AimOffset lookRightOffset;

    [SerializeField] private bool enableAutoDistribution;
    [SerializeField] private bool enableManualSpineControl;
    [SerializeField, Range(-90f, 90f)] public float aimUp;
    [SerializeField, Range(-90f, 90f)] public float aimRight;
    [SerializeField] private float smoothAim;

    [Header("Leaning")]
    [SerializeField][Range(-1, 1)] private int leanDirection;
    [SerializeField] private float leanAmount;
    [SerializeField] private float leanSpeed;
    private float _smoothLean;

    [Header("Misc")]
    [SerializeField] private bool drawDebug;
    [SerializeField] private bool detectZeroFrames = true;
    [SerializeField] private bool useRightOffset = true;

    private Animator _animator;
    private bool _updateInEditor;
    private bool _lastUseMeshSpaceHands;
    private float _interpHands;
    private float _interpLayer;
    private Vector2 _smoothAim;

    // Used to detect zero key frames
    [SerializeField][HideInInspector] private CachedBones cachedBones;
    [SerializeField][HideInInspector] private CachedBones cachedLocal;
    [SerializeField][HideInInspector] private CachedBones cacheRef;

    private void Awake()
    {
        lookUpOffset.Init();
        lookRightOffset.Init();

        if (cachedBones.lookUp == null)
        {
            cachedBones.lookUp ??= new List<Quaternion>();
            cachedLocal.lookUp ??= new List<Quaternion>();
            cacheRef.lookUp ??= new List<Quaternion>();

            for (int i = 0; i < lookUpOffset.bones.Count; i++)
            {
                cachedBones.lookUp.Add(Quaternion.identity);
                cachedLocal.lookUp.Add(Quaternion.identity);
                cacheRef.lookUp.Add(Quaternion.identity);
            }
        }

        _animator = GetComponent<Animator>();
    }

    public void OnValidateFunction()
    {
        if (!lookUpOffset.IsValid() || lookUpOffset.IsChanged())
        {
            lookUpOffset.Init();

            cachedBones.lookUp.Clear();
            cachedLocal.lookUp.Clear();
            cacheRef.lookUp.Clear();

            for (int i = 0; i < lookUpOffset.bones.Count; i++)
            {
                cachedBones.lookUp.Add(Quaternion.identity);
                cachedLocal.lookUp.Add(Quaternion.identity);
                cacheRef.lookUp.Add(Quaternion.identity);
            }
        }

        if (!lookRightOffset.IsValid() || lookRightOffset.IsChanged())
        {
            lookRightOffset.Init();
        }

        void Distribute(ref AimOffset aimOffset)
        {
            if (enableAutoDistribution)
            {
                bool enable = false;
                int divider = 1;
                float sum = 0f;

                for (int i = 0; i < aimOffset.bones.Count - aimOffset.indexOffset; i++)
                {
                    if (enable)
                    {
                        var bone = aimOffset.bones[i];
                        bone.maxAngle.x = (90f - sum) / divider;
                        aimOffset.bones[i] = bone;
                        continue;
                    }

                    if (!Mathf.Approximately(aimOffset.bones[i].maxAngle.x, aimOffset.angles[i].x))
                    {
                        divider = aimOffset.bones.Count - aimOffset.indexOffset - (i + 1);
                        enable = true;
                    }

                    sum += aimOffset.bones[i].maxAngle.x;
                }
            }

            if (enableAutoDistribution)
            {
                bool enable = false;
                int divider = 1;
                float sum = 0f;

                for (int i = 0; i < aimOffset.bones.Count - aimOffset.indexOffset; i++)
                {
                    if (enable)
                    {
                        var bone = aimOffset.bones[i];
                        bone.maxAngle.y = (90f - sum) / divider;
                        aimOffset.bones[i] = bone;
                        continue;
                    }

                    if (!Mathf.Approximately(aimOffset.bones[i].maxAngle.y, aimOffset.angles[i].y))
                    {
                        divider = aimOffset.bones.Count - aimOffset.indexOffset - (i + 1);
                        enable = true;
                    }

                    sum += aimOffset.bones[i].maxAngle.y;
                }
            }

            for (int i = 0; i < aimOffset.bones.Count - aimOffset.indexOffset; i++)
            {
                aimOffset.angles[i] = aimOffset.bones[i].maxAngle;
            }
        }

        if (lookUpOffset.bones.Count > 0)
        {
            Distribute(ref lookUpOffset);
        }

        if (lookRightOffset.bones.Count > 0)
        {
            Distribute(ref lookRightOffset);
        }
    }

    private void ApplyAimOffset()
    {
        _smoothLean = _ANIM_IK_SHIT.Glerp(_smoothLean, leanDirection, leanSpeed);

        if (!useRightOffset)
        {
            aimRight = 0f;
        }

        _smoothAim.y = _ANIM_IK_SHIT.GlerpLayer(_smoothAim.y, -aimUp, smoothAim);
        _smoothAim.x = _ANIM_IK_SHIT.GlerpLayer(_smoothAim.x, aimRight, smoothAim);

        foreach (var bone in lookRightOffset.bones)
        {
            if (!Application.isPlaying && bone.bone == null)
            {
                continue;
            }

            float angleFraction = _smoothAim.x >= 0f ? bone.maxAngle.y : bone.maxAngle.x;
            _ANIM_IK_SHIT.RotateInBoneSpace(rootBone.rotation, bone.bone,
                new Vector3(0f, _smoothAim.x * _interpLayer / (90f / angleFraction), 0f));
        }

        foreach (var bone in lookRightOffset.bones)
        {
            if (!Application.isPlaying && bone.bone == null)
            {
                continue;
            }

            float angleFraction = _smoothAim.x >= 0f ? bone.maxAngle.y : bone.maxAngle.x;
            _ANIM_IK_SHIT.RotateInBoneSpace(rootBone.rotation * Quaternion.Euler(0f, _smoothAim.x, 0f), bone.bone,
                new Vector3(0f, 0f, leanAmount * _smoothLean * _interpLayer / (90f / angleFraction)));
        }

        if (rightHand.obj && leftHand.obj)
        {
            Vector3 rightHandLoc = rightHand.obj.transform.position;
            Quaternion rightHandRot = rightHand.obj.transform.rotation;

            Vector3 leftHandLoc = leftHand.obj.transform.position;
            Quaternion leftHandRot = leftHand.obj.transform.rotation;

            void InterpHands()
            {
                _interpHands = _ANIM_IK_SHIT.GlerpLayer(_interpHands, handsLayerAlpha, handsLerpSpeed);

                rightHand.obj.transform.position = Vector3.Lerp(rightHandLoc, rightHand.obj.transform.position,
                    _interpHands);
                rightHand.obj.transform.rotation = Quaternion.Slerp(rightHandRot, rightHand.obj.transform.rotation,
                    _interpHands);

                leftHand.obj.transform.position = Vector3.Lerp(leftHandLoc, leftHand.obj.transform.position,
                    _interpHands);
                leftHand.obj.transform.rotation = Quaternion.Slerp(leftHandRot, leftHand.obj.transform.rotation,
                    _interpHands);
            }

            InterpHands();
        }


        foreach (var bone in lookUpOffset.bones)
        {
            if (!Application.isPlaying && bone.bone == null)
            {
                continue;
            }

            float angleFraction = _smoothAim.y >= 0f ? bone.maxAngle.y : bone.maxAngle.x;

            _ANIM_IK_SHIT.RotateInBoneSpace(rootBone.rotation * Quaternion.Euler(0f, _smoothAim.x, 0f), bone.bone,
                new Vector3(_smoothAim.y * _interpLayer / (90f / angleFraction), 0f, 0f));
        }
    }

    private void Retarget()
    {
        rightHand.Retarget();
        leftHand.Retarget();
        rightFoot.Retarget();
        leftFoot.Retarget();
    }

    private void ApplyProceduralLayer()
    {
        Vector3 handsFinal = Vector3.Lerp(Vector3.zero, handsOffset, _interpLayer);
        Vector3 pelvisFinal = Vector3.Lerp(Vector3.zero, pelvisOffset, _interpLayer);

        if (leftHand.obj)
            _ANIM_IK_SHIT.MoveInBoneSpace(rootBone, leftHand.obj.transform,
                handsFinal);

        if (rightHand.obj)
            _ANIM_IK_SHIT.MoveInBoneSpace(rootBone, rightHand.obj.transform,
            handsFinal);

        _smoothPelvisAlpha = _ANIM_IK_SHIT.GlerpLayer(_smoothPelvisAlpha, pelvisAlpha, pelvisLerpSpeed);

        _ANIM_IK_SHIT.MoveInBoneSpace(rootBone, pelvis,
            Vector3.Lerp(Vector3.zero, pelvisFinal, _smoothPelvisAlpha));

        ApplyAimOffset();
    }

    private void ApplyIK()
    {
        if (!useIK)
        {
            return;
        }

        Transform lowerBone = rightHand.target.parent;

        // todo optimization

        _ANIM_IK_SHIT.SolveTwoBoneIK(lowerBone.parent, lowerBone, rightHand.target,
            rightHand.obj.transform, rightHand.hintTarget, 1f, 1f, 1f);

        lowerBone = leftHand.target.parent;

        _ANIM_IK_SHIT.SolveTwoBoneIK(lowerBone.parent, lowerBone, leftHand.target,
            leftHand.obj.transform, leftHand.hintTarget, 1f, 1f, 1f);

        lowerBone = rightFoot.target.parent;

        _ANIM_IK_SHIT.SolveTwoBoneIK(lowerBone.parent, lowerBone, rightFoot.target,
            rightFoot.obj.transform, rightFoot.hintTarget, 1f, 1f, 1f);

        lowerBone = leftFoot.target.parent;

        _ANIM_IK_SHIT.SolveTwoBoneIK(lowerBone.parent, lowerBone, leftFoot.target,
            leftFoot.obj.transform, leftFoot.hintTarget, 1f, 1f, 1f);
    }

    private bool BlendLayers()
    {
        _interpLayer = _ANIM_IK_SHIT.GlerpLayer(_interpLayer, layerAlpha, layerInterpSpeed);
        return Mathf.Approximately(_interpLayer, 0f);
    }

    // If bone transform is the same - zero frame
    // Use cached data to prevent continuous translation/rotation
    private void CheckZeroFrames()
    {
        if (cachedBones.pelvis.Item1 == pelvis.localPosition)
        {
            pelvis.localPosition = cachedLocal.pelvis.Item1;
        }

        bool bZeroSpine = false;
        for (int i = 0; i < cachedBones.lookUp.Count; i++)
        {
            var bone = lookUpOffset.bones[i].bone;
            if (bone == null)
            {
                continue;
            }

            if (cachedBones.lookUp[i] == bone.localRotation)
            {
                bZeroSpine = true;
                bone.localRotation = cachedLocal.lookUp[i];
            }
        }

        if (bZeroSpine)
        {
            rightHand.Retarget();
            leftHand.Retarget();
            rightFoot.Retarget();
            leftFoot.Retarget();
        }

        cacheRef.pelvis.Item1 = pelvis.localPosition;
        cacheRef.pelvis.Item2 = pelvis.localRotation;

        for (int i = 0; i < lookUpOffset.bones.Count; i++)
        {
            var bone = lookUpOffset.bones[i].bone;
            if (bone == null)
            {
                continue;
            }

            cacheRef.lookUp[i] = bone.localRotation;
        }
    }

    private void CacheBones()
    {
        cachedBones.pelvis.Item1 = pelvis.localPosition;
        cachedLocal.pelvis.Item1 = cacheRef.pelvis.Item1;

        for (int i = 0; i < cachedBones.lookUp.Count; i++)
        {
            var bone = lookUpOffset.bones[i].bone;
            if (bone == null)
            {
                continue;
            }

            cachedBones.lookUp[i] = bone.localRotation;
            cachedLocal.lookUp[i] = cacheRef.lookUp[i];
        }
    }

    public void Update()
    {
        if (!Application.isPlaying && _updateInEditor)
        {
            if (_animator == null)
            {
                return;
            }

            _animator.Update(Time.deltaTime);
        }
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (drawDebug)
        {
            Gizmos.color = Color.green;

            void DrawDynamicBone(ref DynamicBone bone, string boneName)
            {
                if (bone.obj != null)
                {
                    var loc = bone.obj.transform.position;
                    Gizmos.DrawWireSphere(loc, 0.06f);
                    Handles.Label(loc, boneName);
                }
            }

            DrawDynamicBone(ref rightHand, "RightHandIK");
            DrawDynamicBone(ref leftHand, "LeftHandIK");
            DrawDynamicBone(ref rightFoot, "RightFootIK");
            DrawDynamicBone(ref leftFoot, "LeftFootIK");

            Gizmos.color = Color.blue;
            if (rootBone != null)
            {
                var mainBone = rootBone.position;
                Gizmos.DrawWireCube(mainBone, new Vector3(0.1f, 0.1f, 0.1f));
                Handles.Label(mainBone, "rootBone");
            }
        }

        if (!Application.isPlaying)
        {
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
    }
#endif

    public void LateUpdate()
    {
        if (!Application.isPlaying && !_updateInEditor)
        {
            return;
        }

        if (BlendLayers())
        {
            return;
        }

        //Debug.Log("updating ik");

        //if (Application.isEditor && detectZeroFrames)
        //{
        Retarget();
        CheckZeroFrames();
        ApplyProceduralLayer();
        ApplyIK();
        CacheBones();
        //}
        //else
        //{
        //    Retarget();
        //    ApplyProceduralLayer();
        //    ApplyIK();
        //}
    }

    public void SetPelvisAlpha(float weight)
    {
        pelvisAlpha = Mathf.Clamp01(weight);
    }

    public void SetAimRotation(Vector2 newAimRot)
    {
        if (!enableManualSpineControl)
        {
            aimUp += newAimRot.x;
            aimRight += newAimRot.y;

            aimUp = Mathf.Clamp(aimUp, -90f, 90f);
            aimRight = Mathf.Clamp(aimRight, -90f, 90f);
        }
    }

    public void SetHandsOffset(Vector3 offset)
    {
        handsOffset = offset;
    }

    public void SetHipsOffset(Vector3 offset)
    {
        pelvisOffset = offset;
    }

    // 0 - no effect, 1 - fully applied
    public void SetBlendAlpha(float alpha)
    {
        layerAlpha = Mathf.Clamp01(alpha);
    }

    // 0 - Hands aren't affected by spine/neck/head rotations
    // 1 - Hands are fully affected by spine/neck/head rotations
    public void SetHandsBlendAlpha(float alpha)
    {
        handsLayerAlpha = Mathf.Clamp01(alpha);
    }

    // -1: left, 1: right, 0: no lean
    public void SetLeanDirection(int direction)
    {
        leanDirection = 0;
        if (direction < 0)
        {
            leanDirection = -1;
        }
        else if (direction > 0)
        {
            leanDirection = 1;
        }
    }

    public void EnableEditorPreview()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _updateInEditor = true;
    }

    public void DisableEditorPreview()
    {
        _updateInEditor = false;

        if (_animator == null)
        {
            return;
        }

        _animator.Rebind();
        _animator.Update(0f);
    }

    public void SetupBones()
    {
        if (rootBone == null)
        {
            var root = transform.Find("rootBone");

            if (root != null)
            {
                rootBone = root.transform;
            }
            else
            {
                var bone = new GameObject("rootBone");
                bone.transform.parent = transform;
                rootBone = bone.transform;
                rootBone.localPosition = Vector3.zero;
            }
        }

        if (rightFoot.obj == null)
        {
            var bone = transform.Find("RightFootIK");

            if (bone != null)
            {
                rightFoot.obj = bone.gameObject;
            }
            else
            {
                rightFoot.obj = new GameObject("RightFootIK");
                rightFoot.obj.transform.parent = transform;
                rightFoot.obj.transform.localPosition = Vector3.zero;
            }
        }

        if (leftFoot.obj == null)
        {
            var bone = transform.Find("LeftFootIK");

            if (bone != null)
            {
                leftFoot.obj = bone.gameObject;
            }
            else
            {
                leftFoot.obj = new GameObject("LeftFootIK");
                leftFoot.obj.transform.parent = transform;
                leftFoot.obj.transform.localPosition = Vector3.zero;
            }
        }

        var children = transform.GetComponentsInChildren<Transform>(true);

        bool foundRightHand = false;
        bool foundLeftHand = false;
        bool foundRightFoot = false;
        bool foundLeftFoot = false;
        bool foundHead = false;
        bool foundPelvis = false;

        foreach (var bone in children)
        {
            if (bone.name.ToLower().Contains("ik"))
            {
                continue;
            }

            bool bMatches = bone.name.ToLower().Contains("hips") || bone.name.ToLower().Contains("pelvis");
            if (!foundPelvis && bMatches)
            {
                pelvis = bone;
                foundPelvis = true;
                continue;
            }

            bMatches = bone.name.ToLower().Contains("lefthand") || bone.name.ToLower().Contains("hand_l")
                                                                || bone.name.ToLower().Contains("hand l")
                                                                || bone.name.ToLower().Contains("l hand")
                                                                || bone.name.ToLower().Contains("l.hand")
                                                                || bone.name.ToLower().Contains("hand.l");
            if (!foundLeftHand && bMatches)
            {
                leftHand.target = bone;

                if (leftHand.hintTarget == null)
                {
                    leftHand.hintTarget = bone.parent;
                }

                foundLeftHand = true;
                continue;
            }

            bMatches = bone.name.ToLower().Contains("righthand") || bone.name.ToLower().Contains("hand_r")
                                                                 || bone.name.ToLower().Contains("hand r")
                                                                 || bone.name.ToLower().Contains("r hand")
                                                                 || bone.name.ToLower().Contains("r.hand")
                                                                 || bone.name.ToLower().Contains("hand.r");
            if (!foundRightHand && bMatches)
            {
                rightHand.target = bone;

                if (rightHand.hintTarget == null)
                {
                    rightHand.hintTarget = bone.parent;
                }

                foundRightHand = true;
            }

            bMatches = bone.name.ToLower().Contains("rightfoot") || bone.name.ToLower().Contains("foot_r")
                                                                 || bone.name.ToLower().Contains("foot r")
                                                                 || bone.name.ToLower().Contains("r foot")
                                                                 || bone.name.ToLower().Contains("r.foot")
                                                                 || bone.name.ToLower().Contains("foot.r");
            if (!foundRightFoot && bMatches)
            {
                rightFoot.target = bone;
                rightFoot.hintTarget = bone.parent;

                foundRightFoot = true;
            }

            bMatches = bone.name.ToLower().Contains("leftfoot") || bone.name.ToLower().Contains("foot_l")
                                                                || bone.name.ToLower().Contains("foot l")
                                                                || bone.name.ToLower().Contains("l foot")
                                                                || bone.name.ToLower().Contains("l.foot")
                                                                || bone.name.ToLower().Contains("foot.l");
            if (!foundLeftFoot && bMatches)
            {
                leftFoot.target = bone;
                leftFoot.hintTarget = bone.parent;

                foundLeftFoot = true;
            }

            if (!foundHead && bone.name.ToLower().Contains("head"))
            {
                if (rightHand.obj == null)
                {
                    var boneObject = bone.transform.Find("RightHandIK");

                    if (boneObject != null)
                    {
                        rightHand.obj = boneObject.gameObject;
                    }
                    else
                    {
                        rightHand.obj = new GameObject("RightHandIK");
                        rightHand.obj.transform.parent = bone;
                        rightHand.obj.transform.localPosition = Vector3.zero;
                    }
                }

                if (leftHand.obj == null)
                {
                    var boneObject = bone.transform.Find("LeftHandIK");

                    if (boneObject != null)
                    {
                        leftHand.obj = boneObject.gameObject;
                    }
                    else
                    {
                        leftHand.obj = new GameObject("LeftHandIK");
                        leftHand.obj.transform.parent = bone;
                        leftHand.obj.transform.localPosition = Vector3.zero;
                    }
                }

                foundHead = true;
            }
        }

        bool bFound = foundRightHand && foundLeftHand && foundRightFoot && foundLeftFoot && foundHead &&
                      foundPelvis;

        Debug.Log(bFound ? "All bones are found!" : "Some bones are missing!");
    }
}


public static class _ANIM_IK_SHIT
{
    private const float FloatMin = 1e-10f;
    private const float SqrEpsilon = 1e-8f;

    // Frame-rate independent interpolation
    public static float Glerp(float a, float b, float speed)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-speed * Time.deltaTime));
    }

    public static float GlerpLayer(float a, float b, float speed)
    {
        return Mathf.Approximately(speed, 0f)
            ? b
            : Mathf.Lerp(a, b, 1 - Mathf.Exp(-speed * Time.deltaTime));
    }

    public static Vector3 Glerp(Vector3 a, Vector3 b, float speed)
    {
        return Vector3.Lerp(a, b, 1 - Mathf.Exp(-speed * Time.deltaTime));
    }

    public static Quaternion Glerp(Quaternion a, Quaternion b, float speed)
    {
        return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-speed * Time.deltaTime));
    }

    public static void RotateInBoneSpace(Quaternion target, Transform boneToRotate, Vector3 rotationAmount)
    {
        var headRot = boneToRotate.rotation;
        var headToMesh = Quaternion.Inverse(target) * headRot;
        var headOffsetRot = target * Quaternion.Euler(rotationAmount);

        var finalRot = headOffsetRot * headToMesh;

        boneToRotate.rotation = finalRot;
    }

    public static void RotateInBoneSpace(Quaternion target, Transform boneToRotate, Quaternion rotationAmount)
    {
        var headRot = boneToRotate.rotation;
        var headToMesh = Quaternion.Inverse(target) * headRot;
        var headOffsetRot = target * rotationAmount;

        var finalRot = headOffsetRot * headToMesh;

        boneToRotate.rotation = finalRot;
    }

    public static void MoveInBoneSpace(Transform target, Transform boneToMove, Vector3 offsetMeshSpace)
    {
        var root = target.transform;
        Vector3 offset = root.TransformPoint(offsetMeshSpace);
        offset -= root.position;

        boneToMove.position += offset;
    }

    //todo create native array with 3 quaternions, these will be used after the job is done

    // Adapted from Two Bone IK constraint, Unity Animation Rigging package
    public static void SolveTwoBoneIK(
        Transform root,
        Transform mid,
        Transform tip,
        Transform target,
        Transform hint,
        float posWeight,
        float rotWeight,
        float hintWeight
    )
    {
        Vector3 aPosition = root.position;
        Vector3 bPosition = mid.position;
        Vector3 cPosition = tip.position;
        Vector3 tPosition = Vector3.Lerp(cPosition, target.position, posWeight);
        Quaternion tRotation = Quaternion.Lerp(tip.rotation, target.rotation, rotWeight);
        bool hasHint = hint != null && hintWeight > 0f;

        Vector3 ab = bPosition - aPosition;
        Vector3 bc = cPosition - bPosition;
        Vector3 ac = cPosition - aPosition;
        Vector3 at = tPosition - aPosition;

        float abLen = ab.magnitude;
        float bcLen = bc.magnitude;
        float acLen = ac.magnitude;
        float atLen = at.magnitude;

        float oldAbcAngle = TriangleAngle(acLen, abLen, bcLen);
        float newAbcAngle = TriangleAngle(atLen, abLen, bcLen);

        // Bend normal strategy is to take whatever has been provided in the animation
        // stream to minimize configuration changes, however if this is collinear
        // try computing a bend normal given the desired target position.
        // If this also fails, try resolving axis using hint if provided.
        Vector3 axis = Vector3.Cross(ab, bc);
        if (axis.sqrMagnitude < SqrEpsilon)
        {
            axis = hasHint ? Vector3.Cross(hint.position - aPosition, bc) : Vector3.zero;

            if (axis.sqrMagnitude < SqrEpsilon)
                axis = Vector3.Cross(at, bc);

            if (axis.sqrMagnitude < SqrEpsilon)
                axis = Vector3.up;
        }

        axis = Vector3.Normalize(axis);

        float a = 0.5f * (oldAbcAngle - newAbcAngle);
        float sin = Mathf.Sin(a);
        float cos = Mathf.Cos(a);
        Quaternion deltaR = new Quaternion(axis.x * sin, axis.y * sin, axis.z * sin, cos);
        mid.rotation = deltaR * mid.rotation;

        cPosition = tip.position;
        ac = cPosition - aPosition;
        root.rotation = FromToRotation(ac, at) * root.rotation;

        if (hasHint)
        {
            float acSqrMag = ac.sqrMagnitude;
            if (acSqrMag > 0f)
            {
                bPosition = mid.position;
                cPosition = tip.position;
                ab = bPosition - aPosition;
                ac = cPosition - aPosition;

                Vector3 acNorm = ac / Mathf.Sqrt(acSqrMag);
                Vector3 ah = hint.position - aPosition;
                Vector3 abProj = ab - acNorm * Vector3.Dot(ab, acNorm);
                Vector3 ahProj = ah - acNorm * Vector3.Dot(ah, acNorm);

                float maxReach = abLen + bcLen;
                if (abProj.sqrMagnitude > (maxReach * maxReach * 0.001f) && ahProj.sqrMagnitude > 0f)
                {
                    Quaternion hintR = FromToRotation(abProj, ahProj);
                    hintR.x *= hintWeight;
                    hintR.y *= hintWeight;
                    hintR.z *= hintWeight;
                    hintR = NormalizeSafe(hintR);
                    root.rotation = hintR * root.rotation;
                }
            }
        }

        tip.rotation = tRotation;
    }

    private static float TriangleAngle(float aLen, float aLen1, float aLen2)
    {
        float c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
        return Mathf.Acos(c);
    }

    private static Quaternion FromToRotation(Vector3 from, Vector3 to)
    {
        float theta = Vector3.Dot(from.normalized, to.normalized);
        if (theta >= 1f)
            return Quaternion.identity;

        if (theta <= -1f)
        {
            Vector3 axis = Vector3.Cross(from, Vector3.right);
            if (axis.sqrMagnitude == 0f)
                axis = Vector3.Cross(from, Vector3.up);

            return Quaternion.AngleAxis(180f, axis);
        }

        return Quaternion.AngleAxis(Mathf.Acos(theta) * Mathf.Rad2Deg, Vector3.Cross(from, to).normalized);
    }

    private static Quaternion NormalizeSafe(Quaternion q)
    {
        float dot = Quaternion.Dot(q, q);
        if (dot > FloatMin)
        {
            float rsqrt = 1.0f / Mathf.Sqrt(dot);
            return new Quaternion(q.x * rsqrt, q.y * rsqrt, q.z * rsqrt, q.w * rsqrt);
        }

        return Quaternion.identity;
    }
}