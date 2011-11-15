// 
//  Copyright 2010  Trust4 Developers
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Data4
{
    [Serializable()]
    public class ID : ISerializable
    {
        private byte[] m_Bytes;

        public ID(IEnumerable<byte> bytes)
        {
            int i = 0;
            this.m_Bytes = new byte[64];
            foreach (byte b in bytes)
            {
                this.m_Bytes[i] = b;
                i += 1;

                // Take only the first 64 bytes.
                if (i >= 64)
                    break;
            }
        }

        public ID(Guid a, Guid b, Guid c, Guid d)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(a.ToByteArray());
            bytes.AddRange(b.ToByteArray());
            bytes.AddRange(c.ToByteArray());
            bytes.AddRange(d.ToByteArray());
            this.m_Bytes = bytes.ToArray();
        }

        public ID(SerializationInfo info, StreamingContext context)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < 64; i += 1)
                bytes.Add(info.GetByte("k" + i.ToString()));
            this.m_Bytes = bytes.ToArray();
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

            if (a.m_Bytes.Length != b.m_Bytes.Length)
                return false;

            bool same = true;
            for (int i = 0; i < a.m_Bytes.Length; i += 1)
            {
                if (a.m_Bytes[i] != b.m_Bytes[i])
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

            if (a.m_Bytes.Length != b.m_Bytes.Length)
                return true;

            bool same = true;
            for (int i = 0; i < a.m_Bytes.Length; i += 1)
            {
                if (a.m_Bytes[i] != b.m_Bytes[i])
                {
                    same = false;
                    break;
                }
            }

            return !same;
        }

        public override string ToString()
        {
            if (this.m_Bytes == null)
                return new Guid().ToString() + " " + new Guid().ToString() + " " + new Guid().ToString() + " " + new Guid().ToString();

            return new Guid(this.m_Bytes.Skip(0).Take(16).ToArray()).ToString() + " " +
                new Guid(this.m_Bytes.Skip(16).Take(16).ToArray()).ToString() + " " +
                new Guid(this.m_Bytes.Skip(32).Take(16).ToArray()).ToString() + " " +
                new Guid(this.m_Bytes.Skip(48).Take(16).ToArray()).ToString();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            for (int i = 0; i < 64; i += 1)
                info.AddValue("k" + i.ToString(), this.m_Bytes[i]);
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

        public ID GetHashedKey()
        {
            ID unhashedKey1 = this + 1;
            ID unhashedKey2 = unhashedKey1 + 1;
            ID unhashedKey3 = unhashedKey2 + 1;
            SHA512 hasher = SHA512.Create();
            byte[] b = hasher.ComputeHash(
                    this
                    .GetBytes()
                    .ToArray()
                    )
                .Append(hasher.ComputeHash(unhashedKey1.GetBytes().ToArray()))
                .Append(hasher.ComputeHash(unhashedKey2.GetBytes().ToArray()))
                .Append(hasher.ComputeHash(unhashedKey3.GetBytes().ToArray()))
                .ToArray();

            if (b.Length * 8 < 512)
                throw new Exception("Length of array should be " + 512 + " bits or more");

            return new ID(b.Take(512));
        }

        public static ID operator +(ID a, int value)
        {
            byte[] b = new byte[a.m_Bytes.Length];
            a.m_Bytes.CopyTo(b, 0);

            for (int i = b.Length - 1; i >= 0; i--)
            {
                int v = b[i] + value;
                if (v > byte.MaxValue)
                {
                    b[i] = (byte) ( value % byte.MaxValue );
                    value -= byte.MaxValue * b[i];
                }
                else
                    b[i] = (byte) v;
            }

            return new ID(b);
        }

        /// <summary>
        /// Gets the bytes which make up this identifier
        /// </summary>
        /// <returns></returns>
        public IEnumerable<byte> GetBytes()
        {
            return this.m_Bytes;
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> before, IEnumerable<T> after)
        {
            foreach (var item in before)
                yield return item;
            foreach (var item in after)
                yield return item;
        }
    }
}

