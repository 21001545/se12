using Festa.Client.Module.UI;
using System.Collections;
using UnityEngine;
using EnhancedUI;
using Festa.Client.NetData;

namespace Festa.Client
{
    public class snapPicker_height : snapPicker_work
    {
        protected override currentPicked init_pickedData()
        {
            double height = ViewModel.Health.Body.height;
            int height_unit_type = ViewModel.Health.Body.height_unit_type;

            currentPicked picked = new currentPicked();
            if ( height_unit_type == UnitDefine.DistanceType.unknown)
			{
                // 2022.04.28 소현 : 디폴트에서 열 경우 100cm 로 설정
                picked.first_val = 0;
                picked.second_val = 100;
                picked.measure_index = 0;
                //height_unit_type = UnitDefine.DistanceType.cm;
			}

            else if( height_unit_type == UnitDefine.DistanceType.cm)
			{
                picked.first_val = 0;
                picked.second_val = Mathf.CeilToInt((float)height);
                picked.measure_index = 0;
			}
            else if( height_unit_type == UnitDefine.DistanceType.ft)
			{
                picked.first_val = Mathf.FloorToInt((float)height);
                picked.second_val = Mathf.CeilToInt((float)(height % 1.0) * 100);
                picked.measure_index = 1;
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
                    if (_measure == 1)
                    {
                        for (int i = 0; i < 9; ++i)
                            list.Add(new pickerData() { stringData = i.ToString() + "'" });
                    }

                    // cm 인 경우 첫 번째 열은 빈 상태로 둔다
                    break;

                case 1:
                    if (_measure == 0)
                    {
                        for (int i = 0; i < 273; ++i)
                            list.Add(new pickerData() { stringData = i.ToString() });
                    }
                    else if (_measure == 1)
                    {
                        for (int i = 0; i < 12; ++i)
                            list.Add(new pickerData() { stringData = i.ToString() + "\"" });
                    }
                    break;

                case 2:
                    list.Add(new pickerData() { stringData = "cm" });
                    list.Add(new pickerData() { stringData = "ft" });
                    break;
            }

            return list;
        }

        protected override currentPicked measure_value(currentPicked _data, int _measure)
        {
            if(_measure == 0)
            {
                _data.second_val = (int)Mathf.Clamp(ft_to_cm(_data.first_val, _data.second_val), 0f, 272f);
                _data.first_val = 0;
            }
            else if(_measure == 1)
            {
                int[] feet = cm_to_ft(_data.second_val);

                _data.first_val = (int)Mathf.Clamp(feet[0], 0f, 8f);
                _data.second_val = (int)Mathf.Clamp(feet[1], 0f, 11f);
            }

            _data.measure_index = _measure;

            return _data;
        }

        public override void onClickCommit()
        {
            currentPicked newData = getCurrentPicked();
            ClientBody body = ViewModel.Health.Body;

            if( newData.measure_index == 0)
			{
                body.height = (double)newData.second_val;
                body.height_unit_type = UnitDefine.DistanceType.cm;
            }
            else if( newData.measure_index == 1)
			{
                body.height = (double)newData.first_val + (double)newData.second_val * 0.01f;
                body.height_unit_type = UnitDefine.DistanceType.ft;
			}

            UISettings.getInstance().sendChangeBody(() => {
                UISettings.getInstance().onClickSetHeight(false);
            });
        }


        #region measure conversion

        private int ft_to_cm(int _first, int _second)
        {
            return (int)Mathf.Round((float)(_first + _second / 12f) * 30.48f);
        }

        private int[] cm_to_ft(int _second)
        {
            int[] ft = new int[2];

            float feet = (float)_second / 30.48f;

            ft[0] = (int)Mathf.Floor(feet);
            ft[1] = (int)Mathf.Round((feet - Mathf.Floor(feet)) * 12f);

            return ft;
        }

        #endregion
    }
}
