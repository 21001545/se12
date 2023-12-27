using Festa.Client.Module;
using TMPro;

namespace DRun.Client
{
	public class UIInvitationCode_MyCode_Acceptor : UIInvitationCode_MyCode_CellBase
	{
		public TMP_Text txtName;
		public TMP_Text txtDate;

		public override void setup(UIInvitationCode.CellItem cellItem)
		{
			if(cellItem.acceptor._profileCache != null)
			{
				txtName.text = cellItem.acceptor._profileCache.Profile.name;
			}
			else
			{
				txtName.text = "...";
			}

			txtDate.text = cellItem.acceptor.accept_time.ToString("yyyy-MM-dd");
		}
	}
}
