using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Tgs.LogApi;

internal class TaskSubject : IObserver<Func<Task>>, IObservable<Func<Task>>
{
    private readonly Subject<Func<Task>> _subject = new();

    public IDisposable Subscribe(IObserver<Func<Task>> observer) => _subject.Subscribe(observer);

    public void OnCompleted() => _subject.OnCompleted();

    public void OnError(Exception error) => _subject.OnError(error);

    public void OnNext(Func<Task> value) => _subject.OnNext(value);
}
