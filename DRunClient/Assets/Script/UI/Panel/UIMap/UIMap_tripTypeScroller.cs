using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;

namespace Festa.Client
{
    public class UIMap_tripTypeScroller : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private RectTransform rect_contentBox;
        [SerializeField]
        private UIMap_tripTypeCellView[] cells;

        private bool _isDragging = false;
        private int _tripTypes;
        private int _currentIndex = 0;
        private float _slotWidth = 206f;
        private float _initialScrollPosition = 84f;
        private float _dragStartX;
        private float _draggingThreshold = 20f;
        private float _animationDuration = 0.3f;

        #region drag~~

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isDragging)
                return;

            _dragStartX = eventData.position.x;
            _isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = true;
            int lastIndex = _currentIndex;

            if (_dragStartX - eventData.position.x > _draggingThreshold)
            {
                // 오른쪽 아이템이 보이자
                dragLeft();
            }
            else if (_dragStartX - eventData.position.x < -_draggingThreshold)
            {
                // 왼쪽 아이템이 보이자
                dragRight();
            }
        }

        #endregion

        public int getCurrentTripType()
        {
            return _currentIndex;
        }

        public void init()
        {
            _isDragging = false;
            _tripTypes = rect_contentBox.gameObject.transform.childCount;
            _currentIndex = 0;
            rect_contentBox.anchoredPosition = new Vector2(_initialScrollPosition, rect_contentBox.anchoredPosition.y);
            cells[_currentIndex].pointOnImmediately();

            for (int i = 1; i < cells.Length; ++i)
            {
                cells[i].pointOffImmediately();
            }
        }

        private void dragLeft()
        {
            if (_currentIndex > _tripTypes - 2)
            {
                _currentIndex = _tripTypes - 2;
            }

            cells[_currentIndex].pointOff();
            ++_currentIndex;
            float targetX = _initialScrollPosition - _slotWidth * _currentIndex;

            DOTween.To(() => rect_contentBox.anchoredPosition, x => rect_contentBox.anchoredPosition = x, new Vector2(targetX, rect_contentBox.anchoredPosition.y), _animationDuration);
            highlightCell();

        }

        private void dragRight()
        {
            if (_currentIndex < 1)
            {
                _currentIndex = 1;
            }

            cells[_currentIndex].pointOff();
            --_currentIndex;
            float targetX = _initialScrollPosition - _slotWidth * _currentIndex;

            DOTween.To(() => rect_contentBox.anchoredPosition, x => rect_contentBox.anchoredPosition = x, new Vector2(targetX, rect_contentBox.anchoredPosition.y), _animationDuration);
            highlightCell();
        }

        private void highlightCell()
        {
            cells[_currentIndex].pointOn();
            _isDragging = false;
        }
    }

}