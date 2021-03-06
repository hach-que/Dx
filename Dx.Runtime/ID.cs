using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using ProtoBuf;

namespace Dx.Runtime
{
    [ProtoContract]
    public class ID
    {
        [ProtoMember(1)]
        public byte[] Bytes { get; set; }

        public ID()
        {
        }

        public ID(IEnumerable<byte> bytes)
        {
            int i = 0;
            this.Bytes = new byte[64];
            foreach (byte b in bytes)
            {
                this.Bytes[i] = b;
                i += 1;

                // Take only the first 64 bytes.
                if (i >= 64)
                    break;
            }
        }

        public ID(Guid a, Guid b, Guid c, Guid d)
        {
            var bytes = new List<byte>();
            bytes.AddRange(a.ToByteArray());
            bytes.AddRange(b.ToByteArray());
            bytes.AddRange(c.ToByteArray());
            bytes.AddRange(d.ToByteArray());
            this.Bytes = bytes.ToArray();
        }

        public ID(SerializationInfo info, StreamingContext context)
        {
            var bytes = new List<byte>();
            for (int i = 0; i < 64; i += 1)
                bytes.Add(info.GetByte("k" + i));
            this.Bytes = bytes.ToArray();
        }

        public static ID NewRandom()
        {
            return new ID(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        }

        public static bool operator ==(ID a, ID b)
        {
            if (object.ReferenceEquals(a, b))
                return true;
            if (object.ReferenceEquals(a, null))
                return false;
            if (object.ReferenceEquals(b, null))
                return false;

            if (a.Bytes.Length != b.Bytes.Length)
                return false;

            bool same = true;
            for (int i = 0; i < a.Bytes.Length; i += 1)
            {
                if (a.Bytes[i] != b.Bytes[i])
                {
                    same = false;
                    break;
                }
            }

            return same;
        }

        public static bool operator !=(ID a, ID b)
        {
            if (object.ReferenceEquals(a, b))
                return false;
            if (object.ReferenceEquals(a, null))
                return true;
            if (object.ReferenceEquals(b, null))
                return true;

            if (a.Bytes.Length != b.Bytes.Length)
                return true;

            bool same = true;
            for (int i = 0; i < a.Bytes.Length; i += 1)
            {
                if (a.Bytes[i] != b.Bytes[i])
                {
                    same = false;
                    break;
                }
            }

            return !same;
        }

        public static ID operator +(ID a, int value)
        {
            var b = new byte[a.Bytes.Length];
            a.Bytes.CopyTo(b, 0);

            for (int i = b.Length - 1; i >= 0; i--)
            {
                int v = b[i] + value;
                if (v > byte.MaxValue)
                {
                    b[i] = (byte)(value % byte.MaxValue);
                    value -= byte.MaxValue * b[i];
                }
                else
                    b[i] = (byte)v;
            }

            return new ID(b);
        }

        /*
         * Functions below here are adapted from Identifier512 by Martin Devans
         */
        public static ID NewHash(string s)
        {
            MD5 hasher = MD5.Create();
            byte[] s1 = hasher.ComputeHash(Encoding.ASCII.GetBytes(s));
            byte[] b = s1.Append(s1).Append(s1).Append(s1).ToArray();
            if (b.Length * 8 != 512)
                throw new Exception("Length of array should be 512 bits");

            return new ID(b).GetHashedKey();
        }

        public static ID FromString(string s)
        {
            string[] ss = s.Split(new char[] { ' ' });
            if (ss.Length != 4)
                return null;
            return new ID(new Guid(ss[0]), new Guid(ss[1]), new Guid(ss[2]), new Guid(ss[3]));
        }
        
        public override string ToString()
        {
            if (this.Bytes == null)
                return new Guid() + " " + new Guid() + " " + new Guid() + " " + new Guid();

            return new Guid(this.Bytes.Skip(0).Take(16).ToArray()) + " " +
                new Guid(this.Bytes.Skip(16).Take(16).ToArray()) + " " +
                new Guid(this.Bytes.Skip(32).Take(16).ToArray()) + " " +
                new Guid(this.Bytes.Skip(48).Take(16).ToArray());
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            for (int i = 0; i < 64; i += 1)
                info.AddValue("k" + i, this.Bytes[i]);
        }

        public ID GetHashedKey()
        {
            ID unhashedKey1 = this + 1;
            ID unhashedKey2 = unhashedKey1 + 1;
            ID unhashedKey3 = unhashedKey2 + 1;
            SHA512 hasher = SHA512.Create();
            byte[] b = hasher.ComputeHash(
                    this
                    .Bytes
                    .ToArray())
                .Append(hasher.ComputeHash(unhashedKey1.Bytes.ToArray()))
                .Append(hasher.ComputeHash(unhashedKey2.Bytes.ToArray()))
                .Append(hasher.ComputeHash(unhashedKey3.Bytes.ToArray()))
                .ToArray();

            if (b.Length * 8 < 512)
                throw new Exception("Length of array should be " + 512 + " bits or more");

            return new ID(b.Take(512));
        }

        public override bool Equals(object other)
        {
            if (other is ID)
                return this == (other as ID);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return this.Bytes.Sum(value => Convert.ToInt32(value));
            }
        }
    }
}
