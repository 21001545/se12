using DRun.Client.Running;
using Festa.Client;
using Festa.Client.Module;
using UnityEditor;
using UnityEngine;

namespace DRun.Client
{
	public class DRunTestWindow : EditorWindow
	{
		[MenuItem("Window/DRun/TestWindow")]
		static void init()
		{
			DRunTestWindow window = EditorWindow.GetWindow<DRunTestWindow>();
			window.Show();
		}

		private void OnGUI()
		{
			if( Application.isPlaying == false)
			{
				EditorGUILayout.LabelField("start player for test !!");
				return;
			}

			testScreenKeyboard();

			EditorGUILayout.LabelField("-----------------------------------");

			testTrackingType();

			EditorGUILayout.LabelField("-----------------------------------");

			testGPSTracker();

			testStepCounter();

			EditorGUILayout.LabelField("-----------------------------------");

			testOpenClaimWeeklyReward();
		}

		private void testScreenKeyboard()
		{
			TouchScreenKeyboardUtil_Editor keyboardUtil = (TouchScreenKeyboardUtil_Editor)TouchScreenKeyboardUtil.getInstance();
			if (keyboardUtil == null)
			{
				return;
			}

			bool visible = EditorGUILayout.Toggle("TouchScreenKeyboard Visible", keyboardUtil.isVisible(), GUILayout.ExpandWidth(true));
			if (keyboardUtil.isVisible() != visible)
			{
				keyboardUtil.setVisible(visible);
			}

			keyboardUtil.HeightRatio = EditorGUILayout.Slider("HeightRatio", keyboardUtil.HeightRatio, 0, 1);
		}

		private void testTrackingType()
		{
			GPSTracker_Editor tracker = GPSTracker_Editor.getInstance();
			HealthDevice_EditorDev stepCounter = HealthDevice_EditorDev._instance;
			if (tracker == null || stepCounter == null)
			{
				return;
			}

			EditorGUILayout.BeginHorizontal();

			if( GUILayout.Button("Walk"))
			{
				tracker.speedScale = 1.0f;
				tracker.changeUpdateInterval(1.0f);
				tracker.accuracy = 30;
				stepCounter.stepCountRangeMin = 5;
				stepCounter.stepCountRangeMax = 20;
				
			}

			if( GUILayout.Button("Running"))
			{
				tracker.speedScale = 2.0f;
				tracker.changeUpdateInterval(1.0f);
				tracker.accuracy = 30;
				stepCounter.stepCountRangeMin = 5;
				stepCounter.stepCountRangeMax = 20;
			}

			if (GUILayout.Button("Car(Slow)"))
			{
				tracker.speedScale = 2.0f;
				tracker.changeUpdateInterval(1.0f);
				tracker.accuracy = 30;
				stepCounter.stepCountRangeMin = 0;
				stepCounter.stepCountRangeMax = 5;
			}

			if ( GUILayout.Button("Car(Fast)"))
			{
				tracker.speedScale = 10.0f;
				tracker.changeUpdateInterval(1.0f);
				tracker.accuracy = 30;
				stepCounter.stepCountRangeMin = 0;
				stepCounter.stepCountRangeMax = 5;
			}

			if( GUILayout.Button("Subway"))
			{
				tracker.speedScale = 10.0f;
				tracker.changeUpdateInterval(1.0f);
				tracker.accuracy = 2700;
				stepCounter.stepCountRangeMin = 0;
				stepCounter.stepCountRangeMax = 5;
			}

			EditorGUILayout.EndHorizontal();
		}

		private void testGPSTracker()
		{
			GPSTracker_Editor tracker = GPSTracker_Editor.getInstance();
			if( tracker == null)
			{
				return;
			}

			tracker.rotateDirection = EditorGUILayout.Toggle("Rotate Direction", tracker.rotateDirection);
			tracker.speedScale = EditorGUILayout.Slider("SpeedScale", (float)tracker.speedScale, 0.0f, 10.0f);
			tracker.accuracy = EditorGUILayout.FloatField("Accuracy", tracker.accuracy);
			
			float newInterval = EditorGUILayout.Slider("UpdateInterval", tracker.updateInterval, 0.3f, 20.0f);
			if( newInterval != tracker.updateInterval)
			{
				tracker.changeUpdateInterval(newInterval);
			}

		}

		private void testStepCounter()
		{
			HealthDevice_EditorDev stepCounter = HealthDevice_EditorDev._instance;
			if( stepCounter == null)
			{
				return;
			}

			stepCounter.stepCountRangeMin = (int)EditorGUILayout.Slider("StepCount Min", stepCounter.stepCountRangeMin, 0, 100);
			stepCounter.stepCountRangeMax = (int)EditorGUILayout.Slider("StepCount Max", stepCounter.stepCountRangeMax, 0, 100);
		}

		private void testOpenClaimWeeklyReward()
		{
			var basicMode = ClientMain.instance.getViewModel()?.BasicMode;
			if (basicMode == null)
				return;

			if (GUILayout.Button("주간 보상 획득창 열기"))
				UIClaimWeeklyReward.getInstance()?.open(basicMode.ClaimedWeeklyRewardData);
		}
	}

}

