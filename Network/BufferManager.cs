using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Network
{
    internal class BufferManager : IDisposable
    {
        private List<byte> _buffer = new List<byte>();

        #region Add/Insert

        public void AddInt(int value, bool isReversed = false)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (isReversed)
                bytes = bytes.Reverse().ToArray();
            //_buffer.Add((byte)bytes.Length);
            _buffer.AddRange(bytes);
        }
        public void AddLong(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            //_buffer.Add((byte)bytes.Length);
            _buffer.AddRange(bytes);
        }
        public void AddShort(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            //_buffer.Add((byte)bytes.Length);
            _buffer.AddRange(bytes);
        }
        public void AddUShort(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            //_buffer.Add((byte)bytes.Length);
            _buffer.AddRange(bytes);
        }
        public void AddULong(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            //_buffer.Add((byte)bytes.Length);
            _buffer.AddRange(bytes);
        }
        public void AddDouble(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            //_buffer.Add((byte)bytes.Length);
            _buffer.AddRange(bytes);
        }
        public void AddFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            //_buffer.Add((byte)bytes.Length);
            _buffer.AddRange(bytes);
        }
        public void AddString(string value, bool useUShort = false)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (!useUShort)
                AddVarInt(bytes.Length);
            else
            {
                //AddUShort((ushort)bytes.Length);
                AddByte((byte)(bytes.Length >> 8));
                AddByte((byte)bytes.Length);
            }
            //AddUShort((ushort)bytes.Length);
            //_buffer.Add((byte)bytes.Length);
            _buffer.AddRange(bytes);
        }
        public void AddBytes(byte[] value, bool includeLength = true)
        {
            if (includeLength)
                _buffer.Add((byte)value.Length);
            _buffer.AddRange(value);
        }
        public void AddByte(byte value)
        {
            //_buffer.Add((byte)1);
            _buffer.Add(value);
        }
        public void AddBool(bool value)
        {
            byte boolByte = value ? (byte)1 : (byte)0;
            _buffer.Add(boolByte);
        }

        public void AddVarInt(int value)
        {
            while ((value & -128) != 0)
            {
                _buffer.Add((byte)(value & 127 | 128));
                value = (int)(((uint)value) >> 7);
            }
            _buffer.Add((byte)value);
        }

        public void AddVarLong(long value)
        {
            while ((value & 128) != 0)
            {
                _buffer.Add((byte)(value & 127 | 128));
                value = (int)((uint)value) >> 7;
            }
            _buffer.Add((byte)value);
        }

        public void SetPacketId(byte id)
        {
            _buffer = new List<byte>();

            if (_buffer.Count < 1)
                _buffer.Add(id);
            else
                _buffer[0] = id;
        }

        public void SetPacketUid(int puid)
        {
            _buffer.InsertRange(1, BitConverter.GetBytes(puid));
        }
        public static byte[] SetPacketUid(int puid, byte[] bytes)
        {
            List<byte> buffer = bytes.ToList();
            buffer.InsertRange(1, BitConverter.GetBytes(puid));
            return buffer.ToArray();
        }

        #endregion

        #region Get/Retreive
        public int GetPacketSize()
        {
            if (_buffer.Count > 0)
            {
                int size = ReadVarInt();
                //_buffer.RemoveAt(0);
                return size;
            }
            return -1;
        }
        public int GetPacketId(int offset = 0)
        {
            if (_buffer.Count > offset)
            {
                int id = _buffer[offset];
                _buffer.RemoveAt(0);
                return id;
            }
            return -1;
        }

        public int GetIntToVarIntLength(int value)
        {
            int i = 0;
            while (true)
            {
                if ((value & ~128) == 0)
                {
                    i++;
                    break;
                }
                //_buffer.Add((byte)(value & 127 | 128));
                i++;
                value >>>= 7;
            }
            //_buffer.Add((byte)value);
            return i;
        }

        public int ReadVarInt()
        {
            var value = 0;
            var size = 0;
            int i = 0;
            int b;
            while (((b = _buffer[0]) & 0x80) == 0x80)
            {
                value |= (b & 0x7F) << (size++ * 7);
                if (size > 5)
                {
                    continue;
                }
                i++;
                _buffer.RemoveAt(0);
            }
            _buffer.RemoveAt(0);

            return value | ((b & 0x7F) << (size * 7));

            /*int value = 0;
            int position = 0;
            byte currentByte;

            while (true)
            {
                currentByte = _buffer[0];
                _buffer.RemoveAt(0);
                value |= (currentByte & 0x7F) << position;

                if ((currentByte & 0x80) == 0) break;

                position += 7;

                if (position >= 32) throw new Exception("FUCK YOU");
            }

            return value;*/
        }

        private List<byte> _varIntOffsetBuffer = new List<byte>(5);
        public int GetVarIntOffset()
        {
            _varIntOffsetBuffer = _buffer.Take(5).ToList();
            int value = 0;
            int position = 0;
            int i = 0;

            while (true)
            {
                value |= (_varIntOffsetBuffer[0] & 0x7F) << position;
                i++;

                if ((_varIntOffsetBuffer[0] & 0x80) == 0) break;

                _varIntOffsetBuffer.RemoveAt(0);

                position += 7;

                if (position >= 32) break;
            }
            return i;
        }

        public int GetInt()
        {
            byte[] result = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                result[i] = _buffer[i];
            }

            _buffer.RemoveRange(0, 4);

            return BitConverter.ToInt32(result);
        }
        /*public int GetLong()
        {
            byte[] result = new byte[_buffer[0]];

            for (int i = 1; i < _buffer[0] + 1; i++)
            {
                result[i - 1] = _buffer[i];
            }

            _buffer.RemoveRange(0, _buffer[0] + 1);

            return BitConverter.ToInt32(result);
        }*/
        public long GetLong()
        {
            byte[] result = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                result[i] = _buffer[i];
            }

            _buffer.RemoveRange(0, 8);

            return BitConverter.ToInt64(result);
        }
        public short GetShort()
        {
            byte[] result = new byte[2];

            for (int i = 0; i < 2; i++)
            {
                result[i] = _buffer[i];
            }

            _buffer.RemoveRange(0, 2);

            return BitConverter.ToInt16(result);
        }
        public string GetString()
        {
            byte[] result = new byte[_buffer[0]];

            for (int i = 1; i < (int)_buffer[0] + 1; i++)
            {
                result[i - 1] = _buffer[i];
            }

            _buffer.RemoveRange(0, (int)_buffer[0] + 1);

            return Encoding.UTF8.GetString(result);
        }
        public bool GetBool()
        {
            bool value = _buffer[0] != 0;
            _buffer.RemoveAt(0);
            return value;
        }

        #endregion

        public void SetBytes(byte[] bytes)
        {
            _buffer = bytes.ToList();
        }
        public void InsertAt(byte[] bytes, int offset)
        {
            Array.Copy(bytes, _buffer.ToArray(), offset);
        }
        public void InsertBytes(byte[] bytes)
        {
            _buffer.AddRange(bytes);
        }
        public byte[] GetBytes()
        {
            return _buffer.ToArray();
        }
        public void RemoveRangeByte(int range)
        {
            _buffer.RemoveRange(0, range);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
