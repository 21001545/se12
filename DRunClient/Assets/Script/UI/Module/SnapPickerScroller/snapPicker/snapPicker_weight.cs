using Festa.Client.Module.UI;
using System.Collections;
using UnityEngine;
using EnhancedUI;
using Festa.Client.NetData;

namespace Festa.Client
{
    public class snapPicker_weight : snapPicker_work
    {
        protected override currentPicked init_pickedData()
        {
            double weight = ViewModel.Health.Body.weight;
            int weight_unit_type = ViewModel.Health.Body.weight_unit_type;

            currentPicked picked = new currentPicked();
            if ( weight_unit_type == UnitDefine.WeightType.unknown)
			{
                // 2022.04.28 소현 : 디폴트에서 열 경우 50kg 으로 설정
                picked.first_val = 50;
                picked.second_val = 0;
                picked.measure_index = 0;
			}
            else
            {
                picked.first_val = Mathf.FloorToInt((float)weight);
                picked.second_val = Mathf.CeilToInt((float)(weight % 1.0) * 10f);
                picked.measure_index = weight_unit_type - 1;
            }

            return picked;
        }

        protected override SmallList<pickerData> init_scroller(int _measure, int _col)
        {
            SmallList<pickerData> list = new SmallList<pickerData>();

            // 단위, 열에 따라 다르게 넣는당
            switch (_col)
            {
                case 0:
                    if (_measure == 0)
                    {
                        for (int i = 0; i < 356; ++i)
                            list.Add(new pickerData() { stringData = i.ToString() });
                    }
                    else if (_measure == 1)
                    {
                        for (int i = 0; i < 782; ++i)
                            list.Add(new pickerData() { stringData = i.ToString() });
                    }
                    else if (_measure == 2)
                    {
                        for (int i = 0; i < 56; ++i)
                            list.Add(new pickerData() { stringData = i.ToString() });
                    }
                    break;

                case 1:
                    for (int i = 0; i < 10; ++i)
                        list.Add(new pickerData() { stringData = i.ToString() });

                    break;

                case 2:
                    list.Add(new pickerData() { stringData = "kg" });
                    list.Add(new pickerData() { stringData = "lb" });
                    list.Add(new pickerData() { stringData = "st" });
                    break;
            }

            return list;
        }

        protected override currentPicked measure_value(currentPicked _data, int _measure)
        {
            int[] changed_data = new int[2];

            if (_measure == 0)
            {
                if(_data.measure_index == 1)
                    changed_data = lb_to_kg(_data.first_val, _data.second_val);
                else if(_data.measure_index == 2)
                    changed_data = st_to_kg(_data.first_val, _data.second_val);

                changed_data[0] = (int)Mathf.Clamp(changed_data[0], 0f, 355f);
            }
            else if (_measure == 1)
            {
                if (_data.measure_index == 0)
                    changed_data = kg_to_lb(_data.first_val, _data.second_val);
                else if (_data.measure_index == 2)
                    changed_data = st_to_lb(_data.first_val, _data.second_val);

                changed_data[0] = (int)Mathf.Clamp(changed_data[0], 0f, 781f);
            }
            else if(_measure == 2)
            {
                if (_data.measure_index == 0)
                    changed_data = kg_to_st(_data.first_val, _data.second_val);
                else if (_data.measure_index == 1)
                    changed_data = lb_to_st(_data.first_val, _data.second_val);

                changed_data[0] = (int)Mathf.Clamp(changed_data[0], 0f, 55f);
            }

            _data.first_val = changed_data[0];
            _data.second_val = changed_data[1];
            _data.measure_index = _measure;

            return _data;
        }

        public override void onClickCommit()
        {
            currentPicked newData = getCurrentPicked();
            ClientBody body = ViewModel.Health.Body;
            body.weight = (double)newData.first_val + (double)newData.second_val * 0.1f;
            body.weight_unit_type = newData.measure_index + 1;

            UISettings.getInstance().sendChangeBody(() => {
                UISettings.getInstance().onClickSetWeight(false);
            });
        }


        #region measure conversion

        private int[] kg_to_lb(int _first, int _second)
        {
            int[] lbs = new int[2];

            float pounds = ((float)_first + (float)_second * 0.1f) * 2.205f;

            lbs[0] = (int)Mathf.Floor(pounds);
            lbs[1] = (int)Mathf.Round((pounds - Mathf.Floor(pounds)) * 10f);

            return lbs;
        }

        private int[] kg_to_st(int _first, int _second)
        {
            int[] st = new int[2];

            float stone = ((float)_first + (float)_second * 0.1f) / 6.35f;

            st[0] = (int)Mathf.Floor(stone);
            st[1] = (int)Mathf.Round((stone - Mathf.Floor(stone)) * 10f);

            return st;
        }

        private int[] lb_to_kg(int _first, int _second)
        {
            int[] kgs = new int[2];

            float kilograms = ((float)_first + (float)_second * 0.1f) / 2.205f;

            kgs[0] = (int)Mathf.Floor(kilograms);
            kgs[1] = (int)Mathf.Round((kilograms - Mathf.Floor(kilograms)) * 10f);

            return kgs;
        }

        private int[] lb_to_st(int _first, int _second)
        {
            int[] st = new int[2];

            float stone = ((float)_first + (float)_second * 0.1f) / 14f;

            st[0] = (int)Mathf.Floor(stone);
            st[1] = (int)Mathf.Round((stone - Mathf.Floor(stone)) * 10f);

            return st;
        }

        private int[] st_to_kg(int _first, int _second)
        {
            int[] kgs = new int[2];

            float kilograms = ((float)_first + (float)_second * 0.1f) * 6.35f;

            kgs[0] = (int)Mathf.Floor(kilograms);
            kgs[1] = (int)Mathf.Round((kilograms - Mathf.Floor(kilograms)) * 10f);

            return kgs;
        }

        private int[] st_to_lb(int _first, int _second)
        {
            int[] lb = new int[2];

            float pounds = ((float)_first + (float)_second * 0.1f) * 14f;

            lb[0] = (int)Mathf.Floor(pounds);
            lb[1] = (int)Mathf.Round((pounds - Mathf.Floor(pounds)) * 10f);

            return lb;
        }

        #endregion
    }
}
