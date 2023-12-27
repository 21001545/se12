using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using Festa.Client.Module;
using Poly2Tri;

namespace Festa.Client.MapBox
{
	public class MBMeshBuilderDebug
	{
		public void run()
		{
			try
			{
				test();
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}
		}

		public void test()
		{
			string path = Application.dataPath + "/Script/MapBox/DebugData/native_error.log";
			string text = File.ReadAllText(path);
			JsonObject json = new JsonObject(text);

			JsonArray inputCDT = json.getJsonArray("inputCDT");

			Polygon basePolygon = null;

			for (int i = 0; i < inputCDT.size(); ++i)
			{
				JsonObject jsonCDT = inputCDT.getJsonObject(i);

				List<PolygonPoint> ptList = makePTList(jsonCDT.getJsonArray("points"));

				Polygon polygon = new Polygon(ptList);
				if (i == 0)
				{
					basePolygon = polygon;
				}

				if (jsonCDT.getBool("is_hole"))
				{
					basePolygon.AddHole(polygon);
				}
			}

			Poly2Tri.P2T.Triangulate(basePolygon);
		}

		private List<Poly2Tri.PolygonPoint> makePTList(JsonArray array)
		{
			List<Poly2Tri.PolygonPoint> list = new List<Poly2Tri.PolygonPoint>();
			for(int i = 0; i < array.size(); ++i)
			{
				JsonObject pt = array.getJsonObject(i);
				double x = System.Math.Floor(pt.getDouble("x"));
				double y = System.Math.Floor(pt.getDouble("y"));

				list.Add(new Poly2Tri.PolygonPoint(x, y));
			}

			return list;
		}
	}
}
