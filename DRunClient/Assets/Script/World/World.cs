using Festa.Client;
using Festa.Client.Module;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    // 흠... 싱글톤 비헤이비어 있었던것 같은데..
    public static World Instance { get; private set; }

    [SerializeField]
    private SkyBox _skyBox = null;

    // 노을일 때 보여줄 임시 파티클.
    [SerializeField]
    private ParticleSystem _birdParticle = null;

    // 비내릴 때 보여줄 임시 파티클
    [SerializeField]
    private ParticleSystem _rainParticle = null;
    // 비내릴 때 보여줄 임시 파티클
    [SerializeField]
    private ParticleSystem _rainGroundParticle = null;

    [SerializeField]
    private GameObject go_rainbow = null;

    // 비내릴 때 보여줄 임시 파티클
    [SerializeField]
    private ParticleSystem _snowParticle = null;
    
    //[SerializeField]
    //private DesertFox.DesertFox _desertFox = null;

    [SerializeField]
    private float ChangeTime = 30.0f;

    // 트리가 배치될 root transform
    [SerializeField]
    private Transform _treeRoot = null;

    [SerializeField]
    private bool _enableWeather = true;

    [SerializeField]
    private TMP_Text txt_debug;

    [SerializeField]
    private Button btn_harvest;

    [SerializeField]
    private Button btn_withered;

    public RectTransform btn_root;

	// 현재 생성되어있는 tree.
	private CommonTree _currentTree = null;

    private AbstractInputModule _inputModule = null;
    private float _waitTime = 0.0f;

    private int _currentTime = SkyBox.TimeType.day;

    private IntervalTimer _treePresentaionEventTimer;
    private List<TreePresentationEvent> _treePresentationEventList;
    private WorldInputFSM _inputFSM;

    public int getCurrentTime() => _currentTime;

    public bool isRainning
    {
        get; private set;
    }
    public bool isSnowing
    {
        get; private set;
    }

    public AbstractInputModule getInputModule()
    {
        return _inputModule;
    }

    public CommonTree getCurrentTree()
    {
        return _currentTree;
    }

    //public DesertFox.DesertFox getDesertFox()
    //{
    //    return _desertFox;
    //}

    private void Awake()
    {
        // 딱히 인스턴스 개수 제한을 두진 말자.
        Instance = this;

        //Application.targetFrameRate = 60;
    }

    private void Start()
    {
        //_desertFox.setWorld(this);

        _treePresentaionEventTimer = IntervalTimer.create(0.1f, true, true);
        _treePresentationEventList = new List<TreePresentationEvent>();

#if UNITY_EDITOR
        _inputModule = InputModule_PC.create();
#else
        _inputModule = InputModule_Mobile.create();
#endif

        _skyBox.SetTime(SkyBox.TimeType.day, false);
        _waitTime = Time.time + ChangeTime;
        _inputFSM = WorldInputFSM.create(this);
    }

    private void OnEnable()
    {
        _skyBox.SetTime(_currentTime, isRainning || isSnowing);
    }

    private void Update()
    {   
        if ( _currentTree != null && CheatVariables.tree_showDebug.BoolValue )
        {
            var vm = ClientMain.instance?.getViewModel()?.Tree;
            if ( vm != null )
            {
                var tree = vm.getCurrentTree();
                var refTree = GlobalRefDataContainer.getInstance().get<RefTree>(tree.tree_id);
                if ( tree != null )
                {
                    txt_debug.gameObject.SetActive(true);
                    txt_debug.text = $"step/min = {vm.TreeConfig.step_count}/{refTree.available_stepcount_min} = {(float)vm.TreeConfig.step_count/refTree.available_stepcount_min}\n" + 
                        $"step-min/max-min = {(vm.TreeConfig.step_count- refTree.available_stepcount_min)}/{(refTree.available_stepcount_max - refTree.available_stepcount_min)} = " +
                        $"{(vm.TreeConfig.step_count - refTree.available_stepcount_min)/(float)(refTree.available_stepcount_max - refTree.available_stepcount_min)}";
                }
            }
        }
        else
        {
            txt_debug.gameObject.SetActive(false);
        }

        if ( Time.time > _waitTime )
        {
            _waitTime = Time.time + ChangeTime;

            var testWeather = UnityEngine.Random.Range(0, 3);
            var prevRainning = isRainning;

            if (_enableWeather == false)
                testWeather = 0;

            isRainning = testWeather == 1;
            isSnowing = testWeather == 2;

            go_rainbow.SetActive(false);

            if (_currentTime == SkyBox.TimeType.day )
            {
                _currentTime = SkyBox.TimeType.sunset;
                
            }
            else if (_currentTime == SkyBox.TimeType.sunset)
            {
                _currentTime = SkyBox.TimeType.night;
            }
            else if (_currentTime == SkyBox.TimeType.night)
            {
                _currentTime = SkyBox.TimeType.day;
            }

            if ((_currentTime == SkyBox.TimeType.day || _currentTime == SkyBox.TimeType.sunset)
                    && prevRainning
                    && (isRainning == false && isSnowing == false))
            {
                // 이전에 비가 내렷고, 지금은 그쳤다면?
                go_rainbow.SetActive(true);
            }

            _birdParticle.gameObject.SetActive(_currentTime == SkyBox.TimeType.sunset);
            if ( _birdParticle.gameObject.activeSelf)
            {
                _birdParticle.Play();
            }

            _snowParticle.gameObject.SetActive(isSnowing);
            _rainParticle.gameObject.SetActive(isRainning);
            _rainGroundParticle.gameObject.SetActive(isRainning);

            _skyBox.SetTime(_currentTime, isRainning || isSnowing);
        }

        //if (_inputModule.isTouchDown())
        //{
        //    // 터치된 ui 요소가 없는 경우
        //    // 예시처럼 만들어 놓았으니까 터치,, 괜찮을 것 같긴 한데 아무튼 빌드할 때 다시 확인!!+
        //    if(!EventSystem.current.IsPointerOverGameObject())
        //    {
        //        // 오브젝트 터치 확인
        //        RaycastHit hit;
        //        Ray ray = Camera.main.ScreenPointToRay(_inputModule.getTouchPosition());

        //        if (Physics.Raycast(ray, out hit) == false)
        //        {
        //            return;
        //        }

        //        // 터치된 오브젝트가 있는 경우
        //        if (hit.transform.gameObject == _desertFox.gameObject)
        //        {
        //            _desertFox.touch();
        //        }

        //        if (_currentTree != null && hit.transform.gameObject == _currentTree.gameObject)
        //        {
        //            _currentTree.touch();
        //            _desertFox.onTreeTouch();
        //        }
        //    }
        //}

        // 나무 업데이트
        if (_treePresentaionEventTimer.update() )
        {
            _treePresentationEventList.Clear();
            var vm = ClientMain.instance?.getViewModel()?.Tree;
            vm?.popPresentationEvents(_treePresentationEventList);

            // 한방에 다 쳐리해도 될 것 같은데..?
            foreach (TreePresentationEvent e in _treePresentationEventList)
            {
                if (e.getType() == TreePresentationEvent.EventType.init_tree)
                {
                    // 나무 생성
                    createTree(e);

                    // 초기 init시에 시든 상태일 경우 팝업 노출!

                    if (e.isWithered())
                        UITreeWitheredConfirm.spawn(e.getRefTree());
                }
                else if (e.getType() == TreePresentationEvent.EventType.change_tree)
                {
                    // 이미 있는 나무 지우고 생성.                     
                    createTree(e);
                }
                else if (e.getType() == TreePresentationEvent.EventType.grow_up)
                {
                    if ( e.isSeed() == false )
                    {
                        _currentTree.grow(e);
                    }

                    _currentTree.setHarvest(vm.calcHarvestableCoinAmount_Current() > 0);

                    _currentTree.setWither(e.isWithered());
                }
                else if (e.getType() == TreePresentationEvent.EventType.harvest)
                {
                    _currentTree.grow(e);
                }
                else if (e.getType() == TreePresentationEvent.EventType.wither)
                {
                    // 시들었네~
                    _currentTree.setWither(true);
                }

                Debug.LogWarning($"runTreePresentation:type[{e.getType()}]");
            }
        }

        _currentTree?.update();
        _inputFSM.run();
    }

    private void createTree(TreePresentationEvent presentationEvent)
    {
        if ( _currentTree != null )
        {
            Destroy(_currentTree.gameObject);
        }

        btn_harvest.gameObject.SetActive(false);
        btn_withered.gameObject.SetActive(false);

        var treePrefab = Resources.Load<GameObject>(presentationEvent.getRefTree().resource);
        _currentTree = Instantiate(treePrefab, _treeRoot).GetComponent<CommonTree>();
        _currentTree.bindButtons(btn_harvest, btn_withered);
        _currentTree.treeID = presentationEvent.getRefTree().id;
        var vm = ClientMain.instance?.getViewModel().Tree;

        if (presentationEvent.isWithered())
        {
            // 일단 아무것도 안해,
            _currentTree.setGrow(1);
        }
        else if (presentationEvent.isSeed())
        {
            _currentTree.setGrow(0);
        }
        else
        {
            _currentTree.grow(presentationEvent);
        }

        _currentTree.setWither(presentationEvent.isWithered());
        _currentTree.setHarvest(vm.calcHarvestableCoinAmount_Current() > 0);
       // _desertFox.setRubTarget(_currentTree.transform);
    }
}