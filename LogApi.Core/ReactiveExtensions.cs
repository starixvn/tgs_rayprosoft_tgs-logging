using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tgs.LogApi
{
	internal static class ReactiveExtensions
	{
		public static IDisposable BatchSubscribe<TUnit>(
			this IObservable<TUnit> observable,
			int batchSize,
			Func<TUnit[], Task> handle)
		{
			var batch = new BatchBlock<TUnit>(
				batchSize,
				new GroupingDataflowBlockOptions
				{
					Greedy = false
				});

			batch.TriggerBatch();

			return Observable
				.Using(
					() => observable.Subscribe(batch.AsObserver()),
					res => batch.AsObservable()
						.Select(unit => Observable.FromAsync(async () =>
						{
							await handle(unit);

							if (batch.OutputCount == 0)
								batch.TriggerBatch();
						}))
						.Concat())
				.Subscribe();
		}
	}
}
