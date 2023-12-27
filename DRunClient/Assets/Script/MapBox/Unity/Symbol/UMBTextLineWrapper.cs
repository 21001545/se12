using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client.MapBox
{
	[RequireComponent(typeof(TMP_Text))]
	public class UMBTextLineWrapper : MonoBehaviour
	{
		private UMBSymbolRenderer _owner;
		private TMP_Text _text;

		//private List<float> _charDistanceList;

		public void create(UMBSymbolRenderer owner,TMP_Text text)
		{
			_owner = owner;
			_text = text;
		}

		//public IEnumerator wrapTextEndOfFrame()
		//{
		//	//yield return new WaitForEndOfFrame();
		//	//yield return new WaitForEndOfFrame();
		//	//yield return new WaitForEndOfFrame();
		//	//yield return new WaitForSeconds(0.1f);

		//	while(true)
		//	{
		//		if( _text.havePropertiesChanged == false)
		//		{
		//			yield return null;
		//			continue;
		//		}

		//		wrapText();

		//		yield return new WaitForSeconds(0.025f);
		//	}

		//}

		//public void wrapText()
		//{
		//	_text.ForceMeshUpdate();

		//	UMBSymbolPoint symbolPoint = _owner.getSymbolPoint();
		//	MBAnchorCursor cursor = symbolPoint.getAnchorCursor();

		//	TMP_TextInfo textInfo = _text.textInfo;
		//	TMP_CharacterInfo[] characterInfo = textInfo.characterInfo;

		//	int count = textInfo.characterCount;

		//	//			float text_begin = characterInfo[0].vertex_TL.position.x;
		//	//			float text_end = characterInfo[count-1].vertex_TR.position.x;

		//	Vector3[] vertices;
		//	Matrix4x4 matrix;

		//	Vector3 lastOffsetToMidBaseLine = Vector3.zero;

		//	for(int i = 0; i < count; ++i)
		//	{
		//		if( !characterInfo[i].isVisible)
		//		{
		//			continue;
		//		}

		//		int vertexIndex = characterInfo[i].vertexIndex;
		//		int materialIndex = characterInfo[i].materialReferenceIndex;
		//		vertices = textInfo.meshInfo[materialIndex].vertices;

		//		Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, characterInfo[i].baseLine);
				
		//		if( i == 0)
		//		{
		//			cursor.moveBackward(offsetToMidBaseline.x);
		//		}
		//		else
		//		{
		//			cursor.moveForward(offsetToMidBaseline.x - lastOffsetToMidBaseLine.x);
		//		}

		//		vertices[vertexIndex + 0] += -offsetToMidBaseline;
		//		vertices[vertexIndex + 1] += -offsetToMidBaseline;
		//		vertices[vertexIndex + 2] += -offsetToMidBaseline;
		//		vertices[vertexIndex + 3] += -offsetToMidBaseline;

		//		//Quaternion rotate = Quaternion.Euler(0, 0, cursor.getAngle() * Mathf.Rad2Deg);
		//		//Vector3 translate = cursor.getPositionPerpendicularDistance( characterInfo[i].baseLine);
		//		//matrix = Matrix4x4.TRS(offsetToMidBaseline, rotate, Vector3.one);

		//		//vertices[vertexIndex + 0] = matrix * vertices[vertexIndex + 0];
		//		//vertices[vertexIndex + 1] = matrix * vertices[vertexIndex + 1];
		//		//vertices[vertexIndex + 2] = matrix * vertices[vertexIndex + 2];
		//		//vertices[vertexIndex + 3] = matrix * vertices[vertexIndex + 3];

		//		lastOffsetToMidBaseLine = offsetToMidBaseline;
		//	}

		//	_text.UpdateVertexData();
		//}
	}
}
