using DG.Tweening;
using Festa.Client;
using Festa.Client.Module.Net;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


// 나무들의 단계별 성장을 제어하자.

public class CommonTree : MonoBehaviour
{
    [SerializeField]
    private Animator _seedTreeAnimator;

    [SerializeField]
    private Animator _normalTreeAnimator;

    [SerializeField]
    private SkinnedMeshRenderer _treeMeshRenderer; // 머테리얼을 꺼내야한다.
    private Material _treeMeshMaterial = null;

    [NonSerialized]
    public Button btn_harvest;

	[NonSerialized]
	public Button btn_withered;

    [SerializeField]
    private Transform tf_coinSpawn;

    [SerializeField]
    private ParticleSystem ps_touchParticle;

    //[SerializeField]
    //private SpriteRenderer _rainbowRenderer;

    [SerializeField]
    private Color _witherColor = new Color(0.26f, 0.26f, 0.26f);

    [SerializeField]
    private float EffectTime = 2.0f;

    [SerializeField]
    private Transform pivot_btn_harvest;

    [SerializeField]
    private Transform pivot_btn_withered;

	private bool _isVisibleSnow = true;
    private bool _isVisibleRain = true;

    private List<SkinnedMeshRenderer> _rainNodes = new List<SkinnedMeshRenderer>();
    private List<SkinnedMeshRenderer> _snowNodes = new List<SkinnedMeshRenderer>();

    private bool _growing = false; // 성장 연출중인가?

    private int _currentGrowIndex = -1;
    private int? _reserveGrowIndex = null; // 연출 중 성장 했을 경우.. 예약 해놓기 위한 인덱스.

    private Vector3 _originalScaleNormalTree;

    public int treeID = 0;

    public void Awake()
    {
        var callback = _seedTreeAnimator.GetComponent<AnimationEventCallback>();
        if (callback != null)
        {
            callback.AnimationFinishIntCallback = onFinishGrowAnimationInt;
        }

        callback = _normalTreeAnimator.GetComponent<AnimationEventCallback>();
        if (callback != null)
        {
            callback.AnimationFinishIntCallback = onFinishGrowAnimationInt;
            callback.AnimationFinishStringCallback = onFinishGrowAnimationString;
        }

        //img_harvestImage.gameObject.SetActive(false);
        //img_witheredImage.gameObject.SetActive(false);

        if (_treeMeshRenderer != null)
            _treeMeshMaterial = _treeMeshRenderer.material;

        // 나무 구조가... 노드로 하나씩 쪼개져 있어서.. 
        // snow 하나 rain하나만 있으면 좋을 것 같은데,
        // 불편하군
        recursiveFindNode("rain", _normalTreeAnimator.transform, ref _rainNodes);
        recursiveFindNode("snow", _normalTreeAnimator.transform, ref _snowNodes);
        
        visibleSnow(false);
        visibleRain(false);

        _originalScaleNormalTree = _normalTreeAnimator.transform.localScale;
    }

    private void OnEnable()
    {
        if ( _growing )
        {
            int temp = _currentGrowIndex;

            // 애니메이터가 초기화된다. 연출 중에 멈췄다면 다시 실행해주자..
            _growing = false;
            _currentGrowIndex = -1;
            setGrow(temp);
        }
    }

    // 모든 자식들을 순환하여 찾아내보자.
    private void recursiveFindNode(string name, Transform parent, ref List<SkinnedMeshRenderer> node)
    {
        foreach (Transform child in parent)
        {
            if ( child.name.Contains(name) )
            {
                var renderer = child.GetComponent<SkinnedMeshRenderer>();
                node.Add(renderer);
            }

            recursiveFindNode(name, child, ref node);
        }
    }

    public void visibleRain(bool visible, bool immediate = true)
    {
        if (_isVisibleRain == visible)
        {
            return;
        }

        _isVisibleRain = visible;

        UnityAction func = () =>
        {
            foreach (SkinnedMeshRenderer node in _rainNodes)
            {
                node.gameObject.SetActive(visible);
            }
        };
        if ( immediate )
        {
            func();
        }
        else
        {
            if ( visible )
            {
                func();

                foreach (SkinnedMeshRenderer renderer in _rainNodes)
                {
                    renderer.material.SetFloat("_Alpha", 0.0f);
                    renderer.material.DOFloat(visible ? 1.0f : 0.0f, "_Alpha", EffectTime);
                }

                //_rainbowRenderer.material.SetColor("_BaseColor", new Color(1.0f, 1.0f, 1.0f, 0.0f));
                //_rainbowRenderer.material.DOColor(new Color(1.0f, 1.0f, 1.0f, 150.0f / 255.0f), "_BaseColor", EffectTime);
            }
            else
            {
                foreach (SkinnedMeshRenderer renderer in _rainNodes)
                {
                    renderer.material.DOFloat(visible ? 1.0f : 0.0f, "_Alpha", EffectTime);
                }
                
                //_rainbowRenderer.material.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), "_BaseColor", EffectTime);

                StartCoroutine(WaitCoroutine(EffectTime, func));
            }
        }
    }

