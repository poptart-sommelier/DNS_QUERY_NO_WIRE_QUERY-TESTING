using System;
using System.Runtime.InteropServices;
using System.Net;
using System.ComponentModel;

namespace DNSQuery
{
    public class DNSQuery
    {
        static void Main()
        {
            var result = Query("dns.google");
            Console.WriteLine(String.Join("\n", result));
        }

        public static List<string> Query(string domain)
        {
            var recordsArray = IntPtr.Zero;
            var result = DnsQuery(ref domain, DnsRecordTypes.DNS_TYPE_A, DnsQueryOptions.DNS_QUERY_NO_WIRE_QUERY, IntPtr.Zero, ref recordsArray, IntPtr.Zero);
            if (result != 0)
            {
                throw new Win32Exception(result);
            }

            DNS_RECORD record;
            var recordList = new List<string>();
            for (var recordPtr = recordsArray; !recordPtr.Equals(IntPtr.Zero); recordPtr = record.pNext)
            {
                record = (DNS_RECORD)Marshal.PtrToStructure(recordPtr, typeof(DNS_RECORD));
                if (record.wType == (int)DnsRecordTypes.DNS_TYPE_A)
                {
                    IPAddress ip = ConvertUintToIpAddress(record.Data.A.IpAddress);
                    recordList.Add(ip.ToString());
                    // recordList.Add(Marshal.PtrToStringAuto(record.Data.MX.pNameExchange));
                }
            }

            return recordList;
        }

        // EVERYTHING BELOW IS TAKEN FROM HERE:
        // http://pinvoke.net/default.aspx/dnsapi.DnsQuery

        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpstrName, DnsRecordTypes wType, DnsQueryOptions Options, IntPtr pExtra, ref IntPtr ppQueryResultsSet, IntPtr pReserved);

        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void DnsRecordListFree(IntPtr pRecordList, DNS_FREE_TYPE FreeType);

        [Flags]
        public enum DnsQueryOptions
        {
            DNS_QUERY_STANDARD = 0x0,
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 0x1,
            DNS_QUERY_USE_TCP_ONLY = 0x2,
            DNS_QUERY_NO_RECURSION = 0x4,
            DNS_QUERY_BYPASS_CACHE = 0x8,
            DNS_QUERY_NO_WIRE_QUERY = 0x10,
            DNS_QUERY_NO_LOCAL_NAME = 0x20,
            DNS_QUERY_NO_HOSTS_FILE = 0x40,
            DNS_QUERY_NO_NETBT = 0x80,
            DNS_QUERY_WIRE_ONLY = 0x100,
            DNS_QUERY_RETURN_MESSAGE = 0x200,
            DNS_QUERY_MULTICAST_ONLY = 0x400,
            DNS_QUERY_NO_MULTICAST = 0x800,
            DNS_QUERY_TREAT_AS_FQDN = 0x1000,
            DNS_QUERY_ADDRCONFIG = 0x2000,
            DNS_QUERY_DUAL_ADDR = 0x4000,
            DNS_QUERY_MULTICAST_WAIT = 0x20000,
            DNS_QUERY_MULTICAST_VERIFY = 0x40000,
            DNS_QUERY_DONT_RESET_TTL_VALUES = 0x100000,
            DNS_QUERY_DISABLE_IDN_ENCODING = 0x200000,
            DNS_QUERY_APPEND_MULTILABEL = 0x800000,
            DNS_QUERY_RESERVED = unchecked((int)0xF0000000)
        }

