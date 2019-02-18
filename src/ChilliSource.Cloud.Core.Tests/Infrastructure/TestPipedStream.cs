using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ChilliSource.Cloud.Core.Tests.Infrastructure
{
    public class TestPipedStream : IDisposable
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public TestPipedStream(ITestOutputHelper output)
        {
            _output = output;
        }

        public void Dispose()
        {
            var outputStr = Console.ToString();
            if (outputStr.Length > 0)
            {
                _output.WriteLine(outputStr);
            }
        }

        [Fact]
        public async Task TestPipeAsync()
        {
            var testSizes = new int[] { 1, 3, 64 };
            var maxBlocks = new int[] { 1, 2, 10 };

            for (int bufferSizeIdx = 0; bufferSizeIdx < testSizes.Length; bufferSizeIdx++)
                for (int maxBlockIdx = 0; maxBlockIdx < maxBlocks.Length; maxBlockIdx++)
                {
                    await TestPipeInternalAsync(testSizes[bufferSizeIdx], maxBlocks[maxBlockIdx]);
                }
        }

        private async Task TestPipeInternalAsync(int bufferSize, int maxBlocks)
        {
            var pipe = new PipedStreamManager(new PipedStreamOptions() { BlockSize = bufferSize, MaxBlocks = maxBlocks });
            XunitException assertFailedEx = null;

            var minNumberToSend = uint.MaxValue / 2;
            var maxNumberToSend = minNumberToSend + 5 * 1024;

            var readerTask = Task.Run(async () =>
            {
                using (var reader = pipe.CreateReader())
                {
                    try
                    {
                        uint i = minNumberToSend;

                        UInt32Converter converter = new UInt32Converter(0);
                        byte[] buffer = new byte[4];
                        int read = 0;

                        int position = 0;
                        for (; i <= maxNumberToSend; i++)
                        {
                            read = await reader.ReadAsync(buffer, 0, 4);
                            position += read;
                            converter.ReadFromArray(buffer);

                            Assert.True(read == 4, "read == 4");
                            Assert.True(converter.Value == i, "converter.Value == i");
                        }

                        //test if it's the end.
                        read = await reader.ReadAsync(buffer, 0, 4);
                        Assert.True(read == 0, "read == 0");

                        Assert.True(reader.Position == position, "reader.Position == position");
                    }
                    catch (XunitException failedEx)
                    {
                        assertFailedEx = failedEx;
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            });

            await Task.Delay(1);

            var writerTask = Task.Run(async () =>
            {
                using (var writer = pipe.CreateWriter(throwsFailedWrite: true))
                {
                    try
                    {
                        uint i = minNumberToSend;

                        UInt32Converter converter = new UInt32Converter(0);
                        byte[] buffer = new byte[4];

                        int position = 0;
                        for (; i <= maxNumberToSend; i++)
                        {
                            converter.SetValue(i);
                            converter.WriteToArray(buffer);

                            await writer.WriteAsync(buffer, 0, 4);
                            position += 4;
                        }

                        Assert.True(writer.Position == position, "writer.Position == position");

                        await writer.FlushAsync();
                    }
                    catch (XunitException failedEx)
                    {
                        assertFailedEx = failedEx;
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            });


            try
            {
                await readerTask;
            }
            catch (Exception ex)
            {
                if (assertFailedEx == null || assertFailedEx == ex)
                {
                    throw;
                }
            }

            try
            {
                await writerTask;
            }
            catch (Exception ex)
            {
                if (assertFailedEx == null || assertFailedEx == ex)
                {
                    throw;
                }
            }
        }

        [Fact]
        public void TestPipe()
        {
            var testSizes = new int[] { 1, 3, 64 };
            var maxBlocks = new int[] { 1, 2, 10 };

            for (int bufferSizeIdx = 0; bufferSizeIdx < testSizes.Length; bufferSizeIdx++)
                for (int maxBlockIdx = 0; maxBlockIdx < maxBlocks.Length; maxBlockIdx++)
                {
                    TestPipeInternal(testSizes[bufferSizeIdx], maxBlocks[maxBlockIdx]);
                }
        }

        private void TestPipeInternal(int bufferSize, int maxBlocks)
        {
            var pipe = new PipedStreamManager(new PipedStreamOptions() { BlockSize = bufferSize, MaxBlocks = maxBlocks });
            var minNumberToSend = uint.MaxValue / 2;
            var maxNumberToSend = minNumberToSend + 5 * 1024;

            Exception writerException = null;
            Exception readerException = null;

            var writerThread = new Thread(() =>
            {
                using (var writer = pipe.CreateWriter(throwsFailedWrite: true))
                {
                    try
                    {
                        uint i = minNumberToSend;

                        UInt32Converter converter = new UInt32Converter(0);
                        byte[] buffer = new byte[4];
                        int position = 0;
                        for (; i <= maxNumberToSend; i++)
                        {
                            converter.SetValue(i);
                            converter.WriteToArray(buffer);

                            writer.Write(buffer, 0, 4);
                            position += 4;
                        }

                        Assert.True(writer.Position == position, "writer.Position == position");

                        writer.Flush();
                    }
                    catch (Exception ex)
                    {
                        writerException = ex;
                    }
                }
            });

            var readerThread = new Thread(() =>
            {
                using (var reader = pipe.CreateReader())
                {
                    try
                    {
                        uint i = minNumberToSend;

                        UInt32Converter converter = new UInt32Converter(0);
                        byte[] buffer = new byte[4];
                        int read = 0;
                        int position = 0;
                        for (; i <= maxNumberToSend; i++)
                        {
                            read = reader.Read(buffer, 0, 4);
                            position += read;
                            converter.ReadFromArray(buffer);

                            Assert.True(read == 4, "read == 4");
                            Assert.True(converter.Value == i, "converter.Value == i");
                        }

                        //test if it's the end.
                        read = reader.Read(buffer, 0, 4);
                        Assert.True(read == 0, "read == 0");

                        Assert.True(reader.Position == position, "reader.Position == position");
                    }
                    catch (Exception ex)
                    {
                        readerException = ex;
                    }
                }
            });

            readerThread.Start();
            Thread.Sleep(1);
            writerThread.Start();

            readerThread.Join();
            writerThread.Join();

            Assert.True(readerException == null, "readerException == null");
            Assert.True(writerException == null, "writerException == null");
        }


        [Fact]
        public async Task TestReaderClosed()
        {
            var pipe = new PipedStreamManager();

            var writerTask = Task.Run(async () =>
            {
                using (var writer = pipe.CreateWriter(throwsFailedWrite: true))
                {
                    byte[] buffer = new byte[1];
                    while (true)
                    {
                        await Task.Delay(10);
                        await writer.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
            });

            using (var reader = pipe.CreateReader())
            {
                reader.Close();
            }

            Exception eex = null;
            try
            {
                await writerTask;
            }
            catch (Exception ex)
            {
                eex = ex;
            }

            if (eex == null)
                throw new ApplicationException("Exception is expected.");
        }

        [Fact]
        public async Task TestWriterClosed()
        {
            var pipe = new PipedStreamManager();

            var readerTask = Task.Run(async () =>
            {
                using (var reader = pipe.CreateReader())
                {
                    byte[] buffer = new byte[1];

                    await Task.Delay(10);
                    var read = await reader.ReadAsync(buffer, 0, 1);

                    Assert.True(read == 0, "read == 0");
                }
            });

            using (var writer = pipe.CreateWriter(throwsFailedWrite: true))
            {
                writer.Close();
            }

            //Reader should not throw exception when the writer closes.
            await readerTask;
        }

        [Fact]
        public void TestReaderTimeout()
        {
            var pipe = new PipedStreamManager(new PipedStreamOptions() { BlockSize = 1, MaxBlocks = 1, ReadTimeout = TimeSpan.FromMilliseconds(100) });

            Exception eex = null;
            try
            {
                using (var reader = pipe.CreateReader())
                {
                    byte[] buffer = new byte[1];

                    //Read should get blocked for 100 ms and then time out- there's no writer 
                    var read = reader.Read(buffer, 0, 1);
                }
            }
            catch (Exception ex)
            {
                eex = ex;
            }

            if (eex == null)
                throw new ApplicationException("Exception is expected.");
        }

        [Fact]
        public void TestWriterTimeout()
        {
            var pipe = new PipedStreamManager(new PipedStreamOptions() { BlockSize = 1, MaxBlocks = 1, WriteTimeout = TimeSpan.FromMilliseconds(100) });

            Exception eex = null;
            try
            {
                using (var writer = pipe.CreateWriter(throwsFailedWrite: false))
                {
                    byte[] buffer = new byte[1];
                    writer.Write(buffer, 0, 1);

                    //Second write should get blocked for 100 ms and then time out: max blocks = 1 and we don't have a reader
                    writer.Write(buffer, 0, 1);
                }
            }
            catch (Exception ex)
            {
                eex = ex;
            }

            if (eex == null)
                throw new ApplicationException("Exception is expected.");
        }

        [Fact]
        public async Task TestMultiplexed()
        {
            var pipe = new PipedStreamManager(new PipedStreamOptions() { BlockSize = 7, MaxBlocks = 1, Multiplexed = true });
            var upperNumber = 5 * 1024;

            byte[] originalData = new byte[4 * upperNumber];
            UInt32Converter converter = new UInt32Converter(0);

            for (int i = 0; i < upperNumber; i++)
            {
                converter.SetValue(Convert.ToUInt32(i));
                converter.WriteToArray(originalData, i * 4);
            }

            var streamReaders = Enumerable.Range(0, 3)
                                .Select(i => pipe.CreateReader()).ToList();

            var readerTasks = streamReaders.Select(stream =>
            {
                return Task.Run(async () =>
                {
                    using (stream)
                    {
                        byte[] buffer = new byte[4];
                        UInt32Converter numberRead = new UInt32Converter(0);

                        for (int i = 0; i < upperNumber; i++)
                        {
                            var uintValue = Convert.ToUInt32(i);
                            var read = await stream.ReadAsync(buffer, 0, 4);
                            numberRead.ReadFromArray(buffer);

                            Assert.True(read == 4, "read == 4");
                            Assert.True(numberRead.Value == uintValue, "numberRead.Value == uintValue");
                        }
                    }
                });
            }).ToList();

            var writerTask = Task.Run(async () =>
            {
                using (var writer = pipe.CreateWriter(throwsFailedWrite: true))
                {
                    for (int i = 0; i < upperNumber; i++)
                    {
                        await writer.WriteAsync(originalData, i * 4, 4);
                    }

                    await writer.FlushAsync();
                }
            });

            foreach (var reader in readerTasks)
            {
                await reader;
            }

            await writerTask;
        }

        [Fact]
        public async Task TestPipe50MBAsync()
        {
            var pipe = new PipedStreamManager(new PipedStreamOptions() { BlockSize = 32 * 1024, MaxBlocks = 1 });
            byte[] data = new byte[50 * 1024 * 1024];
            int i = 0;
            byte byteI = 0;
            while (i < data.Length)
            {
                data[i] = byteI;
                i++;
                byteI++;
            }
            var sourceStream = new MemoryStream(data);

            Stopwatch watch = Stopwatch.StartNew();

            var writerTask = Task.Run(async () =>
            {
                using (var writer = pipe.CreateWriter(throwsFailedWrite: true))
                using (sourceStream)
                {
                    await sourceStream.CopyToAsync(writer, 31 * 1024);
                }
            });

            using (var reader = pipe.CreateReader())
            {
                byte[] buffer = new byte[31 * 1024];
                int read;
                while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    /* noop */
                }
            }

            await writerTask;
            watch.Stop();

            Console.AppendLine("Elapsed time (ms): " + watch.Elapsed.TotalMilliseconds);
        }

        [Fact]
        public async Task TestNoWriterException()
        {
            var pipe = new PipedStreamManager(new PipedStreamOptions() { BlockSize = 1, MaxBlocks = 1 });

            var readerTask = Task.Run(() =>
            {
                using (var reader = pipe.CreateReader())
                {
                    reader.Close();
                }
            });

            using (var writer = pipe.CreateWriter(throwsFailedWrite: false))
            {
                byte[] buffer = new byte[1];

                for (int i = 0; i < 30; i++)
                {
                    writer.Write(buffer, 0, 1);

                    await Task.Delay(10);
                }
            }

            await readerTask;
        }

        [Fact]
        public async Task TestWriterException()
        {
            var pipe = new PipedStreamManager(new PipedStreamOptions() { BlockSize = 1, MaxBlocks = 1 });

            var readerTask = Task.Run(() =>
            {
                using (var reader = pipe.CreateReader())
                {
                    reader.Close();
                }
            });

            Exception eex = null;

            try
            {
                using (var writer = pipe.CreateWriter(throwsFailedWrite: true))
                {
                    byte[] buffer = new byte[1];

                    for (int i = 0; i < 30; i++)
                    {
                        writer.Write(buffer, 0, 1);

                        await Task.Delay(10);
                    }
                }
            }
            catch (Exception ex)
            {
                eex = ex;
            }
            if (eex == null)
                throw new ApplicationException("Exception is expected.");

            await readerTask;
        }

        [Fact]
        public async Task TestFailedEndOfStream()
        {
            var pipe = new PipedStreamManager();
            pipe.OnWrite += (count) => { };
            pipe.OnRead += (count) => { };

            var writerTask = Task.Run(async () =>
            {
                using (var writer = pipe.CreateWriter(throwsFailedWrite: true))
                {
                    byte[] buffer = new byte[1];

                    await writer.WriteAsync(buffer, 0, buffer.Length);
                    await writer.FlushAsync();

                    await Task.Delay(10);

                    pipe.FaultPipe();
                }
            });

            Exception eex = null;
            try
            {
                using (var reader = pipe.CreateReader())
                {
                    byte[] buffer = new byte[1];

                    int read;
                    while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                eex = ex;
            }

            if (eex == null)
                throw new ApplicationException("Exception is expected.");
            else
                Console.AppendLine("TestFailedEndOfStream: " + eex.Message);

            await writerTask;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public class UInt32Converter
    {
        [FieldOffset(0)] private uint _value;
        [FieldOffset(0)] private byte Byte3;
        [FieldOffset(1)] private byte Byte2;
        [FieldOffset(2)] private byte Byte1;
        [FieldOffset(3)] private byte Byte0;

        public UInt32Converter() { }

        public UInt32Converter(uint value)
        {
            SetValue(value);
        }

        public void SetValue(uint value)
        {
            _value = value;
        }

        public uint Value { get { return _value; } }

        public void WriteToArray(byte[] array)
        {
            array[0] = Byte0;
            array[1] = Byte1;
            array[2] = Byte2;
            array[3] = Byte3;
        }

        public void WriteToArray(byte[] array, int offset)
        {
            array[offset + 0] = Byte0;
            array[offset + 1] = Byte1;
            array[offset + 2] = Byte2;
            array[offset + 3] = Byte3;
        }

        internal void ReadFromArray(byte[] array)
        {
            Byte0 = array[0];
            Byte1 = array[1];
            Byte2 = array[2];
            Byte3 = array[3];
        }
    }
}
