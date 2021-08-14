using System;
using System.Collections.Generic;

namespace Droneboi_Server
{
	public class ThreadManager
	{
		private static readonly List<Action> executeOnMainThread = new List<Action>();
		private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
		private static bool actionToExecuteOnMainThread = false;
		public static void Update()
		{
			while (true)
				ThreadManager.UpdateMain();
		}
		public static void ExecuteOnMainThread(Action _action)
		{
			if (_action == null)
			{
				Debug.Log("No action to execute on main thread!");
				return;
			}
			List<Action> obj = ThreadManager.executeOnMainThread;
			lock (obj)
			{
				ThreadManager.executeOnMainThread.Add(_action);
				ThreadManager.actionToExecuteOnMainThread = true;
			}
		}
		public static void UpdateMain()
		{
			if (ThreadManager.actionToExecuteOnMainThread)
			{
				ThreadManager.executeCopiedOnMainThread.Clear();
				List<Action> obj = ThreadManager.executeOnMainThread;
				lock (obj)
				{
					ThreadManager.executeCopiedOnMainThread.AddRange(ThreadManager.executeOnMainThread);
					ThreadManager.executeOnMainThread.Clear();
					ThreadManager.actionToExecuteOnMainThread = false;
				}
				for (int i = 0; i < ThreadManager.executeCopiedOnMainThread.Count; i++)
				{
					ThreadManager.executeCopiedOnMainThread[i]();
				}
			}
		}
	}
}