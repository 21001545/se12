using DRun.Client.RefData;
using Festa.Client;
using System.Runtime.CompilerServices;

namespace DRun.Client.NetData
{
	public class ClientNFTItem
	{
		public int token_id;
		// 201 - bronz
		// 202 - siilver
		// 203 - gold
		// 204 - platinum
		// 205 - ultra
		public int grade;			// 등급	: RefNFTGrade 연경
		public int level;			// 현재 레벨 : RefNFTLevel 연결
		public int exp;				// 현재 경험치 : 단위 미터
		public int max_distance;	// 최대 D-Stance : 단위 미터
		public int max_heart;		// 최대 Heart : 0 -> 무제한
		public int distance;		// 현재 D-Stance
		public int heart;			// 현재 Heart
		public int stamina;			// 현재 스태미나
		public System.DateTime next_refill_time;    // 다음 충전 시간 (UTC)
		public double total_distance;   // 누적 거리 (km)
		public long total_time;         // 누적 시간 (second)

		public float getEXPRatio()
		{
			RefNFTLevel next_level = GlobalRefDataContainer.getInstance().get<RefNFTLevel>(level + 1);
			if (next_level == null)
			{
				return 0;
			}

			float ratio = (float)exp / (next_level.required_disance * 1000.0f);
			ratio = System.Math.Clamp(ratio, 0, 1);
			return ratio;
		}

		public float getDistanceRatio()
		{
			float ratio = (float)distance / (float)max_distance;
			return System.Math.Clamp(ratio, 0, 1);
		}

		public float getHeartRatio()
		{
			if( max_heart == 0)
			{
				return 1;
			}
			float ratio = (float)heart / (float)max_heart;
			return System.Math.Clamp(ratio, 0, 1);
		}

		public float getStaminaRatio()
		{
			RefNFTGrade refGrade = GlobalRefDataContainer.getRefDataHelper().getNFTGrade(grade);

			return (float)stamina / ((float)refGrade.stamina * 100.0f);
		}
	}
}
