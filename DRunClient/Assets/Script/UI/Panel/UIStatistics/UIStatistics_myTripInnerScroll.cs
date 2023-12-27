using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Linq;

namespace Festa.Client
{
    [System.Serializable]
    public class UIStatistics_myTripInnerScroll : ScrollRect
    {
        private bool _routeToParent = false;
        private float _startPosX;
        private int _currentPanelIndex = 0;
        private int _currentDescIndex = 0;      // 마지막만 잡아!!
        private bool _isAnimating = false;
        private float _animatingDuration = 0.5f;
        private UIStatistics_myTripSummaryTicket _removeTarget;

        // 가장 앞에 보이는 애부터 정렬
        private UIStatistics_myTripSummaryTicket[] panels;
        private Vector2[] targetPosition = { Vector2.zero, new Vector2(30f, 0f), new Vector2(56f, 0f) };
        private Vector2[] targetScale = { new Vector2(344f, 258f), new Vector2(320f, 240f), new Vector2(300f, 225f) };
        private Vector2 _removeTargetPosition = new Vector2(-360f, 0f);
        private float[] targetShadeOpacity = { 0.25f, 0.5f, 0.7f };
        private string[] descriptions = new string[4];

        #region drag functions

        /// <summary>
        /// Do action for all parents
        /// </summary>
        private void DoForParents<T>(Action<T> action) where T : IEventSystemHandler
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                foreach (var component in parent.GetComponents<Component>())
                {
                    if (component is T)
                        action((T)(IEventSystemHandler)component);
                }
                parent = parent.parent;
            }
        }

        /// <summary>
        /// Always route initialize potential drag event to parents
        /// </summary>
        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            DoForParents<IInitializePotentialDragHandler>((parent) => { parent.OnInitializePotentialDrag(eventData); });
            base.OnInitializePotentialDrag(eventData);
        }

        /// <summary>
        /// Drag event
        /// </summary>
        public override void OnDrag(PointerEventData eventData)
        {
            if (_routeToParent)
                DoForParents<IDragHandler>((parent) => { parent.OnDrag(eventData); });
            else
            {
                _startPosX = eventData.position.x;
                base.OnDrag(eventData);

                if (panels == null || panels.Length == 0)
                {
                    panels = new UIStatistics_myTripSummaryTicket[3];
                    panels[0] = transform.GetChild(3).GetComponent<UIStatistics_myTripSummaryTicket>();
                    panels[1] = transform.GetChild(2).GetComponent<UIStatistics_myTripSummaryTicket>();
                    panels[2] = transform.GetChild(1).GetComponent<UIStatistics_myTripSummaryTicket>();
                }
            }
        }

        /// <summary>
        /// Begin drag event
        /// </summary>
        public override void OnBeginDrag(PointerEventData eventData)
        {
            if(!_isAnimating)
            {
                if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
                    _routeToParent = true;
                else if (!vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
                    _routeToParent = true;
                else
                    _routeToParent = false;

                if (_routeToParent)
                    DoForParents<IBeginDragHandler>((parent) => { parent.OnBeginDrag(eventData); });
                else
                    base.OnBeginDrag(eventData);
            }
        }

        /// <summary>
        /// End drag event
        /// </summary>
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!_isAnimating)
            {
                if (_routeToParent)
                    DoForParents<IEndDragHandler>((parent) => { parent.OnEndDrag(eventData); });
                else
                {
                    if (_startPosX - eventData.position.x > 0)
                    {
                        dragRightAction();
                    }

                    base.OnEndDrag(eventData);
                }
                _routeToParent = false;
            }
        }

        #endregion

        public void setupData(string[] desc, string month)
        {
            descriptions = desc;
            panels = new UIStatistics_myTripSummaryTicket[3];
            panels[0] = transform.GetChild(2).GetComponent<UIStatistics_myTripSummaryTicket>();
            panels[1] = transform.GetChild(1).GetComponent<UIStatistics_myTripSummaryTicket>();
            panels[2] = transform.GetChild(0).GetComponent<UIStatistics_myTripSummaryTicket>();

            panels[0].txt_month.text = month;
            panels[1].txt_month.text = month;
            panels[2].txt_month.text = month;

            panels[0].txt_desc.text = descriptions[0];
            panels[1].txt_desc.text = descriptions[1];
            panels[2].txt_desc.text = descriptions[2];

            _currentPanelIndex = 0;
            _currentDescIndex = 2;
        }

        private void dragRightAction()
        {
            _isAnimating = true;
            _removeTarget = panels[_currentPanelIndex];
            ++_currentPanelIndex;
            if(_currentPanelIndex == 3)
            {
                _currentPanelIndex = 0;
            }
            UIStatistics_myTripSummaryTicket bringFrontTarget = panels[_currentPanelIndex];
            UIStatistics_myTripSummaryTicket currentlyLastTarget = panels[_currentPanelIndex + 1 == 3 ? 0 : _currentPanelIndex + 1];

            // 기존 맨 앞
            DOTween.To(() => _removeTarget.rect_panel.anchoredPosition, x => _removeTarget.rect_panel.anchoredPosition = x, _removeTargetPosition, _animatingDuration * 0.5f);

            // 기존 중간
            DOTween.To(() => bringFrontTarget.rect_panel.anchoredPosition, x => bringFrontTarget.rect_panel.anchoredPosition = x, targetPosition[0], _animatingDuration);
            DOTween.To(() => bringFrontTarget.rect_panel.sizeDelta, x => bringFrontTarget.rect_panel.sizeDelta = x, targetScale[0], _animatingDuration);

            Color targetColor = ColorChart.gray_900;
            targetColor.a = targetShadeOpacity[0];
            DOTween.To(() => bringFrontTarget.img_shade.color, x => bringFrontTarget.img_shade.color = x, targetColor, _animatingDuration);


            // 기존 마지막
            DOTween.To(() => currentlyLastTarget.rect_panel.anchoredPosition, x => currentlyLastTarget.rect_panel.anchoredPosition = x, targetPosition[1], _animatingDuration);
            DOTween.To(() => currentlyLastTarget.rect_panel.sizeDelta, x => currentlyLastTarget.rect_panel.sizeDelta = x, targetScale[1], _animatingDuration);
            targetColor.a = targetShadeOpacity[1];
            DOTween.To(() => currentlyLastTarget.img_shade.color, x => currentlyLastTarget.img_shade.color = x, targetColor, _animatingDuration);

            Invoke("setLastPanel", _animatingDuration * 0.5f + 0.1f);
        }

        private void setLastPanel()
        {
            _removeTarget.rect_panel.gameObject.SetActive(false);
            _removeTarget.rect_panel.transform.SetAsFirstSibling();
            _removeTarget.rect_panel.anchoredPosition = targetPosition[2];
            _removeTarget.rect_panel.sizeDelta = targetScale[2];
            _removeTarget.rect_panel.gameObject.SetActive(true);

            ++_currentDescIndex;
            if (_currentDescIndex == 4)
            {
                _currentDescIndex = 0;
            }
            _removeTarget.txt_desc.text = descriptions[_currentDescIndex];
            Color targetColor = ColorChart.gray_900;
            targetColor.a = targetShadeOpacity[2];
            _removeTarget.img_shade.color = targetColor;

            _isAnimating = false;
        }

    }
}