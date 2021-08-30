using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tgs.LogApi
{
    internal static class ReactiveExtensions
    {
        public static IDisposable BatchSubscribe<TUnit>(
            this IObservable<TUnit> observable,
            int batchSize,
            Func<TUnit[], Task> handle,
            ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

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
                            try
                            {
                                await handle(unit);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, string.Empty);
                            }

                            if (batch.OutputCount == 0)
                                batch.TriggerBatch();
                        }))
                        .Concat()
                        .Catch<Unit, Exception>(ex =>
                        {
                            logger.LogError(ex, "Batch subscribe error !");

                            return Observable.Empty<Unit>();
                        }))
                .Subscribe();
        }
    }
}
