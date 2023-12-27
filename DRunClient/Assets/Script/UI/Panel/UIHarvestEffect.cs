using DG.Tweening;
using Festa.Client.Module.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHarvestEffect : UIPanel
{
    [SerializeField]
    private GameObject _targetObject;

    [SerializeField]
    private int _maxCount = 10;

    [SerializeField]
    private float _spawnTime;

    [SerializeField]
    private float _speed = 0.2f;

    private Vector2 _startPosition;
    private Vector2 _goalPosition;

    public void Start()
    {
        _targetObject.gameObject.SetActive(false);
        StartCoroutine(harvest());
    }


    public static UIHarvestEffect spawn(Vector2 startPosition, Vector2 goalPosition)
    {
        var effect = UIManager.getInstance().spawnInstantPanel<UIHarvestEffect>();

        effect._startPosition = startPosition;
        effect._goalPosition = goalPosition;

        return effect;
    }

    public float _throwingPower = 0.25f;
    public Ease _easeType = Ease.InQuad;

    IEnumerator harvest()
    {
        int count = 0;
        while(count++ < _maxCount)
        {
            var spawn = Instantiate(_targetObject, _targetObject.transform.parent);
            spawn.gameObject.SetActive(true);
            var rect = spawn.GetComponent<RectTransform>();
            rect.position = new Vector3(_startPosition.x, _startPosition.y, rect.position.z);
            

            var start = rect.position;
            var end = new Vector3(_goalPosition.x, _goalPosition.y, rect.position.z);
            var dir = (end - start);

            Vector3 subDir;
            if ( UnityEngine.Random.Range(0, 2) == 0 )
            {
                subDir = Vector3.up;
            }
            else
            {
                subDir = Vector3.left;
            }

            var secondPoint = start + (dir * 0.5f) + (subDir * Random.Range(0, dir.magnitude * _throwingPower));
            Vector3[] points = new Vector3[3];
            points.SetValue(start, 0);
            points.SetValue(secondPoint, 1);
            points.SetValue(end, 2);
            float time = 0;
            var tween = DOTween.To(() => time, x =>
            {
                time = x;
                var v1 = Vector3.Lerp(points[0], points[1], time / _speed);
                var v2 = Vector3.Lerp(points[1], points[2], time / _speed);
                var v3 = Vector3.Lerp(v1, v2, time / _speed);
                rect.position = v3;
            }
            , _speed, _speed).SetEase(_easeType);
            
            rect.DOScale(1.0f, _speed);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(tween);            
            sequence.Append(rect.DOScale(2.0f, 0.1f));
            sequence.Append(rect.DOScale(1.0f, 0.1f));
            yield return new WaitForSeconds(_spawnTime);
        }

        yield return new WaitForSeconds(_speed - _spawnTime);
        close();
    }
}
