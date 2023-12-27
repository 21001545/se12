using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Logic
{
	public class TripAddPhotoProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private int _tripID;
		private MBLongLatCoordinate _location;
		private List<NativeGallery.NativePhotoContext> _photoList;
		private int _agentUploadID;
		private int _agentID;

		private List<string> _photoURLList;
		private JsonObject _photoData;

		private ClientTripPhoto _tripPhoto;

		public ClientTripPhoto getTripPhoto()
		{
			return _tripPhoto;
		}

		public static TripAddPhotoProcessor create(int tripID,MBLongLatCoordinate location,List<NativeGallery.NativePhotoContext> photo_list)
		{
			TripAddPhotoProcessor p = new TripAddPhotoProcessor();
			p.init(tripID,location,photo_list);
			return p;
		}

		private void init(int tripID,MBLongLatCoordinate location,List<NativeGallery.NativePhotoContext> photo_list)
		{
			base.init();
			_tripID = tripID;
			_location = location;
			_photoList = photo_list;
		}

		protected override void buildSteps()
		{
			_stepList.Add(uploadPhoto);
			_stepList.Add(uploadToStorage);
			_stepList.Add(makePhotoData);
			_stepList.Add(reqAddPhoto);
			_stepList.Add(updateViewModel);
		}

		private void uploadPhoto(Handler<AsyncResult<Module.Void>> handler)
		{
			List<string> photo_file_list = new List<string>();
			foreach(NativeGallery.NativePhotoContext photo in _photoList)
			{
				photo_file_list.Add(photo.path);
			}
			HttpFileUploader uploader = Network.createFileUploader(photo_file_list);
			uploader.run(ack => { 
				if(	ack.getInteger("result") != ResultCode.ok)
				{
					handler(Future.failedFuture(new Exception("upload photo fail")));
				}
				else
				{
					_agentUploadID = ack.getInteger("id");
					_agentID = ack.getInteger("agent_id");

					handler(Future.succeededFuture());
				}
			});
		}

		private void uploadToStorage(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Trip.UploadTripPhotoToStorageReq);
			req.put("agent_upload_id", _agentUploadID);
			req.put("agent_id", _agentID);
			req.put("id", _tripID);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_photoURLList = ack.getList<string>("file");
					handler(Future.succeededFuture());
				}
			});
		}

		private void makePhotoData(Handler<AsyncResult<Module.Void>> handler)
		{
			_photoData = new JsonObject();
			_photoData.put("url", _photoURLList[0]);    // 일단 한개만
			_photoData.put("location", _location.toJson());

			handler(Future.succeededFuture());
		}

		private void reqAddPhoto(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Trip.AddPhotoReq);
			req.put("data", _photoData);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_tripPhoto = (ClientTripPhoto)ack.get("data");
					handler(Future.succeededFuture());
				}
			});
		}

		private void updateViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			ViewModel.Trip.CurrentTripPhotoList.Add(_tripPhoto);
			handler(Future.succeededFuture());
		}
	}
}
