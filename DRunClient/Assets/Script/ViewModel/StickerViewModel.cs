using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.ViewModel
{
	public class StickerViewModel : AbstractViewModel
	{
		private HashSet<int> _stickerSet;
		private ClientStickerBoard _stickerBoard;

		public ClientStickerBoard StickerBoard
		{
			get
			{
				return _stickerBoard;
			}
			set
			{
				Set(ref _stickerBoard, value);
			}
		}

		public static StickerViewModel create()
		{
			StickerViewModel vm = new StickerViewModel();
			vm.init();
			return vm;
		}

		public bool hasSticker(int sticker_id)
		{
			return _stickerSet.Contains(sticker_id);
		}

		protected override void init()
		{
			base.init();
			_stickerSet = new HashSet<int>();
			_stickerBoard = null;
		}

		public override void updateFromAck(MapPacket ack)
		{
			if( ack.contains(MapPacketKey.ClientAck.sticker))
			{
				updateList(ack.getList<ClientSticker>(MapPacketKey.ClientAck.sticker));
			}

			if( ack.contains(MapPacketKey.ClientAck.sticker_board))
			{
				_stickerBoard = (ClientStickerBoard)ack.get(MapPacketKey.ClientAck.sticker_board);
			}
		}

		private void updateList(List<ClientSticker> list)
		{
			foreach(ClientSticker sticker in list)
			{
				if( _stickerSet.Contains( sticker.sticker_id) == false)
				{
					Debug.Log($"get sticker: {sticker.sticker_id}");

					_stickerSet.Add(sticker.sticker_id);
				}
			}
		}
	}
}
