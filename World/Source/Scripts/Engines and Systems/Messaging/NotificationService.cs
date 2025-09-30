using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Server.Engines.Messaging
{
	public class NotificationService
	{
		private readonly ConcurrentQueue<string> m_MessageQueue;
		private readonly string m_WebhookUrl;
		private CancellationTokenSource m_CancellationTokenSource;

		public NotificationService(string webhookUrl)
		{
			m_MessageQueue = new ConcurrentQueue<string>();
			m_WebhookUrl = webhookUrl;
		}

		public static NotificationService Start(string webhookUrl)
		{
			var service = new NotificationService(webhookUrl);
			var task = Task.Factory.StartNew(() => service.StartAsync(), TaskCreationOptions.LongRunning);
			task.GetAwaiter().GetResult();

			return service;
		}

		public void QueueMessage(string message)
		{
			m_MessageQueue.Enqueue(message);
		}

		public void Stop()
		{
			var source = Interlocked.Exchange(ref m_CancellationTokenSource, new CancellationTokenSource());
			if (source != null)
			{
				source.Cancel();
				source.Dispose();
			}

			if (!m_MessageQueue.IsEmpty)
			{
				Console.WriteLine("[Notification Service] Stopping. Discarding pending messages.");
				string _;
				while (m_MessageQueue.TryDequeue(out _)) { }
			}
		}

		private async Task StartAsync()
		{
			Stop();

			var cancellationToken = m_CancellationTokenSource.Token;
			using (var m_Client = new HttpClient())
			{
				const int DEFAULT_DELAY = 5000;
				const int ERROR_DELAY = 30000;
				var delay = 0;
				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						await Task.Delay(delay, cancellationToken);
						delay = DEFAULT_DELAY;

						string message;
						if (!m_MessageQueue.TryDequeue(out message)) continue;

						using (var content = new StringContent(string.Format("{{\"content\": \"{0}\"}}", HttpUtility.JavaScriptStringEncode(message)), Encoding.UTF8, "application/json"))
						using (var result = await m_Client.PostAsync(m_WebhookUrl, content, cancellationToken))
						{
							if (result.IsSuccessStatusCode)
							{
								delay = 0;
								continue;
							}

							var content2 = await result.Content.ReadAsStringAsync();
							Console.WriteLine("[Notification] Failed to send message ({0}) - {1}", result.StatusCode, content);
							delay = ERROR_DELAY;
						}
					}
					catch (OperationCanceledException)
					{
						// ignore
					}
					catch (Exception ex)
					{
						Console.WriteLine("[Notification] Error: {0}", ex.Message);
						delay = ERROR_DELAY;
					}
				}
			}
		}
	}
}