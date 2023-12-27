using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AvatarMeshCustomize : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer _face;

    [SerializeField]
    private Transform _rootBone;

    [SerializeField]
    private Animator _animator;

    Matrix4x4 _initialTransform;
    Dictionary<string, Transform> _bones = new Dictionary<string, Transform>();
    Dictionary<string, Vector3> _originPosition = new Dictionary<string, Vector3>();
    Dictionary<string, Vector3> _originScale = new Dictionary<string, Vector3>();
    Dictionary<string, Quaternion> _originRotation = new Dictionary<string, Quaternion>();

    Dictionary<string, int> _weightIndex = new Dictionary<string, int>();


    GameObject _eyeGlassesMesh = null;
    GameObject _beardMesh = null;
    GameObject _hairMesh = null;
    GameObject _topMesh = null;
    GameObject _shoesMesh = null;
    GameObject _bottomMesh = null;
    GameObject _capMesh = null;
    GameObject _onepieceMesh = null;

    [Header("테스트용 임시 모델")]
    [SerializeField]
    GameObject _eyeGlassesPrefab = null;
    //[SerializeField]
    //GameObject _beardPrefab = null;
    [SerializeField]
    GameObject _hairPrefab = null;

    private bool _dirty = true;

    public struct MinMaxRange
    {
        public Vector3 Min;
        public Vector3 Max;
    };
    // 본 이동이 가능한.. 본들의 범위값.
    public Dictionary<string, MinMaxRange> BoneTranslateValueRange = new Dictionary<string, MinMaxRange>()
    {
        { "iScale_L", new MinMaxRange() { Min = new Vector3(-0.065f, 0.095f, 0.1017452f), Max = new Vector3(-0.045f, 0.12f, 0.1017452f) }},
        { "iScale_R", new MinMaxRange() { Min = new Vector3(0.045f, 0.095f, 0.1017452f), Max = new Vector3(0.065f, 0.12f, 0.1017452f) }},
        { "nose", new MinMaxRange() { Min = new Vector3(0.0f, 0.034f, 0.13f), Max = new Vector3(0.0f, 0.055f, 0.16f) }},
        { "mouth", new MinMaxRange() { Min = new Vector3(0.0f, -0.01f, 0.155f), Max = new Vector3(0.0f, 0.02f, 0.13f) }},
    };

    // 본 스케일이 가능한.. 본들의 범위값.
    public Dictionary<string, MinMaxRange> BoneScaleValueRange = new Dictionary<string, MinMaxRange>()
    {
        { "iScale_L", new MinMaxRange() { Min = new Vector3(0.5f, 0.5f, 0.5f), Max = new Vector3(1.2f, 1.2f, 1.2f) }},
        { "iScale_R", new MinMaxRange() { Min = new Vector3(0.5f, 0.5f, 0.5f), Max = new Vector3(1.2f, 1.2f, 1.2f) }},
        { "nose", new MinMaxRange() { Min = new Vector3(0.3f, 0.3f, 0.3f), Max = new Vector3(1.4f, 1.4f, 1.4f) }},
        { "mouth", new MinMaxRange() { Min = new Vector3(0.55f, 0.55f, 0.55f), Max = new Vector3(1.25f, 1.25f, 1.25f) }},
        { "Head", new MinMaxRange() { Min = new Vector3(0.8f, 0.8f, 0.8f), Max = new Vector3(1.25f, 1.25f, 1.25f) }},
    };

    GameObject RecursiveFindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child.gameObject;
            
            var result = RecursiveFindChild(child, childName);
            if ( result != null )
            {
                return result;
            }
        }

        return null;
    }

    private void Awake()
    {
        _initialTransform = transform.localToWorldMatrix;
        if (_rootBone != null)
        {
            Action<Transform> iterator = null;
            iterator = (Transform transform) =>
            {
                foreach (Transform childTransform in transform)
                {
                    if (childTransform.name.EndsWith("_mesh"))
                        continue;
                    _bones.Add(childTransform.name, childTransform);
                    _originPosition.Add(childTransform.name, childTransform.localPosition);
            _originRotation.Add(childTransform.name, childTransform.localRotation);
                    _originScale.Add(childTransform.name, childTransform.localScale);
                    iterator(childTransform);
                }
            };
            iterator(_rootBone);
            _bones.Add(_rootBone.name, _rootBone);
            _originPosition.Add(_rootBone.name, _rootBone.localPosition);
            _originScale.Add(_rootBone.name, _rootBone.localScale);
            _originRotation.Add(_rootBone.name, _rootBone.localRotation);
        }

        if (_face != null)
        {
            for ( int i = 49; i < _face.sharedMesh.blendShapeCount; ++i )
            {
                var blendShapeName = _face.sharedMesh.GetBlendShapeName(i);
                Debug.Log(blendShapeName);

                _weightIndex.Add(blendShapeName, i);
            }
        }

        var path = Path.Combine(Application.temporaryCachePath, "customize.json");
        loadCustomize(path);

        equipHair(Instantiate(_hairPrefab));
        //equipEyeglasses();
    }

    public void playAnimation()
    {
        if ( _animator != null )
        {
            _animator.enabled = true;
        }
    }

    public void changeAnimation(string animName)
    {
        // 음.. 애니메이션 하드코딩.
        _animator?.SetBool("isIdle", animName == "isIdle");
        _animator?.SetBool("isStanding", animName == "isStanding");
    }

    public void equipMesh(GameObject mesh, string type )
    {
        string prevMeshName = null;
        if ( type == "top")
        {
            if (_topMesh != null)
            {
                prevMeshName = _topMesh.name;
                Destroy(_topMesh);
                _topMesh = null;
            }
            _topMesh = mesh;
        }
        else if (type == "bottom")
        {
            if (_bottomMesh != null)
            {
                prevMeshName = _bottomMesh.name;
                Destroy(_bottomMesh);
                _bottomMesh = null;
            }
            _bottomMesh = mesh;
        }
        else if (type == "cap")
        {
            if (_capMesh != null)
            {
                prevMeshName = _capMesh.name;
                Destroy(_capMesh);
                _capMesh = null;
            }
            _capMesh = mesh;
        }
        else if (type == "onepiece")
        {
            if (_onepieceMesh != null)
            {
                prevMeshName = _onepieceMesh.name;
                Destroy(_onepieceMesh);
                _onepieceMesh = null;
            }
            _onepieceMesh = mesh;
        }
        else if (type == "shoes")
        {
            if (_shoesMesh != null)
            {
                prevMeshName = _shoesMesh.name;
                Destroy(_shoesMesh);
                _shoesMesh = null;
            }
            _shoesMesh = mesh;
        }

        if ( string.IsNullOrEmpty(prevMeshName) == false && prevMeshName == mesh.name)
        {
            // unequip 임시로 이렇게 하자.
            Destroy(mesh);
            return;
        }

        var hip = _bones["Hips"];

        if (hip == null)
        {
            Debug.Log("not found head bone");
            return;
        }

        //GetComponent<Animator>().enabled = false;
        foreach (var bone in _bones)
        {
            Debug.Log($"bone : {bone.Key}");
            bone.Value.localRotation = _originRotation[bone.Key];
            bone.Value.localPosition = _originPosition[bone.Key];
        }
        mesh.transform.parent = hip;
        mesh.transform.localPosition = Vector3.zero;
        mesh.transform.localRotation = Quaternion.identity;

        var skinnedMeshRenderer = mesh.GetComponentInChildren<SkinnedMeshRenderer>();
        var bones = skinnedMeshRenderer.bones;

        List<Transform> newBones = new List<Transform>();
        List<BoneWeight> newBoneWeights = new List<BoneWeight>();
        for (int i = 0; i < bones.Length; ++i)
        {
            Transform bone = null;
            if (_bones.TryGetValue(bones[i].name, out bone) == false)
            {
                newBones.Add(bones[i]);
            }
            else
            {
                newBones.Add(bone);

                bones[i].localPosition = bone.localPosition;
                bones[i].localRotation = bone.localRotation;
                bones[i].localScale = bone.localScale;
            }

        }

        var boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;
        for (int i = 0; i < boneWeights.Length; ++i)
        {
            BoneWeight bw = skinnedMeshRenderer.sharedMesh.boneWeights[i];
            newBoneWeights.Add(bw);
        }

        List<Matrix4x4> bindposes = new List<Matrix4x4>();

        for (int i = 0; i < newBones.Count; ++i)
        {
            //if (_boneTransform.ContainsKey(newBones[i].name))
            //{
            //    bindposes.Add(_boneTransform[newBones[i].name]);
            //}
            //else
            {
                //Debug.Log("없는 본 " + newBones[i].name);
                bindposes.Add(newBones[i].worldToLocalMatrix);
            }
        }
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        CombineInstance ci = new CombineInstance();
        ci.mesh = skinnedMeshRenderer.sharedMesh;
        ci.transform = transform.localToWorldMatrix; ;
        combineInstances.Add(ci);

        skinnedMeshRenderer.sharedMesh = new Mesh();
        skinnedMeshRenderer.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        skinnedMeshRenderer.bones = newBones.ToArray();
        skinnedMeshRenderer.rootBone = hip;
        skinnedMeshRenderer.sharedMesh.bindposes = bindposes.ToArray();
        skinnedMeshRenderer.sharedMesh.boneWeights = newBoneWeights.ToArray();
        skinnedMeshRenderer.sharedMesh.RecalculateBounds();
    }

    public void equipHair(GameObject hairMesh)
    {
        if (_hairMesh != null )
        {
            Destroy(_hairMesh);
            _hairMesh = null;
        }
        _hairMesh = hairMesh;
        var head = _bones["Head"];

        if (head == null )
        {
            Debug.Log("not found head bone");
            return;
        }

        //GetComponent<Animator>().enabled = false;
        foreach (var bone in _bones)
        {
            Debug.Log($"bone : {bone.Key}");
            bone.Value.localRotation = _originRotation[bone.Key];
            bone.Value.localPosition = _originPosition[bone.Key];
        }
        hairMesh.transform.parent = head;
        hairMesh.transform.localPosition = Vector3.zero;
        hairMesh.transform.localRotation = Quaternion.identity;

        var skinnedMeshRenderer = hairMesh.GetComponentInChildren<SkinnedMeshRenderer>();
        var bones = skinnedMeshRenderer.bones;

        List<Transform> newBones = new List<Transform>();
        List<BoneWeight> newBoneWeights = new List<BoneWeight>();
        for ( int i =0; i < bones.Length; ++i)
        {
            Transform bone = null;
            if (_bones.TryGetValue(bones[i].name, out bone) == false)
            {
                newBones.Add(bones[i]);
            }
            else
            {
                newBones.Add(bone);

                bones[i].localPosition = bone.localPosition;
                bones[i].localRotation = bone.localRotation;
                bones[i].localScale = bone.localScale;
            }

        }

        var boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;
        for( int i =0; i < boneWeights.Length;++i)
        {
            BoneWeight bw = skinnedMeshRenderer.sharedMesh.boneWeights[i];
            newBoneWeights.Add(bw);
        }

        List<Matrix4x4> bindposes = new List<Matrix4x4>();

        for (int i = 0; i < newBones.Count; ++i)
        {
            //if (_boneTransform.ContainsKey(newBones[i].name))
            //{
            //    bindposes.Add(_boneTransform[newBones[i].name]);
            //}
            //else
            {
                Debug.Log("없는 본 " + newBones[i].name);
                bindposes.Add(newBones[i].worldToLocalMatrix);
            }
        }
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        CombineInstance ci = new CombineInstance();
        ci.mesh = skinnedMeshRenderer.sharedMesh;
        ci.transform = transform.localToWorldMatrix; ;
        combineInstances.Add(ci);

        skinnedMeshRenderer.sharedMesh = new Mesh();
        skinnedMeshRenderer.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        skinnedMeshRenderer.bones = newBones.ToArray();
        skinnedMeshRenderer.rootBone = head;
        skinnedMeshRenderer.sharedMesh.bindposes = bindposes.ToArray();
        skinnedMeshRenderer.sharedMesh.boneWeights = newBoneWeights.ToArray();
        skinnedMeshRenderer.sharedMesh.RecalculateBounds();

        //var boingBones = hairMesh.GetComponent<BoingKit.BoingBones>();
        //if ( boingBones != null )
        //{
        //    var colliders = transform.GetComponentsInChildren<BoingKit.BoingBoneCollider>();
        //    boingBones.BoingColliders = colliders;
        //}

        //var dynamicBones = hairMesh.GetComponent<DynamicBone>();
        //if (dynamicBones != null)
        //{
        //    var colliders = transform.GetComponentsInChildren<DynamicBoneCollider>();
        //    dynamicBones.m_Colliders.Clear();

        //    for(int i = 0; i < colliders.Length; ++i)
        //    {
        //        dynamicBones.m_Colliders.Add(colliders[i]);
        //    }
        //}

    }

    public void equipEyeglasses()
    {
        _eyeGlassesMesh = Instantiate(_eyeGlassesPrefab, _bones["Head"]);
        setDirty();

    }

    public void Update()
    {
        if ( _dirty )
        {
            _dirty = false;

            if (_eyeGlassesMesh == null && _beardMesh == null)
            {
                return;
            }

            Mesh mesh = null;
            try
            {
                mesh = new Mesh();
                _face.BakeMesh(mesh);
                var vertices = mesh.vertices;

                if (_eyeGlassesMesh != null )
                {
                    _eyeGlassesMesh.transform.position = eyeCenterPosition();

                    var eyeLPosition = vertices[1157] + transform.position;

                    _eyeGlassesMesh.transform.rotation = Quaternion.Euler(0, 0, 0);


                    var glasses_leftEnd = RecursiveFindChild(_eyeGlassesMesh.transform, "glasses_leftEnd");
                    var glasses_leftEar = RecursiveFindChild(_eyeGlassesMesh.transform, "glasses_leftear");

                    var dir = (eyeLPosition - glasses_leftEnd.transform.position).normalized;
                    var dir2 = (glasses_leftEar.transform.position - glasses_leftEnd.transform.position).normalized;

                    _eyeGlassesMesh.transform.rotation = Quaternion.Euler(Vector3.Angle(dir2, dir), 0, 0);
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                if ( mesh != null )
                {
                    Destroy(mesh);
                }
            }
        }
    }

    private void setDirty()
    {
        _dirty = true;
    }
    public int getBlendShapeCount()
    {
        return _face.sharedMesh.blendShapeCount;
    }

    public string getBlendShapeName(int index)
    {
        return _face.sharedMesh.GetBlendShapeName(index);
    }

    public float getBlendShapeWeight(string name)
    {
        int index = -1;
        if (_weightIndex.TryGetValue(name, out index) && index != -1)
        {
            return _face.GetBlendShapeWeight(index);
        }

        return 0.0f;
    }

    public void setBlendShapeWeight(string name, float weight)
    {
        int index = -1;
        if (_weightIndex.TryGetValue(name, out index) && index != -1)
        {
            _face.SetBlendShapeWeight(index, weight);
            setDirty();
        }
    }

    [SerializeField]
    private GameObject[] testCube;
    public void updateBeard()
    {
        if (testCube == null || testCube.Length != 9)
            return;
        if (testCube[0] == null)
            return;

        Mesh mesh = new Mesh();
        _face.BakeMesh(mesh);
        var vertices = mesh.vertices;
        testCube[0].transform.localPosition = vertices[544];
        testCube[1].transform.localPosition = vertices[605];
        testCube[2].transform.localPosition = vertices[581];
        testCube[3].transform.localPosition = vertices[585];
        testCube[4].transform.localPosition = vertices[2900];
        testCube[5].transform.localPosition = vertices[2893];
        testCube[6].transform.localPosition = vertices[2889];
        testCube[7].transform.localPosition = vertices[2914];
        testCube[8].transform.localPosition = vertices[2847];
     
        Destroy(mesh);
    }

    public void setFaceBoneTranslation(string name, Vector3 position)
    {
        if (_bones.ContainsKey(name))
        {
            _bones[name].localPosition = position;
        }
        setDirty();
    }

    public void setFaceBoneWorldTranslation(string name, Vector3 position)
    {
        if (_bones.ContainsKey(name))
        {
            _bones[name].position = position;
        }
        setDirty();
    }

    public void setFaceBoneScale(string name, Vector3 scale)
    {
        if (_bones.ContainsKey(name))
        {
            _bones[name].localScale = scale;
        }
        setDirty();
    }

    public Vector3 getFaceBoneTranslation(string name)
    {
        if (_originPosition.ContainsKey(name))
        {
            return _originPosition[name];
        }

        return Vector3.zero;
    }

    public Vector3 getFaceBoneWorldTranslation(string name)
    {
        if (_bones.ContainsKey(name))
        {
            return _bones[name].position;
        }

        return Vector3.zero;
    }

    public Vector3 getFaceBoneScale(string name)
    {
        if (_originScale.ContainsKey(name))
        {
            return _originScale[name];
        }

        return Vector3.zero;
    }

    public void loadCustomize(string path)
    {
        if (File.Exists(path) == false)
        {
            return;
        }

        var json = Festa.Client.Module.JsonObject.parse(File.ReadAllText(path));
        if (json != null)
        {
            var names = new string[]
                {"Face.CheekIn"
                ,"Face.CheekOut"
                ,"Face.ChinThin"
                ,"Face.ChinThick"
                ,"Face.EarBig"
                ,"Face.EyeInUp"
                ,"Face.EyeInDown"
                ,"Face.EyeOutUp"
                ,"Face.EyeOutDown"
                ,"Face.JawBig"
                ,"Face.JawSmall"
                ,"Face.HeadLength_short"
                ,"Face.HeadLength_long"
                ,"Face.LipsUpperThick"
                ,"Face.LipsLowerThick"
                ,"Face.NoseSharp"
                ,"Face.MouthOut"
                ,"Face.MouthIn"
                };

            //foreach (var name in names)
            //{
            //    setBlendShapeWeight(name, json.getFloat(name));
            //}

            //Vector3 t = new Vector3(json.getFloat("mouthTranslateX"), json.getFloat("mouthTranslateY"), json.getFloat("mouthTranslateZ"));
            //setFaceBoneTranslation("mouth", t);
            //t = new Vector3(json.getFloat("mouthScaleX"), json.getFloat("mouthScaleY"), json.getFloat("mouthScaleZ"));
            //setFaceBoneScale("mouth", t);
            //t = new Vector3(json.getFloat("noseTranslateX"), json.getFloat("noseTranslateY"), json.getFloat("noseTranslateZ"));
            //setFaceBoneTranslation("nose", t);
            //t = new Vector3(json.getFloat("noseScaleX"), json.getFloat("noseScaleY"), json.getFloat("noseScaleZ"));
            //setFaceBoneScale("nose", t);
            //t = new Vector3(json.getFloat("eye"), json.getFloat("eye"), json.getFloat("eye"));
            //setFaceBoneScale("iScale_L", t);
            //setFaceBoneScale("iScale_R", t);
        }
    }

    [SerializeField]
    private float distance = 0.5f;
    public Vector3 eyeCenterPosition()
    {
        var left = getFaceBoneWorldTranslation("iScale_L");
        var right = getFaceBoneWorldTranslation("iScale_R");
        var nose = getFaceBoneWorldTranslation("nose");

        var center = ((left + right) * 0.5f);
        nose = (nose - center);
        var dir = nose.normalized;
        var length = nose.magnitude;

        

        return (center + (dir * length * distance));
    }
}
