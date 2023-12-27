using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPLinePlacement : MonoBehaviour
{
    public List<Vector2> pathList;

    private TMP_Text _tmp_text;

	private List<CircleCollider2D> _colliderList;

	public class PathCursor
	{
		public float pathDistance;

		public int pathIndex;
		public float pathLocalDistance;

		public Vector3 position;
		public float angle;
		public Vector2 perpendicular_dir;
	}

	PathCursor getCursorOnPath(float distance)
	{
		float remain_distance = distance;

		for(int i = 0; i < pathList.Count - 1; ++i)
		{
			Vector2 begin = pathList[i];
			Vector2 end = pathList[i + 1];

			float segment_distance = (end - begin).magnitude;
			if( remain_distance > segment_distance)
			{
				remain_distance -= segment_distance;
				continue;
			}
			else
			{
				float r = remain_distance / segment_distance;

				Vector2 dir = (end - begin).normalized;
				float angle = Mathf.Atan2(dir.y, dir.x);

				PathCursor cursor = new PathCursor();
				cursor.position = Vector2.Lerp(begin, end, r);
				cursor.angle = angle;
				cursor.pathIndex = i;
				cursor.pathLocalDistance = remain_distance;
				cursor.pathDistance = distance;
				cursor.perpendicular_dir = Vector2.Perpendicular(dir);

				return cursor;
			}
		}

		Vector2 diff_end = (pathList[pathList.Count - 1] - pathList[pathList.Count - 2]);
		Vector2 dir_end = diff_end.normalized;
		float angle_end = Mathf.Atan2(dir_end.y, dir_end.x); 

		PathCursor cursor_end = new PathCursor();
		cursor_end.position = pathList[pathList.Count - 1];
		cursor_end.angle = angle_end;
		cursor_end.pathIndex = pathList.Count - 1;
		cursor_end.pathLocalDistance = diff_end.magnitude;
		cursor_end.pathDistance = distance;
		cursor_end.perpendicular_dir = Vector2.Perpendicular(dir_end);
		return cursor_end;
	}

	public float getLineTotalLength()
	{
		float length = 0;
		for(int i = 0; i < pathList.Count - 1; ++i)
		{
			Vector2 begin = pathList[i];
			Vector2 end = pathList[i + 1];

			length += (end - begin).magnitude;
		}

		return length;
	}
    
    void Awake()
	{
        _tmp_text = GetComponent<TMP_Text>();
		_colliderList = new List<CircleCollider2D>();
	}

    void Start()
    {
        StartCoroutine(WrapText());
    }

	void OnValidate()
	{
		if( _tmp_text != null)
		{
			_tmp_text.havePropertiesChanged = true;
		}
	}

	IEnumerator WrapText()
	{
		_tmp_text.havePropertiesChanged = true;

		Vector3[] vertices;
		//Matrix4x4 matrix;

		//List<float> deltaList = new List<float>();
		List<float> distanceList = new List<float>();

		while (true)
		{
			if (_tmp_text.havePropertiesChanged == false)
			{
				yield return null;
				continue;
			}

			_tmp_text.ForceMeshUpdate();

			TMP_TextInfo textInfo = _tmp_text.textInfo;
			int characterCount = textInfo.characterCount;

			if (characterCount == 0)
				continue;

			distanceList.Clear();
			removeAllCollider();
			float total_length = getLineTotalLength();
			float line_center_pos = total_length / 2;

			float text_begin = 0;
			float text_end = 0;

			for (int i = 0; i < characterCount; ++i)
			{
				float pos = (textInfo.characterInfo[i].vertex_TR.position.x + textInfo.characterInfo[i].vertex_TL.position.x) / 2;
				distanceList.Add(pos + line_center_pos);

				if( i == 0)
				{
					text_begin = textInfo.characterInfo[i].vertex_TL.position.x + line_center_pos;
				}
				else if ( i == characterCount - 1)
				{
					text_end = textInfo.characterInfo[i].vertex_TR.position.x + line_center_pos;
				}
			}

			float line_thickness = 5.0f;
			float total_distance = (text_end - text_begin);
			float collider_count = Mathf.Ceil((total_distance - line_thickness)/ line_thickness);

			for (int i = 0; i < collider_count; ++i)
			{
				PathCursor cursor = getCursorOnPath(text_begin + i * line_thickness + line_thickness / 2);

				GameObject go = new GameObject();
				go.transform.SetParent(transform, false);
				go.transform.localPosition = Vector3.zero;
				CircleCollider2D col = go.AddComponent<CircleCollider2D>();
				col.offset = cursor.position;
				col.radius = line_thickness / 2;

				_colliderList.Add(col);
			}

			//float distance = 0;

			for (int i = 0; i < characterCount; ++i)
			{
				if (!textInfo.characterInfo[i].isVisible)
				{
					continue;
				}

				TMP_CharacterInfo char_info = textInfo.characterInfo[i];

				int vertexIndex = textInfo.characterInfo[i].vertexIndex;
				int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
				char charactor = textInfo.characterInfo[i].character;
				float xAdvance = textInfo.characterInfo[i].xAdvance;

				//extent.min.x = Mathf.Min(extent.min.x, m_textInfo.characterInfo[i].origin);
				//extent.min.y = Mathf.Min(extent.min.y, m_textInfo.characterInfo[i].descender);

				//extent.max.x = Mathf.Max(extent.max.x, m_textInfo.characterInfo[i].xAdvance);
				//extent.max.y = Mathf.Max(extent.max.y, m_textInfo.characterInfo[i].ascender);

				vertices = textInfo.meshInfo[materialIndex].vertices;

				Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
				//Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2,
				//	(textInfo.characterInfo[i].ascender + textInfo.characterInfo[i].descender) / 2);
				//	//textInfo.characterInfo[i].baseLine);

				float x = (vertices[vertexIndex + 2].x + vertices[vertexIndex + 0].x) / 2;
				float y = (vertices[vertexIndex + 2].y + vertices[vertexIndex + 0].y) / 2;
				float width = vertices[vertexIndex + 2].x - vertices[vertexIndex + 0].x;
				float height = vertices[vertexIndex + 2].y - vertices[vertexIndex + 0].y;

				// Apply offset to adjust our pivot point.
				vertices[vertexIndex + 0] += -offsetToMidBaseline;
				vertices[vertexIndex + 1] += -offsetToMidBaseline;
				vertices[vertexIndex + 2] += -offsetToMidBaseline;
				vertices[vertexIndex + 3] += -offsetToMidBaseline;

				Debug.Log($"{charactor}:{x},{y},{width},{height} origin[{char_info.origin}], xAdvance[{char_info.xAdvance}], descender[{char_info.descender}], ascender[{char_info.ascender}], baseline[{char_info.baseLine}]");

				PathCursor cursor = getCursorOnPath(distanceList[i]);

				Quaternion rotate = Quaternion.Euler(0, 0, cursor.angle * Mathf.Rad2Deg);

				vertices[vertexIndex + 0] = rotate * vertices[vertexIndex + 0];
				vertices[vertexIndex + 1] = rotate * vertices[vertexIndex + 1];
				vertices[vertexIndex + 2] = rotate * vertices[vertexIndex + 2];
				vertices[vertexIndex + 3] = rotate * vertices[vertexIndex + 3];

				Vector3 baselineOffset = cursor.perpendicular_dir * textInfo.characterInfo[i].baseLine;

				vertices[vertexIndex + 0] += cursor.position + baselineOffset;
				vertices[vertexIndex + 1] += cursor.position + baselineOffset;
				vertices[vertexIndex + 2] += cursor.position + baselineOffset;
				vertices[vertexIndex + 3] += cursor.position + baselineOffset;

			}

			_tmp_text.UpdateVertexData();

			yield return new WaitForSeconds(0.025f);
		}
	}

	private void removeAllCollider()
	{
		foreach(CircleCollider2D col in _colliderList)
		{
			Destroy(col.gameObject);
		}

		_colliderList.Clear();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		for(int i = 0; i < pathList.Count - 1; ++i)
		{
			Vector2 begin = pathList[i];
			Vector2 end = pathList[i + 1];

			Gizmos.DrawLine( transform.TransformPoint(begin), transform.TransformPoint(end));
		}

		PathCursor cursor = getCursorOnPath(getLineTotalLength() / 2);
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(transform.TransformPoint(cursor.position), Vector3.one);
	}

}
