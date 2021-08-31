using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MessagePack;
using Tgs.MessageQueue;

namespace Tgs.Logging
{
    internal class TgsLogBackgroundProcessor<TRecord> : IDisposable
    {
        private readonly Task _background;
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
            _background = BeginProcessLogAsync();
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
                if (disposing)
                {
                    _logRecordEvents.CompleteAdding();
                }

                _disposedValue = true;
            }
        }

        private Task BeginProcessLogAsync() => Task.Run(() =>
        {
            foreach (var record in _logRecordEvents.GetConsumingEnumerable())
            {
                var data = MessagePackSerializer.Serialize(record, _messagePackSerializerOptions);
                _ = _messageSender.PublishAsync(_subject, data);
            }
        });
    }
}
