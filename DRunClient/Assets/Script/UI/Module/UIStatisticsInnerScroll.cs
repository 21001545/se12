using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnhancedScrollerDemos.NestedScrollers;

namespace Festa.Client
{
    using UnityEngine;
    using System.Collections;
    using UnityEngine.UI;
    using System;
    using UnityEngine.EventSystems;

    public class UIStatisticsInnerScroll : ScrollRect
    {
        private bool _routeToParent = false;
        private int _graphType;
        private float _startPosX;

        public void setGraphType(int type)
        { _graphType = type; }

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
            }
        }

        /// <summary>
        /// Begin drag event
        /// </summary>
        public override void OnBeginDrag(PointerEventData eventData)
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

        /// <summary>
        /// End drag event
        /// </summary>
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_routeToParent)
                DoForParents<IEndDragHandler>((parent) => { parent.OnEndDrag(eventData); });
            else
            {
                if (_startPosX - eventData.position.x < 0)
                {
                    if (_graphType == UIStatistics.GraphTypes.Weekly)
                        UIStatistics.getInstance().onClickWeekAveragePrev();
                    else if (_graphType == UIStatistics.GraphTypes.Monthly)
                        UIStatistics.getInstance().onClickMonthChartPrev();
                }
                else
                {
                    if (_graphType == UIStatistics.GraphTypes.Weekly)
                        UIStatistics.getInstance().onClickWeekAverageNext();
                    else if (_graphType == UIStatistics.GraphTypes.Monthly)
                        UIStatistics.getInstance().onClickMonthChartNext();
                }

                base.OnEndDrag(eventData);
            }
            _routeToParent = false;
        }
    }
}