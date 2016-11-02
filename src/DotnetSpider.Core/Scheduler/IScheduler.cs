using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Scheduler is the part of url management. 
	/// You can implement interface Scheduler to do:
	/// manage urls to fetch
	/// remove duplicate urls
	/// </summary>
	public interface IScheduler : IDisposable, IMonitorable
	{
		bool DepthFirst { get; set; }

		void Init(ISpider spider);

		/// <summary>
		/// Add a url to fetch
		/// </summary>
		/// <param name="request"></param>
		void Push(Request request);

		/// <summary>
		/// Get an url to crawl
		/// </summary>
		/// <returns></returns>
		Request Poll();

		void Clear();

		void Load(HashSet<Request> requests);
	}
}
