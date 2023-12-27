using Festa.Client.Module;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MapBoxUtil
	{
		public static void getTileXY(double lon, double lat, int zoom, out double tile_x, out double tile_y)
		{
			tile_x = (lon + 180.0) / 360.0 * (1 << zoom);
			tile_y = (1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
				1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom);
		}

		public static void fromTileXY(double tile_x,double tile_y,int zoom,out double lon,out double lat)
		{
			double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoom));

			lon = (float)((tile_x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
			lat = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));
		}

		public static int maxTile(int zoom)
		{
			return 1 << zoom;
		}

		public static bool isValidTile(int tile,int zoom)
		{
			int max = maxTile(zoom);
			return tile >= 0 && tile < max;
		}

		public static int clampTileMax(int tile,int zoom)
		{
			int max = maxTile(zoom);
			if( tile < 0)
			{
				tile = 0;
			}
			else if( tile >= max)
			{
				tile = max - 1;
			}
			return tile;
		}

		public static double clampTileMax(double tile,int zoom)
		{
			double max = maxTile(zoom);
			if (tile < 0)
			{
				tile = 0;
			}
			else if (tile >= max)
			{
				tile = max - 1;
			}
			return tile;
		}

		public static int wrapTile(int tile,int zoom)
		{
			int max = maxTile(zoom);
			tile %= max;
			if( tile < 0)
			{
				tile += max;
			}

			return tile;
		}

		public static double wrapTile(double tile,int zoom)
		{
			int max = maxTile(zoom);
			tile %= (double)max;
			if (tile < 0)
			{
				tile += max;
			}

			return tile;
		}

		public static double exponentialInterpolation(double input, double exp_base, double lower, double upper)
		{
			double diff = upper - lower;
			double progress = input - lower;

			if (diff == 0)
			{
				return 0;
			}
			else if (exp_base == 1)
			{
				return progress / diff;
			}
			else
			{
				return (System.Math.Pow(exp_base, progress) - 1) / (System.Math.Pow(exp_base, diff) - 1);
			}
		}

		public static double lerp(double begin,double end,double t)
		{
			return begin + (end - begin) * t;
		}

		public static double distance(DoubleVector2 from,DoubleVector2 to)
		{
			return distance(from.x, from.y, to.x, to.y);
		}

		// return kilometers
		public static double distance(double lon1,double lat1,double lon2,double lat2)
		{
			//double R = (type == DistanceType.Miles) ? 3960 : 6371;
			double R = 6371;

			double dLat = toRadian(lat2 - lat1);
			double dLon = toRadian(lon2 - lon1);

			double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Cos(toRadian(lat1)) * Math.Cos(toRadian(lat2)) *
				Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
			double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
			double d = R * c;

			return d;
		}

		/// <summary>
		/// Convert to Radians.
		/// </summary>
		/// <param name=”val”></param>
		/// <returns></returns>
		public static double toRadian(double val)
		{
			return (Math.PI / 180) * val;
		}

		public static void clipLines(short[] path,List<List<Vector2Int>> clipLines)
		{
			clipLines.Clear();
			List<Vector2Int> currentLine = null;

			int x1 = 0;
			int y1 = 0;
			int x2 = MapBoxDefine.tile_extent;
			int y2 = MapBoxDefine.tile_extent;

			int count = path.Length / 2;
			for (int i = 0; i < count - 1; i++)
			{
				Vector2Int p0 = new Vector2Int(path[i * 2 + 0], path[i * 2 + 1]);
				Vector2Int p1 = new Vector2Int(path[(i + 1) * 2 + 0], path[(i + 1) * 2 + 1]);

				if (p0.x < x1 && p1.x < x1)
				{
					continue;
				}
				else if (p0.x < x1)
				{
					p0.y = Mathf.RoundToInt(p0.y + (p1.y - p0.y) * (x1 - p0.x) / (p1.x - p0.x));
					p0.x = x1;
				}
				else if (p1.x < x1)
				{
					p1.y = Mathf.RoundToInt(p0.y + (p1.y - p0.y) * (x1 - p0.x) / (p1.x - p0.x));
					p1.x = x1;
				}

				if (p0.y < y1 && p1.y < y1)
				{
					continue;
				}
				else if (p0.y < y1)
				{
					p0.x = Mathf.RoundToInt(p0.x + (p1.x - p0.x) * (y1 - p0.y) / (p1.y - p0.y));
					p0.y = y1;
				}
				else if (p1.y < y1)
				{
					p1.x = Mathf.RoundToInt(p0.x + (p1.x - p0.x) * (y1 - p0.y) / (p1.y - p0.y));
					p1.y = y1;
				}

				if (p0.x >= x2 && p1.x >= x2)
				{
					continue;
				}
				else if (p0.x >= x2)
				{
					p0.y = Mathf.RoundToInt(p0.y + (p1.y - p0.y) * (x2 - p0.x) / (p1.x - p0.x));
					p0.x = x2;
				}
				else if (p1.x >= x2)
				{
					p1.y = Mathf.RoundToInt(p0.y + (p1.y - p0.y) * (x2 - p0.x) / (p1.x - p0.x));
					p1.x = x2;
				}

				if (p0.y >= y2 && p1.y >= y2)
				{
					continue;
				}
				else if (p0.y >= y2)
				{
					p0.x = Mathf.RoundToInt(p0.x + (p1.x - p0.x) * (y2 - p0.y) / (p1.y - p0.y));
					p0.y = y2;
				}
				else if (p1.y >= y2)
				{
					p1.x = Mathf.RoundToInt(p0.x + (p1.x - p0.x) * (y2 - p0.y) / (p1.y - p0.y));
					p1.y = y2;
				}

				if( clipLines.Count == 0 || (currentLine.Count > 0 && currentLine[currentLine.Count - 1] != p0))
				{
					currentLine = new List<Vector2Int>();
					clipLines.Add(currentLine);
					currentLine.Add(p0);
				}

				currentLine.Add(p1);
			}
		}

		public static float zoomToScale(float zoom)
		{
			int integer = Mathf.FloorToInt(zoom);
			float fraction = zoom - integer;

			return Mathf.Pow(2, integer) * (1.0f + fraction);
		}

		public static float scaleToZoom(float scale)
		{
			int integer_zoom = Mathf.FloorToInt(Mathf.Log(scale, 2));
			
			float base_scale = Mathf.Pow(2, integer_zoom);
			float fraction = scale / base_scale - 1.0f;

			return integer_zoom + fraction;
		}
	}
}
