using UnityEngine;

namespace DRun.Client
{
	class UICircleFill_InnerDivision : UICircleFill
	{
		[Range(1, 10)]
		public int inner_subdivision = 1;

		protected override void buildMesh()
		{
			_vertexList.Clear();
			_indexList.Clear();

			Rect rect = rectTransform.rect;
			Vector2 halfSize = rect.size / 2.0f;
			Vector2 center = rect.center;

			Vector2 innerStepSize = halfSize / inner_subdivision;
			
			addVertex(center);

			int out_count = subdivision + 1;
			for(int i = 0; i < out_count; ++i)
			{
				float angle = Mathf.PI * 2.0f * i / (out_count - 1);

				int inner_count = inner_subdivision;
				
				for(int j = 0; j < inner_count; ++j)
				{
					Vector2 position = center;
					position.x += Mathf.Cos(angle) * innerStepSize.x * (j + 1);
					position.y += Mathf.Sin(angle) * innerStepSize.y * (j + 1);

					addVertex(position);
				}

				if( i == 0)
				{
					continue;
				}

				int prev_group_index = 1 + (inner_count * (i - 1));
				int next_group_index = 1 + (inner_count * i);

				// 가장 안쪽
				addTriangle(0, next_group_index, prev_group_index);

				for(int j = 1; j < inner_count; ++j)
				{
					int p0 = prev_group_index + (j - 1);
					int p1 = prev_group_index + j;
					int n0 = next_group_index + (j - 1);
					int n1 = next_group_index + j;

					addTriangle(p0, n0, n1);
					addTriangle(p0, n1, p1);
				}
			}
		}

	}
}