        public enum DnsRecordTypes
        {
            DNS_TYPE_A = 0x1,
            DNS_TYPE_NS = 0x2,
            DNS_TYPE_MD = 0x3,
            DNS_TYPE_MF = 0x4,
            DNS_TYPE_CNAME = 0x5,
            DNS_TYPE_SOA = 0x6,
            DNS_TYPE_MB = 0x7,
            DNS_TYPE_MG = 0x8,
            DNS_TYPE_MR = 0x9,
            DNS_TYPE_NULL = 0xA,
            DNS_TYPE_WKS = 0xB,
            DNS_TYPE_PTR = 0xC,
            DNS_TYPE_HINFO = 0xD,
            DNS_TYPE_MINFO = 0xE,
            DNS_TYPE_MX = 0xF,
            DNS_TYPE_TEXT = 0x10,       // This is how it's specified on MSDN
            DNS_TYPE_TXT = DNS_TYPE_TEXT,
            DNS_TYPE_RP = 0x11,
            DNS_TYPE_AFSDB = 0x12,
            DNS_TYPE_X25 = 0x13,
            DNS_TYPE_ISDN = 0x14,
            DNS_TYPE_RT = 0x15,
            DNS_TYPE_NSAP = 0x16,
            DNS_TYPE_NSAPPTR = 0x17,
            DNS_TYPE_SIG = 0x18,
            DNS_TYPE_KEY = 0x19,
            DNS_TYPE_PX = 0x1A,
            DNS_TYPE_GPOS = 0x1B,
            DNS_TYPE_AAAA = 0x1C,
            DNS_TYPE_LOC = 0x1D,
            DNS_TYPE_NXT = 0x1E,
            DNS_TYPE_EID = 0x1F,
            DNS_TYPE_NIMLOC = 0x20,
            DNS_TYPE_SRV = 0x21,
            DNS_TYPE_ATMA = 0x22,
            DNS_TYPE_NAPTR = 0x23,
            DNS_TYPE_KX = 0x24,
            DNS_TYPE_CERT = 0x25,
            DNS_TYPE_A6 = 0x26,
            DNS_TYPE_DNAME = 0x27,
            DNS_TYPE_SINK = 0x28,
            DNS_TYPE_OPT = 0x29,
            DNS_TYPE_DS = 0x2B,
            DNS_TYPE_RRSIG = 0x2E,
            DNS_TYPE_NSEC = 0x2F,
            DNS_TYPE_DNSKEY = 0x30,
            DNS_TYPE_DHCID = 0x31,
            DNS_TYPE_UINFO = 0x64,
            DNS_TYPE_UID = 0x65,
            DNS_TYPE_GID = 0x66,
            DNS_TYPE_UNSPEC = 0x67,
            DNS_TYPE_ADDRS = 0xF8,
            DNS_TYPE_TKEY = 0xF9,
            DNS_TYPE_TSIG = 0xFA,
            DNS_TYPE_IXFR = 0xFB,
            DNS_TYPE_AFXR = 0xFC,
            DNS_TYPE_MAILB = 0xFD,
            DNS_TYPE_MAILA = 0xFE,
            DNS_TYPE_ALL = 0xFF,
            DNS_TYPE_ANY = 0xFF,
            DNS_TYPE_WINS = 0xFF01,
            DNS_TYPE_WINSR = 0xFF02,
            DNS_TYPE_NBSTAT = DNS_TYPE_WINSR
        }

        public enum DNS_FREE_TYPE
        {
            DnsFreeFlat = 0,
            DnsFreeRecordList = 1,
            DnsFreeParsedMessageFields = 2
        }

