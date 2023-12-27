using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using System.IO;
using System.Text;
using Festa.Client.Module.MsgPack;

namespace Festa.Client.Module.Net
{

	public class TcpSocket
	{
		private int _id;
		private bool _connected = false;
		private string _host;
		private int _port;

		private Socket _socket;
		private TcpRecvBuffer _recv_buffer = new TcpRecvBuffer();
		private bool _sending = false;
		private int _buffer_size;
		private List<AbstractPacket> _write_list = new List<AbstractPacket>();
		private Queue<AbstractPacket> _read_list = new Queue<AbstractPacket>();
		private List<AbstractPacket> _temp_packet_list = new List<AbstractPacket>();

		private UnityAction<TcpSocket, bool> _connect_handler;
		private UnityAction<TcpSocket> _close_handler;
		private MemoryStream _write_buffer;
		private MemoryStream _write_buffer_single;
		private MessagePacker _msg_packer;
		private ObjectPacker _obj_packer;

		private MessageUnpacker _msg_unpacker;
		private ObjectUnpacker _obj_unpacker;

		private AsyncCallback _write_async_callback;
		private long			_last_write_time;

		public int id
		{
			get
			{
				return _id;
			}
		}

		public string Host
		{
			get
			{
				return _host;
			}
		}

		public int Port
		{
			get
			{
				return _port;
			}
		}

		public bool connected
		{
			get
			{
				return _connected;
			}
		}

		public UnityAction<TcpSocket, bool> connect_handler
		{
			get
			{
				return _connect_handler;
			}
			set
			{
				_connect_handler = value;
			}
		}

		public UnityAction<TcpSocket> close_handler
		{
			get
			{
				return _close_handler;
			}
			set
			{
				_close_handler = value;
			}
		}

		private static int _last_id = 0;

		private static int genTcpSocketID()
		{
			_last_id++;
			return _last_id;
		}

		public long getLastWriteTime()
		{
			return _last_write_time;
		}

		public static TcpSocket create(ObjectFactory of,int option_mask,int recv_buffer_size)
		{
			TcpSocket socket = new TcpSocket();
			socket.init( of, option_mask, recv_buffer_size);
			return socket;
		}

		private void init(ObjectFactory of,int option_mask,int recv_buffer_size)
		{
			_id = genTcpSocketID();
			_write_buffer = new MemoryStream();
			_write_buffer_single = new MemoryStream();

			_msg_packer = MessagePacker.create(_write_buffer_single);
			_obj_packer = ObjectPacker.create(of, option_mask);

			_msg_unpacker = MessageUnpacker.create(null);
			_obj_unpacker = ObjectUnpacker.create(of, option_mask);

			_write_async_callback = new AsyncCallback(writeComplete);
			_buffer_size = recv_buffer_size;
		}

		public void connectToServer(string host, int port, UnityAction<TcpSocket, bool> connect_handler, UnityAction<TcpSocket> close_handler)
		{
			_host = host;
			_port = port;
			_connect_handler = connect_handler;
			_close_handler = close_handler;

			startConnect();
		}

		private void startConnect()
		{
			_connected = false;

			try
			{
				Dns.BeginGetHostAddresses(_host, new AsyncCallback(resolutionComplete), null);
			}
			catch (Exception e)
			{
				Debug.LogError("StartConnect Exception : " + e.Message);

				_connect_handler(this, false);
			}
		}

		private void resolutionComplete(IAsyncResult ar)
		{
			try
			{
				IPAddress[] addresses = Dns.EndGetHostAddresses(ar);
				if (addresses.Length > 0)
				{
					IPAddress address = addresses[0];

					//				Debug.Log( string.Format("try to connect : {0}:{1}:{2}", _host, _port, address.AddressFamily));

					//_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					_socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

					// 접속 단절 문제 해결용 테스트
					if (_buffer_size != -1)
					{
						_socket.ReceiveBufferSize = _buffer_size;
						_socket.SendBufferSize = _buffer_size;
					}
					_socket.ReceiveTimeout = 0;
					_socket.SendTimeout = 0;

					//				Debug.Log( string.Format( "socket buffer sice recv:{0} send:{1}", _socket.ReceiveBufferSize, _socket.SendBufferSize));

					// #if !UNITY_IPHONE && !UNITY_EDITOR
					// 				_socket.Blocking = false;
					// #endif

					_socket.BeginConnect(address, _port, new AsyncCallback(connectComplete), null);
				}
				else
				{
					Debug.LogError("Can't resolve ip : " + _host);
					_connect_handler(this, false);
				}
			}
			catch (Exception e)
			{
				Debug.LogError("ResolutionComplete Exception : " + e.Message);
				_connect_handler(this, false);
			}
		}

