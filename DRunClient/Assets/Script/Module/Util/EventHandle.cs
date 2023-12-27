using UnityEngine;
using System.Collections;

namespace Festa.Client.Module.Events
{
	public delegate void Handler();
	public delegate void Handler<T>(T arg0);
	public delegate void Handler<T0, T1>(T0 arg0, T1 arg1);
	public delegate void Handler<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);
	public delegate void Handler<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2,T3 arg3);
	public delegate void Handler<T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}