        public struct DNS_RECORD
        {
            public IntPtr pNext;    // DNS_RECORD*
            public IntPtr pName;    // string
            public ushort wType;
            public ushort wDataLength;
            public FlagsUnion Flags;
            public uint dwTtl;
            public uint dwReserved;
            public DataUnion Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct FlagsUnion
        {
            [FieldOffset(0)]
            public uint DW;
            [FieldOffset(0)]
            public DNS_RECORD_FLAGS S;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_RECORD_FLAGS
        {
            internal uint data;

            // DWORD Section :2;
            public uint Section
            {
                get { return data & 0x3u; }
                set { data = (data & ~0x3u) | (value & 0x3u); }
            }

            // DWORD Delete :1;
            public uint Delete
            {
                get { return (data >> 2) & 0x1u; }
                set { data = (data & ~(0x1u << 2)) | (value & 0x1u) << 2; }
            }

            // DWORD CharSet :2;
            public uint CharSet
            {
                get { return (data >> 3) & 0x3u; }
                set { data = (data & ~(0x3u << 3)) | (value & 0x3u) << 3; }
            }

            // DWORD Unused :3;
            public uint Unused
            {
                get { return (data >> 5) & 0x7u; }
                set { data = (data & ~(0x7u << 5)) | (value & 0x7u) << 5; }
            }

            // DWORD Reserved :24;
            public uint Reserved
            {
                get { return (data >> 8) & 0xFFFFFFu; }
                set { data = (data & ~(0xFFFFFFu << 8)) | (value & 0xFFFFFFu) << 8; }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DataUnion
        {
            [FieldOffset(0)]
            public DNS_A_DATA A;
            [FieldOffset(0)]
            public DNS_SOA_DATA SOA, Soa;
            [FieldOffset(0)]
            public DNS_PTR_DATA PTR, Ptr, NS, Ns, CNAME, Cname, DNAME, Dname, MB, Mb, MD, Md, MF, Mf, MG, Mg, MR, Mr;
            [FieldOffset(0)]
            public DNS_MINFO_DATA MINFO, Minfo, RP, Rp;
            [FieldOffset(0)]
            public DNS_MX_DATA MX, Mx, AFSDB, Afsdb, RT, Rt;
            [FieldOffset(0)]
            public DNS_TXT_DATA HINFO, Hinfo, ISDN, Isdn, TXT, Txt, X25;
            [FieldOffset(0)]
            public DNS_NULL_DATA Null;
            [FieldOffset(0)]
            public DNS_WKS_DATA WKS, Wks;
            [FieldOffset(0)]
            public DNS_AAAA_DATA AAAA;
            [FieldOffset(0)]
            public DNS_KEY_DATA KEY, Key;
            [FieldOffset(0)]
            public DNS_SIG_DATA SIG, Sig;
            [FieldOffset(0)]
            public DNS_ATMA_DATA ATMA, Atma;
            [FieldOffset(0)]
            public DNS_NXT_DATA NXT, Nxt;
            [FieldOffset(0)]
            public DNS_SRV_DATA SRV, Srv;
            [FieldOffset(0)]
            public DNS_NAPTR_DATA NAPTR, Naptr;
            [FieldOffset(0)]
            public DNS_OPT_DATA OPT, Opt;
            [FieldOffset(0)]
            public DNS_DS_DATA DS, Ds;
            [FieldOffset(0)]
            public DNS_RRSIG_DATA RRSIG, Rrsig;
            [FieldOffset(0)]
            public DNS_NSEC_DATA NSEC, Nsec;
            [FieldOffset(0)]
            public DNS_DNSKEY_DATA DNSKEY, Dnskey;
            [FieldOffset(0)]
            public DNS_TKEY_DATA TKEY, Tkey;
            [FieldOffset(0)]
            public DNS_TSIG_DATA TSIG, Tsig;
            [FieldOffset(0)]
            public DNS_WINS_DATA WINS, Wins;
            [FieldOffset(0)]
            public DNS_WINSR_DATA WINSR, WinsR, NBSTAT, Nbstat;
            [FieldOffset(0)]
            public DNS_DHCID_DATA DHCID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_A_DATA
        {
            public uint IpAddress;      // IP4_ADDRESS IpAddress;
            public System.Net.IPAddress IPAddressObject { get { return new IPAddress((long)IpAddress); } }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_SOA_DATA
        {
            public IntPtr pNamePrimaryServer;       // string
            public IntPtr pNameAdministrator;       // string
            public uint dwSerialNo;
            public uint dwRefresh;
            public uint dwRetry;
            public uint dwExpire;
            public uint dwDefaultTtl;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_PTR_DATA
        {
            public IntPtr pNameHost;    // string
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_MINFO_DATA
        {
            public IntPtr pNameMailbox;     // string
            public IntPtr pNameErrorsMailbox;       // string
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_MX_DATA
        {
            public IntPtr pNameExchange;        // string
            public ushort wPreference;
            public ushort Pad;
            public string NameExchange { get { return Marshal.PtrToStringAuto(pNameExchange); } }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_TXT_DATA
        {
            public uint dwStringCount;
            public IntPtr pStringArray;     // PWSTR pStringArray[1];

            public List<string> Strings
            {
                get
                {
                    List<string> res = new List<string>((int)dwStringCount);
                    for (int i = 0; i < dwStringCount; ++i)
                    {
                        IntPtr ptr = IntPtr.Add(pStringArray, i);
                        string s = Marshal.PtrToStringUni(ptr);
                        res.Add(s);
                    }

                    return res;
                }
            }

        }


        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_NULL_DATA
        {
            public uint dwByteCount;
            public IntPtr Data;           // BYTE  Data[1];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_WKS_DATA
        {
            public uint IpAddress;      // IP4_ADDRESS IpAddress;
            public byte chProtocol;     // UCHAR       chProtocol;
            public IntPtr BitMask;        // BYTE    BitMask[1];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_AAAA_DATA
        {
            // IP6_ADDRESS Ip6Address;
            // DWORD IP6Dword[4];
            // This isn't ideal, but it should work without using the fixed and unsafe keywords
            public uint Ip6Address0;
            public uint Ip6Address1;
            public uint Ip6Address2;
            public uint Ip6Address3;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_KEY_DATA
        {
            public ushort wFlags;
            public byte chProtocol;
            public byte chAlgorithm;
            public IntPtr Key;        // BYTE Key[1];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_SIG_DATA
        {
            public IntPtr pNameSigner;      // string
            public ushort wTypeCovered;
            public byte chAlgorithm;
            public byte chLabelCount;
            public uint dwOriginalTtl;
            public uint dwExpiration;
            public uint dwTimeSigned;
            public ushort wKeyTag;
            public ushort Pad;
            public IntPtr Signature;      // BYTE  Signature[1];
        }

        public const int DNS_ATMA_MAX_ADDR_LENGTH = 20;
        public const int DNS_ATMA_FORMAT_E164 = 1;
        public const int DNS_ATMA_FORMAT_AESA = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_ATMA_DATA
        {
            public byte AddressType;
            // BYTE Address[DNS_ATMA_MAX_ADDR_LENGTH];
            // This isn't ideal, but it should work without using the fixed and unsafe keywords
            public byte Address0;
            public byte Address1;
            public byte Address2;
            public byte Address3;
            public byte Address4;
            public byte Address5;
            public byte Address6;
            public byte Address7;
            public byte Address8;
            public byte Address9;
            public byte Address10;
            public byte Address11;
            public byte Address12;
            public byte Address13;
            public byte Address14;
            public byte Address15;
            public byte Address16;
            public byte Address17;
            public byte Address18;
            public byte Address19;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_NXT_DATA
        {
            public IntPtr pNameNext;    // string
            public ushort wNumTypes;
            public IntPtr wTypes;       // WORD  wTypes[1];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_SRV_DATA
        {
            public IntPtr pNameTarget;      // string
            public ushort uPriority;
            public ushort wWeight;
            public ushort wPort;
            public ushort Pad;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_NAPTR_DATA
        {
            public ushort wOrder;
            public ushort wPreference;
            public IntPtr pFlags;       // string
            public IntPtr pService;     // string
            public IntPtr pRegularExpression;       // string
            public IntPtr pReplacement;     // string
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_OPT_DATA
        {
            public ushort wDataLength;
            public ushort wPad;
            public IntPtr Data;           // BYTE Data[1];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_DS_DATA
        {
            public ushort wKeyTag;
            public byte chAlgorithm;
            public byte chDigestType;
            public ushort wDigestLength;
            public ushort wPad;
            public IntPtr Digest;         // BYTE Digest[1];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_RRSIG_DATA
        {
            public IntPtr pNameSigner;      // string
            public ushort wTypeCovered;
            public byte chAlgorithm;
            public byte chLabelCount;
            public uint dwOriginalTtl;
            public uint dwExpiration;
            public uint dwTimeSigned;
            public ushort wKeyTag;
            public ushort Pad;
            public IntPtr Signature;      // BYTE  Signature[1];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_NSEC_DATA
        {
            public IntPtr pNextDomainName;    // string
            public ushort wTypeBitMapsLength;
            public ushort wPad;
            public IntPtr TypeBitMaps;    // BYTE  TypeBitMaps[1];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_DNSKEY_DATA
        {
            public ushort wFlags;
            public byte chProtocol;
            public byte chAlgorithm;
            public ushort wKeyLength;
            public ushort wPad;
            public IntPtr Key;        // BYTE Key[1];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_TKEY_DATA
        {
            public IntPtr pNameAlgorithm;   // string
            public IntPtr pAlgorithmPacket; // PBYTE (which is BYTE*)
            public IntPtr pKey;         // PBYTE (which is BYTE*)
            public IntPtr pOtherData;       // PBYTE (which is BYTE*)
            public uint dwCreateTime;
            public uint dwExpireTime;
            public ushort wMode;
            public ushort wError;
            public ushort wKeyLength;
            public ushort wOtherLength;
            public byte cAlgNameLength;     // UCHAR cAlgNameLength;
            public int bPacketPointers;     // BOOL  bPacketPointers;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_TSIG_DATA
        {
            public IntPtr pNameAlgorithm;   // string
            public IntPtr pAlgorithmPacket; // PBYTE (which is BYTE*)
            public IntPtr pSignature;       // PBYTE (which is BYTE*)
            public IntPtr pOtherData;       // PBYTE (which is BYTE*)
            public long i64CreateTime;
            public ushort wFudgeTime;
            public ushort wOriginalXid;
            public ushort wError;
            public ushort wSigLength;
            public ushort wOtherLength;
            public byte cAlgNameLength;     // UCHAR    cAlgNameLength;
            public int bPacketPointers;     // BOOL     bPacketPointers;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_WINS_DATA
        {
            public uint dwMappingFlag;
            public uint dwLookupTimeout;
            public uint dwCacheTimeout;
            public uint cWinsServerCount;
            public uint WinsServers;    // IP4_ADDRESS WinsServers[1];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_WINSR_DATA
        {
            public uint dwMappingFlag;
            public uint dwLookupTimeout;
            public uint dwCacheTimeout;
            public IntPtr pNameResultDomain;    // string
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DNS_DHCID_DATA
        {
            public uint dwByteCount;
            public IntPtr DHCID;          // BYTE  DHCID[1];
        }

        public static IPAddress ConvertUintToIpAddress(uint ipAddress)
        {
            // x86 is in little endian
            // Network byte order (what the IPAddress object requires) is big endian
            // Ex - 0x7F000001 is 127.0.0.1
            var addressBytes = new byte[4];
            addressBytes[0] = (byte)((ipAddress & 0xFF000000u) >> 24);
            addressBytes[1] = (byte)((ipAddress & 0x00FF0000u) >> 16);
            addressBytes[2] = (byte)((ipAddress & 0x0000FF00u) >> 8);
            addressBytes[3] = (byte)(ipAddress & 0x000000FFu);
            return new IPAddress(addressBytes);
        }

        public static IPAddress ConvertAAAAToIpAddress(DNS_AAAA_DATA data)
        {
            var addressBytes = new byte[16];
            addressBytes[0] = (byte)(data.Ip6Address0 & 0x000000FF);
            addressBytes[1] = (byte)((data.Ip6Address0 & 0x0000FF00) >> 8);
            addressBytes[2] = (byte)((data.Ip6Address0 & 0x00FF0000) >> 16);
            addressBytes[3] = (byte)((data.Ip6Address0 & 0xFF000000) >> 24);
            addressBytes[4] = (byte)(data.Ip6Address1 & 0x000000FF);
            addressBytes[5] = (byte)((data.Ip6Address1 & 0x0000FF00) >> 8);
            addressBytes[6] = (byte)((data.Ip6Address1 & 0x00FF0000) >> 16);
            addressBytes[7] = (byte)((data.Ip6Address1 & 0xFF000000) >> 24);
            addressBytes[8] = (byte)(data.Ip6Address2 & 0x000000FF);
            addressBytes[9] = (byte)((data.Ip6Address2 & 0x0000FF00) >> 8);
            addressBytes[10] = (byte)((data.Ip6Address2 & 0x00FF0000) >> 16);
            addressBytes[11] = (byte)((data.Ip6Address2 & 0xFF000000) >> 24);
            addressBytes[12] = (byte)(data.Ip6Address3 & 0x000000FF);
            addressBytes[13] = (byte)((data.Ip6Address3 & 0x0000FF00) >> 8);
            addressBytes[14] = (byte)((data.Ip6Address3 & 0x00FF0000) >> 16);
            addressBytes[15] = (byte)((data.Ip6Address3 & 0xFF000000) >> 24);

            return new IPAddress(addressBytes);
        }


    }
}
