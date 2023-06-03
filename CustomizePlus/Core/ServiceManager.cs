﻿// © Customize+.
// Licensed under the MIT license.

using CustomizePlus.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace CustomizePlus.Core
{
	//Borrowed from Brio
	public class ServiceManager
	{
		public bool IsStarted { get; private set; } = false;

		private readonly List<IService> Services = new();
		private readonly Stopwatch TickTimer = new Stopwatch();

		public ServiceManager()
		{
			Add<GPoseService>();
			Add<GPoseAmnesisKtisisWarningService>();
			Add<PosingModeDetectService>();
		}

		//--------

		private void Add<T>() where T : ServiceBase<T>, IService
		{
			var newType = typeof(T);

			var service = (T?)Activator.CreateInstance(newType);
			if (service != null)
			{
				Services.Add(service);
				service.AssignInstance();
			}
		}

		public void Start()
		{
			if (IsStarted)
				throw new Exception("Services already running");

			foreach (var service in Services)
				service.Start();

			IsStarted = true;

			TickTimer.Reset();
			TickTimer.Start();
		}

		public void Tick()
		{
			if (!this.IsStarted)
				return;

			var delta = (float)TickTimer.Elapsed.TotalSeconds;
			TickTimer.Restart();

			foreach (var service in Services)
				service.Tick(delta);
		}

		public void Dispose()
		{
			TickTimer.Stop();
			TickTimer.Reset();

			var reversed = Services.ToList();
			reversed.Reverse();

			if (IsStarted)
				foreach (var service in reversed)
					service.Stop();

			IsStarted = false;

			foreach (var service in reversed)
			{
				service.Dispose();
				service.ClearInstance();
			}

			Services.Clear();

		}
	}
}