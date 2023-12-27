using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

// 몇가지 함수들을 override하기 위한 테스트용 클래스...
public class TMP_InputFieldEx : TMP_InputField
{
    [Header("Deselect 무시할 객체")]
    [SerializeField]
    private GameObject[] go_ignoreSelect;

    public override void OnDeselect(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;

        if (pointerEventData != null)
        {
            foreach (var go in go_ignoreSelect)
            {
                if (go == pointerEventData.pointerCurrentRaycast.gameObject)
                {
                    // EventSystem에서 현재 선택된 오브젝트가 다른 selectable껄로 변경 되었을 거다.
                    // Deselect 이벤트를 강제로 태우지 않음에도, 이벤트가 전달이 안될테니.....
                    // 다음 틱에 다시 선택하도록...
                    StartCoroutine(Reactivate());
                    return;
                }
            }

            base.OnDeselect(eventData);
        }
        else
        {
            base.OnDeselect(eventData);
        }
    }

    IEnumerator Reactivate()
    {
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
