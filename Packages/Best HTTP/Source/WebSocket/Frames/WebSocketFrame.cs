#if !BESTHTTP_DISABLE_WEBSOCKET && (!UNITY_WEBGL || UNITY_EDITOR)

using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;
using System;
using System.IO;

namespace BestHTTP.WebSocket.Frames
{
    /// <summary>
    /// Denotes a binary frame. The "Payload data" is arbitrary binary data whose interpretation is solely up to the application layer.
    /// This is the base class of all other frame writers, as all frame can be represented as a byte array.
    /// </summary>
    public struct WebSocketFrame
    {
        public WebSocketFrameTypes Type { get; private set; }

        public bool IsFinal { get; private set; }

        public byte Header { get; private set; }

        public BufferSegment Data { get; private set; }

        public WebSocket Websocket { get; private set; }

        public bool UseExtensions { get; private set; }

        public WebSocketFrame(WebSocket webSocket, WebSocketFrameTypes type, BufferSegment data)
            :this(webSocket, type, data, true)
        { }

        public WebSocketFrame(WebSocket webSocket, WebSocketFrameTypes type, BufferSegment data, bool useExtensions)
            : this(webSocket, type, data, true, useExtensions)
        {
        }

        public WebSocketFrame(WebSocket webSocket, WebSocketFrameTypes type, BufferSegment data, bool isFinal, bool useExtensions)
            :this(webSocket, type, data, isFinal, useExtensions, copyData: true)
        {

        }

        public WebSocketFrame(WebSocket webSocket, WebSocketFrameTypes type, BufferSegment data, bool isFinal, bool useExtensions, bool copyData)
        {
            this.Type = type;
            this.IsFinal = isFinal;
            this.Websocket = webSocket;
            this.UseExtensions = useExtensions;

            this.Data = data;

            if (this.Data.Data != null)
            {
                if (copyData)
                {
                    var from = this.Data;

                    var buffer = BufferPool.Get(this.Data.Count, true);
                    this.Data = new BufferSegment(buffer, 0, this.Data.Count);

                    Array.Copy(from.Data, (int)from.Offset, this.Data.Data, this.Data.Offset, this.Data.Count);
                }
            }
            else
                this.Data = BufferSegment.Empty;

            // First byte: Final Bit + Rsv flags + OpCode
            byte finalBit = (byte)(IsFinal ? 0x80 : 0x0);
            this.Header = (byte)(finalBit | (byte)Type);
        }

        public override string ToString()
        {
            return string.Format("[WebSocketFrame Type: {0}, IsFinal: {1}, Header: {2:X2}, Data: {3}, UseExtensions: {4}]",
                this.Type, this.IsFinal, this.Header, this.Data, this.UseExtensions);
        }

        public void WriteTo(Action<BufferSegment, BufferSegment> callback, uint maxFragmentSize)
        {
            DoExtensions();

            if ((this.Type == WebSocketFrameTypes.Binary || this.Type == WebSocketFrameTypes.Text) && this.Data.Count > maxFragmentSize)
            {
                this.FragmentAndSend(callback, maxFragmentSize);
            }
            else
            {
                WriteFragment(callback, this.Type, this.Header, this.Data);
            }
        }

        private void DoExtensions()
        {
            if (this.UseExtensions && this.Websocket != null && this.Websocket.Extensions != null)
            {
                for (int i = 0; i < this.Websocket.Extensions.Length; ++i)
                {
                    var ext = this.Websocket.Extensions[i];
                    if (ext != null)
                    {
                        this.Header |= ext.GetFrameHeader(this, this.Header);
                        BufferSegment newData = ext.Encode(this);

                        if (newData != this.Data)
                        {
                            BufferPool.Release(this.Data);

                            this.Data = newData;
                        }
                    }
                }
            }
        }

        private void FragmentAndSend(Action<BufferSegment, BufferSegment> callback, uint maxFragmentSize)
        {
            WriteFragment(callback, this.Type, this.Header &= 0x7F, this.Data.Slice(this.Data.Offset, (int)maxFragmentSize));

            // Skip one chunk, for the current one
            int pos = this.Data.Offset + (int)maxFragmentSize;
            while (pos < this.Data.Count)
            {
                int chunkLength = Math.Min((int)maxFragmentSize, this.Data.Count - pos);

                WriteFragment(callback, WebSocketFrameTypes.Continuation, pos + chunkLength >= this.Data.Count, this.Data.Slice((int)pos, (int)chunkLength));

                pos += chunkLength;
            }
        }

