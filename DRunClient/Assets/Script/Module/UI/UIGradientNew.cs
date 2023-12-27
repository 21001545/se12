using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class UIGradientNew : BaseMeshEffect
{
    public Gradient gradient;
    public RectTransform targetRect;
	
    [Range(-180f, 180f)]
    public float m_angle = 0f;
    public bool m_ignoreRatio = true;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (enabled && gradient != null)
        {
            Rect rect = getRect();
            Vector2 dir = UIGradientUtils.RotationDir(-m_angle);

            if( rect.width == 0 || rect.height == 0)
            {
                return;
            }

            if (!m_ignoreRatio)
                dir = UIGradientUtils.CompensateAspectRatio(rect, dir);

            UIGradientUtils.Matrix2x3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);

            UIVertex vertex = default(UIVertex);
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                Vector2 localPosition = localPositionMatrix * vertex.position;
                float x = Mathf.Clamp(localPosition.x, 0, 1);
                vertex.color *= gradient.Evaluate(1 - x);
                vh.SetUIVertex(vertex, i);
            }
        }
    }

    private Rect getRect()
    {
        if( targetRect == null || targetRect == graphic.rectTransform)
        {
            return graphic.rectTransform.rect;
        }
        else
        {
            // 로컬 좌표로 변환한다
            Vector3[] worldCorners = new Vector3[4];
            targetRect.GetWorldCorners(worldCorners);

            Vector3 leftBottom = graphic.rectTransform.InverseTransformPoint(worldCorners[0]);
            Vector3 rightTop = graphic.rectTransform.InverseTransformPoint(worldCorners[2]);

            Vector2 min = Vector2.zero;
            Vector2 max = Vector2.zero;

            min.x = Mathf.Min(leftBottom.x, rightTop.x);
            min.y = Mathf.Min(leftBottom.y, rightTop.y);
            max.x = Mathf.Max(leftBottom.x, rightTop.x);
            max.y = Mathf.Max(leftBottom.y, rightTop.y);

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}