using System.Collections;
using UnityEngine;

namespace Festa.Client
{
    public class setting_userData
    {
        public int language;        // RefLanguage.lang_type

        public int gender;          // 1 : 남, 2 : 여, 3 : 기타 (선택 안하면 0)

        public double height;
        public int height_measure;  // 0 : cm, 1 : ft

        public double weight;
        public int weight_measure;  // 0 : kg, 1 : lb, 2 : st

        public int distance;        // 0 : km, 1 : mil
        public int temperature;     // 0 : cel, 1 : fer
    }
}