		private void connectComplete(IAsyncResult ar)
		{
			try
			{
				if (_socket == null)
				{
					_connect_handler(this, false);
					return;
				}

				//			Debug.Log( "try end connect");

				_socket.EndConnect(ar);

				_connected = true;
				_last_write_time = TimeUtil.unixTimestampUtcNow();
				startReceive();

				//			Debug.Log( string.Format( "ConnectComplete : {0}:{1}", _host, _port));

				_connect_handler(this, true);
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("ConnectComlete Exception : {0}:{1} {2}", _host, _port, e.ToString()));
				shutdown();

				_connect_handler(this, false);
			}
		}

		private void startReceive()
		{
			try
			{
				_socket.BeginReceive(_recv_buffer.buffer,
									 _recv_buffer.offset,
									 _recv_buffer.capacity - _recv_buffer.offset,
									 SocketFlags.None,
									 new AsyncCallback(receiveComplete), null);
			}
			catch(Exception e)
			{
				Debug.LogError(string.Format("BeginReceive Exception : {0}:{1} {2}", _host, _port, e.ToString()));
				Debug.Log(e.StackTrace);
				shutdown();
			}
		}

		private void receiveComplete(IAsyncResult ar)
		{
			try
			{
				if (_socket == null)
				{
					return;
				}

				int len = 0;

				try
				{
					len = _socket.EndReceive(ar);

					if (len == 0)
					{
						shutdown();
						return;
					}
				}
				catch (SocketException socket_ex)
				{
					if (socket_ex.ErrorCode != 10035 && socket_ex.ErrorCode != 997)
					{
						Debug.Log("EndReceive Socket Exception : " + socket_ex.ErrorCode + " " + socket_ex.Message);
						throw socket_ex;
					}

					len = 0;
#if UNITY_EDITOR
					Debug.Log("EndReceive Socket Exception : " + socket_ex.ErrorCode + " " + socket_ex.Message);
#endif
				}
				catch (Exception ex)
				{
					Debug.Log("EndReceive Exception : " + ex.Message);
					throw ex;
				}

				if (len > 0)
				{
					int remain_bytes = _recv_buffer.offset + len;
					int cur_pos = 0;

					int packet_length_buffer_size;

					while(remain_bytes > 0)
					{
						int packet_length = readPacketHeader(_recv_buffer.buffer, cur_pos, remain_bytes);
						if( packet_length == -1)
						{
							break;
						}

						if( packet_length > 127)
						{
							packet_length_buffer_size = 2;
						}
						else
						{
							packet_length_buffer_size = 1;
						}

						if( (packet_length + packet_length_buffer_size) <= remain_bytes)
						{
							processSinglePacket(_recv_buffer.buffer, cur_pos + packet_length_buffer_size, packet_length);

							cur_pos += packet_length + packet_length_buffer_size;
							remain_bytes -= packet_length + packet_length_buffer_size;
						}
						else
						{
							break;
						}
					}

					if( remain_bytes > 0)
					{
						Buffer.BlockCopy(_recv_buffer.buffer, cur_pos, _recv_buffer.buffer, 0, remain_bytes);
						_recv_buffer.offset = cur_pos;
					}
					else
					{
						_recv_buffer.offset = 0;
					}

					// 버퍼가 모자를 수도
					if( _recv_buffer.offset >= _recv_buffer.capacity)
					{
						byte[] new_buffer = new byte[_recv_buffer.capacity * 2];
						Buffer.BlockCopy(_recv_buffer.buffer, 0, new_buffer, 0, _recv_buffer.offset);
						_recv_buffer.capacity = _recv_buffer.capacity * 2;
						_recv_buffer.buffer = new_buffer;
					}
				}

				startReceive();
			}
			catch (Exception e)
			{
				Debug.Log("ReceiveComplete Error : " + e.Message + " : " + e.GetType().Name);
				Debug.Log(e.StackTrace);
				shutdown();
			}
		}

		private int readPacketHeader(byte[] buffer,int offset,int length)
		{
			if (length < 1)
			{
				return -1;
			}

			int a = buffer[offset];
			if( (a & 0x80) != 0)
			{
				if( length < 2)
				{
					return -1;
				}

				int b = (int)buffer[offset + 1];

				return ((a & 0x7f) << 8) + b;
			}
			else
			{
				return a;
			}
		}

