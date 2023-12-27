#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace DRun.Client
{
    public class UIControlCenterEditor
    {
        [MenuItem("Drun/Damper/Set all gauge damp time to 1.5")]
        public static void SetGaugeDampTimeTo1dot5()
        {
            float dampTime = 1.5f;

            var all = Object.FindObjectsOfType<UICircleLineFillAnimator>();
            foreach (var fillAnimator in all)
            {
                fillAnimator.FallbackInterpolationTimeInSeconds = dampTime;
                EditorUtility.SetDirty(fillAnimator.gameObject);
            }

            var all2 = Object.FindObjectsOfType<UICircleLinePointMover>();
            foreach (var mover in all2)
            {
                mover.FallbackInterpolationTimeInSeconds = dampTime;
                EditorUtility.SetDirty(mover.gameObject);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        [MenuItem("Drun/Damper/Set all gauge damp time to 1")]
        public static void SetGaugeDampTimeTo1()
        {
            float dampTime = 1;

            var all = Object.FindObjectsOfType<UICircleLineFillAnimator>();
            foreach (var fillAnimator in all)
            {
                fillAnimator.FallbackInterpolationTimeInSeconds = dampTime;
                EditorUtility.SetDirty(fillAnimator.gameObject);
            }

            var all2 = Object.FindObjectsOfType<UICircleLinePointMover>();
            foreach (var mover in all2)
            {
                mover.FallbackInterpolationTimeInSeconds = dampTime;
                EditorUtility.SetDirty(mover.gameObject);
            }


            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
#endif