        private static void WriteFragment(Action<BufferSegment, BufferSegment> callback, WebSocketFrameTypes Type, bool IsFinal, BufferSegment Data)
        {
            // First byte: Final Bit + Rsv flags + OpCode
            byte finalBit = (byte)(IsFinal ? 0x80 : 0x0);
            byte Header = (byte)(finalBit | (byte)Type);

            WriteFragment(callback, Type, Header, Data);
        }

        private static unsafe void WriteFragment(Action<BufferSegment, BufferSegment> callback, WebSocketFrameTypes Type, byte Header, BufferSegment Data)
        {
            // For the complete documentation for this section see:
            // http://tools.ietf.org/html/rfc6455#section-5.2

            // Header(1) + Len(8) + Mask (4)
            byte[] wsHeader = BufferPool.Get(13, true);
            int pos = 0;

            // Write the header
            wsHeader[pos++] = Header;

            // The length of the "Payload data", in bytes: if 0-125, that is the payload length.  If 126, the following 2 bytes interpreted as a
            // 16-bit unsigned integer are the payload length.  If 127, the following 8 bytes interpreted as a 64-bit unsigned integer (the
            // most significant bit MUST be 0) are the payload length.  Multibyte length quantities are expressed in network byte order.
            if (Data.Count < 126)
            {
                wsHeader[pos++] = (byte)(0x80 | (byte)Data.Count);
            }
            else if (Data.Count < UInt16.MaxValue)
            {
                wsHeader[pos++] = (byte)(0x80 | 126);
                var count = (UInt16)Data.Count;
                wsHeader[pos++] = (byte)(count >> 8);
                wsHeader[pos++] = (byte)(count);
            }
            else
            {
                wsHeader[pos++] = (byte)(0x80 | 127);

                var count = (UInt64)Data.Count;
                wsHeader[pos++] = (byte)(count >> 56);
                wsHeader[pos++] = (byte)(count >> 48);
                wsHeader[pos++] = (byte)(count >> 40);
                wsHeader[pos++] = (byte)(count >> 32);
                wsHeader[pos++] = (byte)(count >> 24);
                wsHeader[pos++] = (byte)(count >> 16);
                wsHeader[pos++] = (byte)(count >> 8);
                wsHeader[pos++] = (byte)(count);
            }

            if (Data != BufferSegment.Empty)
            {
                // All frames sent from the client to the server are masked by a 32-bit value that is contained within the frame.  This field is
                // present if the mask bit is set to 1 and is absent if the mask bit is set to 0.
                // If the data is being sent by the client, the frame(s) MUST be masked.

                int hash = wsHeader.GetHashCode();

                wsHeader[pos++] = (byte)(hash >> 24);
                wsHeader[pos++] = (byte)(hash >> 16);
                wsHeader[pos++] = (byte)(hash >> 8);
                wsHeader[pos++] = (byte)(hash);

                // Do the masking.
                fixed (byte* pData = Data.Data, pmask = &wsHeader[pos - 4])
                {
                    // Here, instead of byte by byte, we reinterpret cast the data as ulongs and apply the masking so.
                    // This way, we can mask 8 bytes in one cycle, instead of just 1
                    int localLength = Data.Count / 8;
                    if (localLength > 0)
                    {
                        ulong* ulpData = (ulong*)&pData[Data.Offset];
                        uint umask = *(uint*)pmask;
                        ulong ulmask = (((ulong)umask << 32) | umask);

                        unchecked
                        {
                            for (int i = 0; i < localLength; ++i)
                                ulpData[i] = ulpData[i] ^ ulmask;
                        }
                    }

                    // Because data might not be exactly dividable by 8, we have to mask the remaining 0..6 too.
                    int from = Data.Offset + (localLength * 8);
                    localLength = from + Data.Count % 8;
                    for (int i = from; i < localLength; ++i)
                        pData[i] = (byte)(pData[i] ^ pmask[(i - from) % 4]);
                }
            }
            else
            {
                wsHeader[pos++] = 0;
                wsHeader[pos++] = 0;
                wsHeader[pos++] = 0;
                wsHeader[pos++] = 0;
            }

            callback(wsHeader.AsBuffer(pos), Data);
        }
    }
}

#endif
