using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Festa.Client;

public class EffectCoin : MonoBehaviour
{
    [SerializeField]
    private float _throwTime = 1.0f;

    private RectTransform _targetTransform;
    public static void spawn(Vector3 spawnPosition, RectTransform targetTransform)
    {
        var go = Instantiate(Resources.Load<GameObject>("Effect/coin"), spawnPosition, Quaternion.identity);
        
        var effects = go.GetComponentsInChildren<EffectCoin>();
        foreach( var effect in effects )
        {
            effect._targetTransform = targetTransform;
        }

        // 아 이거 낱개로 바뀌면서.. 삭제하기가 귀찮아졋네..
        go.AddComponent<EffectCoin>().startCloseCoroutine();
    }

    private void startCloseCoroutine()
    {
        StartCoroutine(close());
    }

    private IEnumerator close()
    {
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject);
    }

    [SerializeField]
    public Ease _easeType = Ease.InQuad;
    [SerializeField]
    public float _popScale = 2.0f;
    [SerializeField]
    public float _popDuration = 0.1f;
    [SerializeField]
    public float _rotationTime = 0.5f;
    [SerializeField]
    public float _throwScale = 0.8f;
    public void onAnimationFinished()
    {
        GetComponent<Animator>().enabled = false;
        // 페이즈1 끝났네, 날리자.
        var coins = GetComponentsInChildren<MeshRenderer>();

        if (_targetTransform == null )
        {
            // 테스트용..
            _targetTransform = UIMain.getInstance().getCoinTransform();
        }

        foreach ( var coin in coins)
        {
            var sequence = DOTween.Sequence();
            var speed = _throwTime;
            sequence.Append(coin.transform.DOMove(_targetTransform.transform.position, speed).SetEase(_easeType));
            //sequence.Append(coin.transform.DOScale(_throwScale * _popScale, _popDuration));
            //sequence.Append(coin.transform.DOScale(_throwScale, _popDuration));
            //sequence.Append(coin.transform.DOScale(0.0f, 0.01f));
            coin.transform.DOScale(_throwScale, speed).onStepComplete = () => {
                UIMain.getInstance().popCoinEffect();
            };
            coin.transform.DORotate(Vector3.zero, _rotationTime);
        }
    }
}
