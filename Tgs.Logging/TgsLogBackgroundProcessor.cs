using System;
using System.Collections.Concurrent;
using MessagePack;
using Tgs.MessageQueue;

namespace Tgs.Logging;

internal class TgsLogBackgroundProcessor<TRecord> : IDisposable
{
#pragma warning disable IDE0052 // 刪除未讀取的私用成員
    private readonly Task _background;
#pragma warning restore IDE0052 // 刪除未讀取的私用成員
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly BlockingCollection<TRecord> _logRecordEvents = new();

    private readonly MessagePackSerializerOptions _messagePackSerializerOptions =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

    private readonly IMessageSender _messageSender;
    private readonly string _subject;
    private bool _disposedValue;

    public TgsLogBackgroundProcessor(string subject, IMessageSender messageSender)
    {
        _subject = subject ?? throw new ArgumentNullException(nameof(subject));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _background = BeginProcessLogAsync(_cancellationTokenSource.Token);
    }

    public void AddRecord(TRecord data)
    {
        _logRecordEvents.Add(data);
    }

    public void Dispose()
    {
        // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _cancellationTokenSource.Cancel();

            if (disposing)
            {
                _logRecordEvents.CompleteAdding();
            }

            _disposedValue = true;
        }
    }

    private Task BeginProcessLogAsync(CancellationToken cancellationToken) => Task.Run(() =>
      {
          foreach (var record in _logRecordEvents.GetConsumingEnumerable(cancellationToken))
          {
              var data = MessagePackSerializer.Serialize(record, _messagePackSerializerOptions);
              _ = _messageSender.PublishAsync(_subject, data, cancellationToken).AsTask();
          }
      }, cancellationToken);
}
