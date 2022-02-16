using System;
using ExpectedObjects;
using MessagePack;
using NUnit.Framework;

namespace Tgs.Logging.UnitTests;

public class SerializerTests
{
    [Test]
    public void Serialize_Data_Test()
    {
        var serializerOptions = MessagePackSerializerOptions.Standard.WithCompression(
            MessagePackCompression.Lz4BlockArray);

        var data = new OldAdminLogEvent
        {
            trackingid = Guid.NewGuid(),
            description = "test",
            ipaddress = "127.0.0.1",
            exception = null,
            executiontimems = 11,
            host = "localhost",
            method = "GET",
            path = "/",
            recordedon = DateTimeOffset.Now,
            requestcontent = "test123",
            requestheaders = "header",
            responsecode = 200,
            responsecontent = "response",
            responseheaders = "aaaa",
            servername = "server",
            StackTrace = "trace"
        };

        var bytes = MessagePackSerializer.Serialize(data, serializerOptions);

        var transObj = MessagePackSerializer.Deserialize<Tgs.Logging.AdminLogEvent>(bytes, serializerOptions);

        var bytes2 = MessagePackSerializer.Serialize(transObj, serializerOptions);

        var actual = MessagePackSerializer.Deserialize<OldAdminLogEvent>(bytes2, serializerOptions);

        data.ToExpectedObject().ShouldMatch(actual);
    }
}
