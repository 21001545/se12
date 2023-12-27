using UnityEngine;

namespace DRun.Client.Extension
{
    public static class ColorExtension
    {
        public static Color rgb(this Color self) => new Color(self.r, self.g, self.b);

        public static Color rgbWithA(this Color self, float alphaOverride = 1.0f)
            => new Color(self.r, self.g, self.b, Mathf.Clamp(alphaOverride, 0, 1));

        public static Color r(this Color self) => new Color(self.r, 0, 0);
        public static Color g(this Color self) => new Color(0, self.g, 0);
        public static Color b(this Color self) => new Color(0, 0, self.b);
    }
}