		private int writePacketHeader(Stream stream,int length)
		{
			if( length > 127)
			{
				stream.WriteByte( (byte)((length >> 8) | 0x80));
				stream.WriteByte((byte)(length & 0xff));
				return 2;
			}
			else
			{
				stream.WriteByte((byte)(length & 0xff));
				return 1;
			}
		}

		private void processSinglePacket(byte[] buffer, int offset, int packet_length)
		{
			using (MemoryStream read_stream = new MemoryStream(buffer, offset, packet_length))
			{
				_msg_unpacker.reset(read_stream);
				AbstractPacket packet = _obj_unpacker.unpack(_msg_unpacker) as AbstractPacket;

				if( packet == null)
				{
					throw new Exception("decode packet fail");
				}

				lock(_read_list)
				{
					_read_list.Enqueue( packet);
				}
			}
		}

		public bool hasReadList()
		{
			return _read_list.Count > 0;
		}

		public void dumpReadList(List<AbstractPacket> read_list)
		{
			if (_read_list.Count == 0)
				return;

			lock (_read_list)
			{
				int count = _read_list.Count;

				for (int i = 0; i < count; ++i)
				{
					read_list.Add(_read_list.Dequeue());
				}
			}
		}

		public void shutdown()
		{
			if (_socket != null)
			{
				//			Debug.Log( "shutdown");

				_connected = false;
				Socket delete_socket = _socket;
				_socket = null;

				if (delete_socket != null)
				{
					try
					{
						if (delete_socket.Connected)
						{
							delete_socket.Shutdown(SocketShutdown.Both);
						}
						delete_socket.Close();

						delete_socket = null;
					}
					catch (Exception e)
					{
						Debug.LogWarning(e);
						delete_socket = null;
					}
				}

				_close_handler(this);
			}
		}

		public void writePacket(AbstractPacket packet)
		{
			if (_socket == null)
			{
				return;
			}

			if (_connected == false)
			{
				return;
			}

			lock (_write_list)
			{
				_last_write_time = TimeUtil.unixTimestampUtcNow();
				_write_list.Add(packet);
				//	Debug.Log( "write packet : " + msg.getID());
			}

			if (_sending)
			{
				return;
			}

			startWrite();
		}

		public void writePackets(List<AbstractPacket> packet_list)
		{
			if (_socket == null)
			{
				return;
			}

			if (_connected == false)
			{
				return;
			}

			lock (_write_list)
			{
				_write_list.AddRange(packet_list);
			}

			if (_sending)
			{
				return;
			}

			startWrite();
		}

		private void startWrite()
		{
			_sending = true;

			//Debug.Log( "begin write");

			try
			{
				_temp_packet_list.Clear();

				lock (_write_list)
				{
					_temp_packet_list.AddRange(_write_list);
					_write_list.Clear();
				}

				_write_buffer.Position = 0;

				for (int i = 0; i < _temp_packet_list.Count; ++i)
				{
					AbstractPacket packet = _temp_packet_list[i];

					// if( msg is RTS.Net.Tcp.SyncReq)
					// {
					// 	RTS.Net.Tcp.SyncReq req = (RTS.Net.Tcp.SyncReq)msg;
					// 	Debug.Log( string.Format( "send req : {0},{1}", req.member_id, req.frame));
					// }

					_msg_packer.resetStream();
					_obj_packer.pack(_msg_packer, packet);

					int body_length = _msg_packer.getTotalWrittenBytes();

					writePacketHeader(_write_buffer, body_length);
					_write_buffer.Write(_write_buffer_single.GetBuffer(), 0, body_length);
				}

				byte[] send_bytes = _write_buffer.GetBuffer();
				int send_length = (int)_write_buffer.Position;

				_socket.BeginSend(send_bytes, 0, send_length, SocketFlags.None, _write_async_callback, null);
			}
			catch (Exception e)
			{
				Debug.LogError("StartWrite Exception : " + e.Message);
				Debug.LogError(e.StackTrace);
				shutdown();
			}

			//System.Console.WriteLine( "end write");
			//_sending = false;
		}

		private void writeComplete(IAsyncResult ar)
		{
			if (_socket == null)
				return;

			try
			{
				_socket.EndSend(ar);
				_sending = false;

				if (_write_list.Count > 0)
				{
					startWrite();
				}
			}
			catch (Exception e)
			{
				Debug.LogError("EndWrite Exception : " + e.Message);
				Debug.LogError(e.StackTrace);
				shutdown();	// 음
			}

			//		Debug.Log( "WriteComplete write : " + System.DateTime.Now.ToString("mm:ss.ffff"));
		}
	}


}