    public void visibleSnow(bool visible, bool immediate = true)
    {
        if (_isVisibleSnow == visible)
        {
            return;
        }

        _isVisibleSnow = visible;

        UnityAction func = () =>
        {
            foreach (SkinnedMeshRenderer node in _snowNodes)
            {
                node.gameObject.SetActive(visible);
            }
        };

        if (immediate)
        {
            func();
        }
        else
        {
            if (visible)
            {
                func();
                foreach (SkinnedMeshRenderer renderer in _snowNodes)
                {
                    renderer.material.SetFloat("_Alpha", 0.0f);
                    renderer.material.DOFloat(visible ? 1.0f : 0.0f, "_Alpha", EffectTime);
                }
            }
            else
            {
                foreach (SkinnedMeshRenderer renderer in _snowNodes)
                {
                    renderer.material.DOFloat(visible ? 1.0f : 0.0f, "_Alpha", EffectTime);
                }

                StartCoroutine(WaitCoroutine(EffectTime, func));
            }
        }
    }

    public void update()
    {
        visibleRain(World.Instance.isRainning, false );
        visibleSnow(World.Instance.isSnowing, false);
    }

	public void bindButtons(Button _btn_harvest,Button _btn_withered)
    {
        btn_harvest = _btn_harvest;
        btn_withered = _btn_withered;

        btn_harvest.onClick.RemoveAllListeners();
        btn_harvest.onClick.AddListener(onClickHarvest);

        btn_withered.onClick.RemoveAllListeners();
        btn_withered.onClick.AddListener(onClickWithered);
    }

	// 2022.08.18 이강희
	private void LateUpdate()
    {
        Vector2 wHarvest = Camera.main.WorldToScreenPoint(pivot_btn_harvest.position);
        Vector2 wWithered = Camera.main.WorldToScreenPoint(pivot_btn_withered.position);

        Vector2 localHarvest;
        Vector2 localWithered;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(World.Instance.btn_root, wHarvest, Camera.main, out localHarvest);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(World.Instance.btn_root, wWithered, Camera.main, out localWithered);

        RectTransform rtHarvest = btn_harvest.transform.parent as RectTransform;
		RectTransform rtWithered = btn_withered.transform.parent as RectTransform;

        rtHarvest.anchoredPosition = localHarvest;
        rtWithered.anchoredPosition = localWithered;

		//Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint(button_root_target.position);
		//targetScreenPosition.z = 3;
		//button_canvas.position = Camera.main.ScreenToWorldPoint(targetScreenPosition);
	}

    public void setHarvest(bool ret)
    {
        btn_harvest?.gameObject.SetActive(ret);
    }

    public void setWither(bool enable)
    {
        btn_withered?.gameObject.SetActive(enable);

        if (enable)
        {
            // 열매를 숨겨야한다.

            _treeMeshMaterial?.SetColor("_Color", _witherColor);
        }
        else
        {
            _treeMeshMaterial?.SetColor("_Color", Color.white);
        }
    }

    private bool isHarvestable()
    {
        if( btn_harvest == null)
        {
            return false;
        }

        return btn_harvest.gameObject.activeSelf;
    }

    private bool isWithered()
    {
        if( btn_withered == null)
        {
            return false;
        }

        return btn_withered.gameObject.activeSelf;
    }

