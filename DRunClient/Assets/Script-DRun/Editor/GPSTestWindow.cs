using Codice.Client.BaseCommands;
using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.NetData;

using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DRun.Client
{
	public class GPSTestWindow : EditorWindow
	{
		[MenuItem("Window/DRun/GPSTest")]
		static void init()
		{
			GPSTestWindow window = EditorWindow.GetWindow<GPSTestWindow>();
			window.Show();
		}

		public TextAsset locationData = null;
		public float meterPerSecond = 5.0f;
		public string textResult = "";
		
		private Vector2 scrollPosition;

		private List<ClientLocationLog> _listOrigin;
		private List<ClientLocationLog> _listFilter;

		private List<Vector3> _polyOrigin;
		private List<Vector3> _polyFilter;
		private Vector3[] _polyOriginArray;
		private Vector3[] _polyFilterArray;

		private string _svgOutput;

		private void OnGUI()
		{
			locationData = (TextAsset)EditorGUILayout.ObjectField("LocationData(json)", locationData, typeof(TextAsset), false);

			meterPerSecond = EditorGUILayout.FloatField("MeterPerSecond", meterPerSecond);
		
			if( GUILayout.Button("test"))
			{
				runTest();

				Repaint();
			}

			EditorGUILayout.Space();

			//scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

//			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.TextArea(textResult, GUILayout.Height(300));

			//			EditorGUILayout.EndHorizontal();

			drawLines();

			//EditorGUILayout.EndScrollView();
		}

		private void runTest()
		{
			if( locationData == null)
			{
				return;
			}

			KalmanLatLong filter = null;

			StringBuilder sbOriginal = new StringBuilder();

			sbOriginal.Append("time(original)\tlongitude(original)\tlatitude(original)\taccuracy(original)\tdist(m)\tspeed(km/h)\t");
			sbOriginal.Append("time(filtered)\tlongitude(filtered)\tlatitude(filtered)\taccuracy(filtered)\tdist(m)\tspeed(km/h)\t");
			sbOriginal.Append("diff(m)\n");

			string dtFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";

			double last_lon = 0, last_lat = 0;
			double last_lon_filter = 0, last_lat_filter = 0;
			double last_timestamp = 0;

			_listOrigin= new List<ClientLocationLog>();
			_listFilter= new List<ClientLocationLog>();

			JsonArray json = new JsonArray(locationData.text);
			for(int i = json.size() -1; i >=0; --i)
			{
				JsonObject locationJson = json.getJsonObject(i);

				string event_time = locationJson.getString("event_time");

				//event_time = event_time.Substring(0, event_time.Length - 4);

				DateTime dt;
				if( TimeUtil.tryParseISO8601(event_time, out dt) == false)
				{
					throw new Exception($"can't parse datetime:{event_time}");
				}

				long timestamp = TimeUtil.unixTimestampFromDateTime(dt);
				double longitude = locationJson.getDouble("longitude");
				double latitude = locationJson.getDouble("latitude");
				double accuracy = locationJson.getDouble("accuracy");
				double dist = 0;
				double speed = 0;
				if( i < json.size() - 1)
				{
					dist = MapBoxUtil.distance(last_lon, last_lat, longitude, latitude) * 1000.0;
					speed = (dist / 1000.0) / ((double)(timestamp - last_timestamp) / 1000.0 / 3600.0);
				}

				_listOrigin.Add(ClientLocationLog.create(longitude, latitude, 0, accuracy, -1, -1, timestamp));
				sbOriginal.Append($"{dt.ToString(dtFormat)}\t{longitude}\t{latitude}\t{accuracy}\t{dist}\t{speed}");

				if ( filter == null)
				{
					filter = new KalmanLatLong(meterPerSecond);
					filter.SetState(latitude, longitude, (float)accuracy, timestamp);
				}
				else
				{
					filter.Process(latitude, longitude, (float)accuracy, timestamp);
				}

				dist = 0;
				if (i < json.size() - 1)
				{
					dist = MapBoxUtil.distance(last_lon_filter, last_lat_filter, filter.Lng, filter.Lat) * 1000.0;
					speed = (dist / 1000.0) / ((double)(timestamp - last_timestamp) / 1000.0 / 3600.0);
				}

				_listFilter.Add(ClientLocationLog.create(filter.Lng, filter.Lat, 0, filter.Accuracy, -1, -1, timestamp));
				sbOriginal.Append($"\t{TimeUtil.dateTimeFromUnixTimestamp(filter.TimeStamp).ToString(dtFormat)}\t{filter.Lng}\t{filter.Lat}\t{filter.Accuracy}\t{dist}\t{speed}");

				double diff = MapBoxUtil.distance(longitude, latitude, filter.Lng, filter.Lat) * 1000;
				sbOriginal.Append($"\t{diff}");

				sbOriginal.Append("\n");

				last_lon = longitude;
				last_lat = latitude;
				last_lon_filter = filter.Lng;
				last_lat_filter = filter.Lat;
				last_timestamp = timestamp;
			}

			textResult = sbOriginal.ToString();

			_polyOrigin = buildPathLine(_listOrigin);
			_polyFilter = buildPathLine(_listFilter);

			_polyOriginArray = _polyOrigin.ToArray();
			_polyFilterArray = _polyFilter.ToArray();
		}

		private List<Vector3> buildPathLine(List<ClientLocationLog> list)
		{
			int zoom = 16;
			double pixel_size = 300;

			MBLongLatCoordinate min = MBLongLatCoordinate.zero;
			MBLongLatCoordinate max = MBLongLatCoordinate.zero;

			for(int i = 0; i < list.Count; ++i)
			{
				ClientLocationLog log = list[i];
				
				if( i == 0)
				{
					max.pos.x = min.pos.x = log.longitude;
					max.pos.y = min.pos.y = log.latitude;
				}
				else
				{
					min.pos.x = System.Math.Min(log.longitude, min.pos.x);
					min.pos.y = System.Math.Min(log.latitude, min.pos.y);
					max.pos.x = System.Math.Max(log.longitude, max.pos.x);
					max.pos.y = System.Math.Max(log.latitude, max.pos.y);
				}
			}

			MBLongLatCoordinate center = MBLongLatCoordinate.zero;
			center.pos = (max.pos + min.pos) / 2.0;

			MBTileCoordinateDouble centerPos = MBTileCoordinateDouble.fromLonLat(center, zoom);
			MBTileCoordinateDouble minPos = MBTileCoordinateDouble.fromLonLat(min, zoom);
			MBTileCoordinateDouble maxPos = MBTileCoordinateDouble.fromLonLat(max, zoom);

			DoubleVector2 size = new DoubleVector2();
			size.x = Math.Abs(maxPos.tile_x - minPos.tile_x);
			size.y = Math.Abs(maxPos.tile_y - minPos.tile_y);

			DoubleVector2 scale = new DoubleVector2();
			scale.x = (pixel_size / 2.0) / (size.x / 2.0);
			scale.y = (pixel_size / 2.0) / (size.y / 2.0);

			List<Vector3> polyLine = new List<Vector3>();

			for (int i = 0; i < list.Count; ++i)
			{
				ClientLocationLog log = list[i];

				MBTileCoordinateDouble pos = MBTileCoordinateDouble.fromLonLat(log.longitude, log.latitude, zoom);

				DoubleVector2 offset = pos.tile_pos - centerPos.tile_pos;
				offset.x *= scale.x;
				offset.y *= scale.y;
				offset.x += pixel_size / 2.0;
				offset.y += pixel_size / 2.0;

				polyLine.Add(new Vector3((float)offset.x, (float)offset.y, 0));
			}

			return polyLine;
		}

		void drawLines()
		{
			Rect layoutRectangle = GUILayoutUtility.GetRect(300, 300, 300, 300);

			if ( Event.current.type != EventType.Repaint)
			{
				return;
			}

			if (_polyOriginArray == null || _polyFilterArray == null)
				return;

			//Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(300), GUILayout.Width(300));
			//GUI.BeginClip(rect);

			//GUI.BeginClip(layoutRectangle, scrollPosition, Vector2.zero, false);
			GUI.BeginClip(layoutRectangle);//, scrollPosition, Vector2.zero, false);

			Handles.color = Color.red;
			Handles.DrawPolyLine(_polyOriginArray);

			Handles.color = Color.blue;
			Handles.DrawPolyLine(_polyFilterArray);
/*
			GL.PushMatrix();
			GL.Clear(true, false, Color.black);
			// material.SetPass(0)

			GL.Begin(GL.QUADS);
			GL.Color(Color.black);
			GL.Vertex3(0, 0, 0);
			GL.Vertex3(layoutRectangle.width, 0, 0);
			GL.Vertex3(layoutRectangle.width, layoutRectangle.height, 0);
			GL.Vertex3(0, layoutRectangle.height, 0);
			GL.End();

			GL.Begin(GL.LINES);

			for (int i = 0; i < _polyOrigin.Count - 1; ++i)
			{
				Vector3 begin = _polyOrigin[i];
				Vector3 end = _polyOrigin[i + 1];

				GL.Color(Color.blue);
				GL.Vertex3(begin.x, begin.y, begin.z);
				GL.Vertex3(end.x, end.y, end.z);
			}

			GL.End();
			GL.PopMatrix();
*/
			GUI.EndClip();
		}
	}


}

