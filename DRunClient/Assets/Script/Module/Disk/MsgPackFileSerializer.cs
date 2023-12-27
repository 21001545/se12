using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Festa.Client.Module.MsgPack;
using System.IO;

namespace Festa.Client.Module
{
	public class MsgPackFileSerializer
	{
		private ObjectFactory _objectFactory;
		private MultiThreadWorker _worker;

		public static MsgPackFileSerializer create(ObjectFactory of,MultiThreadWorker worker)
		{
			MsgPackFileSerializer s = new MsgPackFileSerializer();
			s.init(of,worker);
			return s;
		}

		private void init(ObjectFactory of,MultiThreadWorker worker)
		{
			_objectFactory = of;
			_worker = worker;
		}

		public void save(string path,Action<MessagePacker,ObjectPacker> handlePack,Action<bool> callback)
		{
			_worker.execute<Void>(promise => {

				FileStream outputStream = null;
				try
				{
					outputStream = File.OpenWrite(path);
					MessagePacker msgPacker = MessagePacker.create(outputStream);
					ObjectPacker packer = ObjectPacker.create(_objectFactory, SerializeOption.ALL);

					handlePack(msgPacker,packer);

					outputStream.Flush();

					promise.complete();
				}
				catch (Exception e)
				{
					promise.fail(e);
				}
				finally
				{
					if( outputStream != null)
					{
						outputStream.Close();
					}
				}

			}, result =>
			{
				if (result.failed())
				{
					Debug.LogException(result.cause());
				
					if(callback != null)
						callback(false);
				}
				else
				{
					if(callback != null)
						callback(true);
				}
			}
			);
		}

		public void load(string path, Action<MessageUnpacker, ObjectUnpacker> handleUnpack, Action<bool> callback)
		{
			_worker.execute<Void>(promise => {

				FileStream inputStream = null;
				try
				{
					inputStream = File.OpenRead(path);
					MessageUnpacker msgPacker = MessageUnpacker.create(inputStream);
					ObjectUnpacker packer = ObjectUnpacker.create(_objectFactory, SerializeOption.ALL);

					handleUnpack(msgPacker, packer);

					inputStream.Flush();

					promise.complete();
				}
				catch (Exception e)
				{
					promise.fail(e);
				}
				finally
				{
					if (inputStream != null)
					{
						inputStream.Close();
					}
				}

			}, result =>
			{
				if (result.failed())
				{
					Debug.LogException(result.cause());

					if (callback != null)
						callback(false);
				}
				else
				{
					if (callback != null)
						callback(true);
				}
			}
			);
		}

	}
}
