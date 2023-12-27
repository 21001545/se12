using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using EnhancedUI;
using EnhancedScrollerDemos.FlickSnap;

namespace Festa.Client
{
    public abstract class snapPicker_work : MonoBehaviour
    {
        public class LoadType
        {
            public static int load = 0;
            public static int donotLoad = 1;
        }

        protected int _loadType = LoadType.load;

        public struct currentPicked
        {
            public int first_val, second_val;
            public int measure_index;

            public currentPicked(int _first, int _second, int _measure)
            {
                first_val = _first;
                second_val = _second;
                measure_index = _measure;
            }
        }

        private snapPickerController[] _snapPickerControllers;  // 피커 컨트롤러를 담은 배열
        private currentPicked   _currentPicked;                 // 현재 선택되어 있는 데이터
        private float           _cellSize = 44f;                // 선택바(연두색 그거) 크기랑 똑같이 맞춰야 편하당

        // 2022.4.15 이강희
        protected ClientViewModel ViewModel => ClientMain.instance.getViewModel();

        public currentPicked getCurrentPicked()
        {
            return _currentPicked;
        }

        // 2022.4.15 이강희 호출 타이밍 이슈로 초기화 함수는 UISettings에서 별도 호출하도록 변경
        public virtual void init()
		{
			_snapPickerControllers = gameObject.GetComponentsInChildren<snapPickerController>();

			// 컨트롤러의 snapping handler 설정
			foreach (var snapPickerController in _snapPickerControllers)
			{
                snapPickerController.init(onSnapped);
				snapPickerController.setCellSize(_cellSize);
			}
		}

        protected void setLoadType(int type)
        {
            _loadType = type;
        }

		public virtual void updateFromData()
		{
            // 데이터 초기 세팅
            _currentPicked = init_pickedData();

            // 스크롤러 세팅
            for (int i = 0; i < 3; i++)
            {
                SmallList<pickerData> dataList = init_scroller(_currentPicked.measure_index, i);
                _snapPickerControllers[i].Reload(dataList);

                //FlickSnap flickSnap = _snapPickerControllers[i].scroller.GetComponent<FlickSnap>();
                //flickSnap.MaxDataElements = dataList.Count - 5;
            }

            // 2022.4.15 최초 설정해서는 Tween 애니메이션을 끔
            _snapPickerControllers[0].scroller.JumpToDataIndex(_currentPicked.first_val, 0, 0, true, EnhancedScroller.TweenType.immediate, 0f);
            _snapPickerControllers[1].scroller.JumpToDataIndex(_currentPicked.second_val, 0, 0, true, EnhancedScroller.TweenType.immediate, 0f);
            _snapPickerControllers[2].scroller.JumpToDataIndex(_currentPicked.measure_index, 0, 0, true, EnhancedScroller.TweenType.immediate, 0f);
        }

        // 저장되어 있던 정보들을 가져와~
        protected abstract currentPicked init_pickedData();

        // 스크롤러 안에서 스크롤 될 숫자들 초기화~
        protected abstract SmallList<pickerData> init_scroller(int _measure, int _col);

        // 단위 바뀔 때 값 바꿔서 계산해 주는 부분
        // 종류에 맞게 잘 바꿔서 쓰자~~
        protected abstract currentPicked measure_value(currentPicked _data, int _measure);

        // 픽 된 데이터를 클래스에 저장~~
        public abstract void onClickCommit();


        #region snap scroller

        private void onSnapped(EnhancedScroller scroller, int dataIndex)        // 스냅된 셀의 인덱스 == 멈췄을 때 최상단 셀의 인덱스
        {
            Debug.Log($"{scroller.gameObject.name}.onSnapped[{dataIndex}]");

            if (scroller == _snapPickerControllers[0].scroller)
                _currentPicked.first_val = dataIndex;

            else if (scroller == _snapPickerControllers[1].scroller)
                _currentPicked.second_val = dataIndex;

            else if (scroller == _snapPickerControllers[2].scroller)
                change_measure(dataIndex);
        }

        private void change_measure(int _measure)
        {
            Debug.Log($"change_measure[{_measure}]");

            if (_currentPicked.measure_index != _measure)
            {
                // 단위에 맞게 선택값 바꿔주고
                _currentPicked = measure_value(_currentPicked, _measure);

                // 스크롤러 숫자들 바꿔주고
                if(_loadType == LoadType.load)
                {
                    for (int i = 0; i < 2; ++i)
                        _snapPickerControllers[i].Reload(init_scroller(_currentPicked.measure_index, i));

                    // 바뀐 단위로 값 돌려준다
                    _snapPickerControllers[0].scroller.JumpToDataIndex(_currentPicked.first_val, 0, 0, true, EnhancedScroller.TweenType.easeOutQuad, 0.2f);
                    _snapPickerControllers[1].scroller.JumpToDataIndex(_currentPicked.second_val, 0, 0, true, EnhancedScroller.TweenType.easeOutQuad, 0.2f);
                }
            }
        }

        #endregion
    }
}