    public void grow(TreePresentationEvent e)
    {
        int targetIndex = 1;

        float flower = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Tree.status_flower, 50) * 0.01f;
        float bloom = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Tree.status_fruit, 100) * 0.01f;
        float fruit = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Tree.status_bloom, 100) * 0.01f;

        float ratio = e.getGrowRatio();
        if (ratio < -flower)
        {
            // Normal, 1
            targetIndex = 1;
        }
        else if (ratio < -1.0f + bloom)
        {
            targetIndex = 2;
        }
        else if (ratio < fruit)
        {
            // flower blossom, 3
            targetIndex = 3;
        }
        else
        {
            // fruit, 4
            targetIndex = 4;
        }

        Debug.Log($"[Tree] grow : {ratio} - {targetIndex}");
        setGrow(targetIndex);
    }

    public void setGrow(int index)
    {
        if (_growing)
        {
            _reserveGrowIndex = index;
            return;
        }

        if ( _currentGrowIndex == index )
        {
            Debug.Log($"[Tree] setGrow, already {index}");
            return;
        }

        Debug.Log($"[Tree] setGrow : {index}");
        _currentGrowIndex = index;

        _seedTreeAnimator.gameObject.SetActive(index == 0);

        // 얘는 active를 끄지말고, 숨겨 버리자.
        if ( index == 0 )
        {
            _normalTreeAnimator.transform.localScale = Vector3.zero;
        }

        _growing = true;

        if (index == 0)
        {
            // seed인데?
            _seedTreeAnimator.SetTrigger("grow");
        }
        else
        {
            if ( index == 1)
            {
                _normalTreeAnimator.SetTrigger($"hideFlower");

                // 넌 연출을 기다릴 필요가 읍지~
                _growing = false;
            }
            else if (index == 2)
            {
                _normalTreeAnimator.SetTrigger($"flowerGrow");
            }
            else if (index == 3)
            {
                _normalTreeAnimator.SetTrigger($"flowerBloomGrow");
            }
            else if (index == 4)
            {
                _normalTreeAnimator.SetTrigger($"fruitGrow");
            }
        }

    }

    IEnumerator WaitCoroutine(float time, UnityAction doneCallback)
    {
        yield return new WaitForSeconds(time);
        doneCallback?.Invoke();
    }


    // 각단계별 Grow anim이 끝났을 경우 호출되는 콜백
    // index는 성장 단계
    public void onFinishGrowAnimationInt(int index)
    {
        Debug.Log($"[Tree] onFinishGrowAnimationInt : {index}");
        if ( index == 0 )
        {
            //seed grow가 끝났다.
            _seedTreeAnimator.gameObject.SetActive(false);
            _normalTreeAnimator.SetTrigger("normalGrow");
            // 애님 이벤트와 업데이트 순서에 의하여 튄다. 빠르게 스케일을 늘리는 방식으로 가자.
            _normalTreeAnimator.transform.DOScale(_originalScaleNormalTree, 0.01f);
            return;
        }
        _growing = false;


        if ( _reserveGrowIndex.HasValue)
        {
            setGrow(_reserveGrowIndex.Value);
            _reserveGrowIndex = null;
        }
    }

    public void onFinishGrowAnimationString(string name)
    {
        if ( name == "touch")
        {
            if (harvestTouch)
            {
                harvestTouch = false;

                requestHarvest();
            }
        }
    }

    // 수확 시 터치 애님이 재생되고, 끝났을 때 수확연출을 요청하는데.. 
    // 수확 시 애님이 없어서 터치 애님을 재생하고 있음.
    // 단순 터치시 수확 연출을 요청하지 않기 위한 임시 변수
    private bool harvestTouch = false;

    public void touch()
    {
        // 버튼의 active 여부에 따라 액션을 다르게 한다~!
        // 수확할 코인이 있는지를 기준으로 삼을 경우 연속 터치 시 코인이 무한 수급되는 것처럼 보임(아닌데!!)

        if (isHarvestable())
        {
            // 수확할 수 있는 경우
            onClickHarvest();
        }
        else if (isWithered())
        {
            // 시든 경우
            onClickWithered();
        }
        else
        {
            // 평범한 터치
            _normalTreeAnimator.SetTrigger("touch");
        }

        // 파티클을 계속 평범하게 표시하는 코드래~~
        if (ps_touchParticle != null && ps_touchParticle.isPlaying == false)
            ps_touchParticle.Play();
    }

    public void onClickHarvest()
    {
        harvestTouch = true;
        _normalTreeAnimator.SetTrigger("touch");

        Vector3 position = tf_coinSpawn != null ? tf_coinSpawn.position : transform.position;
        EffectCoin.spawn(position, UIMain.getInstance().getCoinTransform());

        btn_harvest?.gameObject.SetActive(false);
    }

    public void onClickWithered()
    {
        // 시들었을 때 버튼을 클릭하면 상점 오픈!
        UIShop.getInstance().open();

        UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(UIMain.getInstance(), UIShop.getInstance());
        stack.addPrev(UIMainTab.getInstance());
    }

    private void requestHarvest()
    {
        // 수확을 해보자.
        // 수확할게 없다
        var vm = ClientMain.instance?.getViewModel()?.Tree;
        if (vm.calcHarvestableCoinAmount_Current() <= 0)
        {
            return;
        }

        var network = ClientMain.instance.getNetwork();
        MapPacket req = network.createReq(CSMessageID.Tree.ClaimTreeRewardReq);
        network.call(req, ack =>
        {
            if (ack.getResult() == Festa.Client.ResultCode.ok)
            {
                ClientMain.instance?.getViewModel().updateFromPacket(ack);
            }
        });
    }
}
