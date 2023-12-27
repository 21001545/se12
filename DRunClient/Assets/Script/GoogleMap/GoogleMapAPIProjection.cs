using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Module;

namespace Festa.GoogleMap
{
	public class GoogleMapAPIProjection
	{
		private double PixelTileSize = 256;
		private double DegreesToRadiansRatio = 180 / Math.PI;
		private double RadiansToDegreesRatio = Math.PI / 180;
		private DoubleVector2 PixelGlobeCenter;
		private double XPixelsToDegreesRatio;
		private double YPixelsToRadiansRatio;

		public GoogleMapAPIProjection(double zoomLevel)
		{
			var pixelGlobeSize = this.PixelTileSize * Math.Pow(2, zoomLevel);
			this.XPixelsToDegreesRatio = pixelGlobeSize / 360;
			this.YPixelsToRadiansRatio = pixelGlobeSize / (2 * Math.PI);
			double halfPixelGlobeSize = pixelGlobeSize / 2;
			this.PixelGlobeCenter = new DoubleVector2(
				halfPixelGlobeSize, halfPixelGlobeSize);
		}

		public DoubleVector2 FromCoordinatesToPixel(DoubleVector2 coordinates)
		{
			var x = Math.Round(this.PixelGlobeCenter.x
				+ (coordinates.x * this.XPixelsToDegreesRatio));
			var f = Math.Min(
				Math.Max(
					 Math.Sin(coordinates.y * RadiansToDegreesRatio),
					-0.9999d),
				0.9999d);
			var y = Math.Round(this.PixelGlobeCenter.y + .5d *
				Math.Log((1d + f) / (1d - f)) * -this.YPixelsToRadiansRatio);
			return new DoubleVector2(Convert.ToSingle(x), Convert.ToSingle(y));
		}

		public DoubleVector2 FromPixelToCoordinates(DoubleVector2 pixel)
		{
			double longitude = (pixel.x - this.PixelGlobeCenter.x) /
				this.XPixelsToDegreesRatio;
			double latitude = (2 * Math.Atan(Math.Exp(
				(pixel.y - this.PixelGlobeCenter.y) / -this.YPixelsToRadiansRatio))
				- Math.PI / 2) * DegreesToRadiansRatio;
			return new DoubleVector2(latitude, longitude);
		}
	}
}
