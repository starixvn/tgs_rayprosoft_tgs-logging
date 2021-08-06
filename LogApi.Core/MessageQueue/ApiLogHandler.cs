using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tgs.MessageQueue;

namespace Tgs.LogApi.MessageQueue
{
	class ApiLogHandler : IMessageHandler
	{
		public ValueTask HandleAsync(byte[] data, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	}
}
