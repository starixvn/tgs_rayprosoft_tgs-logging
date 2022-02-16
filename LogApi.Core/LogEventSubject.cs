using System;
using System.Reactive.Subjects;

namespace Tgs.LogApi;

internal abstract class LogEventSubject<TLogEvent> : IObservable<TLogEvent>
{
    private readonly Subject<TLogEvent> _subject = new();

    public void Send(TLogEvent logEvent)
        => _subject.OnNext(logEvent);

    public IDisposable Subscribe(IObserver<TLogEvent> observer)
        => _subject.Subscribe(observer);
}
