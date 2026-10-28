using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Automatak.DNP3.Interface;
using Automatak.Simulator.API;

namespace Automatak.Simulator.DNP3
{
    public class NodeConfiguration
    {
        public string Kind;
        public string Alias;
        public string Module;
        public ChannelConfiguration Channel;
        public MasterStackConfig Master;
        public OutstationStackConfig Outstation;
        public List<ScanConfiguration> Scans = new List<ScanConfiguration>();
    }

    public class ScanConfiguration
    {
        public byte Classes;
        public TimeSpan Period;
    }

    public class ChannelConfiguration
    {
        public int Type;
        public string Alias;
        public uint LogFlags;
        public double MinRetryMs;
        public double MaxRetryMs;
        public string ClientHost;
        public ushort ClientPort;
        public string ServerHost;
        public ushort ServerPort;
        public bool ClientTls;
        public bool ServerTls;
        public TlsConfiguration ClientTlsOptions = new TlsConfiguration();
        public TlsConfiguration ServerTlsOptions = new TlsConfiguration();
        public string SerialDevice;
        public int Baud;
        public int DataBits;
        public Parity Parity;
        public StopBits StopBits;
        public FlowControl FlowControl;

        public void Validate()
        {
            if (Type < 0 || Type > 2 || string.IsNullOrWhiteSpace(Alias))
                throw new InvalidDataException("Invalid channel type or alias.");
            if (MinRetryMs <= 0 || MaxRetryMs < MinRetryMs || double.IsNaN(MinRetryMs) || double.IsNaN(MaxRetryMs) ||
                double.IsInfinity(MinRetryMs) || double.IsInfinity(MaxRetryMs))
                throw new InvalidDataException("Retry delays must be positive; maximum must be at least minimum.");
            if ((Type == 0 && (string.IsNullOrWhiteSpace(ClientHost) || ClientPort == 0)) ||
                (Type == 1 && (string.IsNullOrWhiteSpace(ServerHost) || ServerPort == 0)))
                throw new InvalidDataException("Specify a host and a non-zero port.");
            if (Type == 2 && (string.IsNullOrWhiteSpace(SerialDevice) || Baud <= 0 || DataBits < 5 || DataBits > 8 ||
                !Enum.IsDefined(typeof(Parity), Parity) || !Enum.IsDefined(typeof(StopBits), StopBits) ||
                !Enum.IsDefined(typeof(FlowControl), FlowControl)))
                throw new InvalidDataException("Invalid serial settings.");
            if (Type == 0 && ClientTls) ClientTlsOptions.Validate();
            if (Type == 1 && ServerTls) ServerTlsOptions.Validate();
        }

        public IChannel Create(IDNP3Manager manager, IChannelListener listener)
        {
            Validate();
            var min = TimeSpan.FromMilliseconds(MinRetryMs);
            var retry = new ChannelRetry(min, TimeSpan.FromMilliseconds(MaxRetryMs), min);
            if (Type == 0)
            {
                var remotes = new List<IPEndpoint> { new IPEndpoint(ClientHost, ClientPort) };
                return ClientTls
                    ? manager.AddTLSClient(Alias, LogFlags, retry, remotes, ClientTlsOptions.ToNative(), listener)
                    : manager.AddTCPClient(Alias, LogFlags, retry, remotes, listener);
            }
            if (Type == 1)
            {
                var endpoint = new IPEndpoint(ServerHost, ServerPort);
                return ServerTls
                    ? manager.AddTLSServer(Alias, LogFlags, ServerAcceptMode.CloseExisting, endpoint, ServerTlsOptions.ToNative(), listener)
                    : manager.AddTCPServer(Alias, LogFlags, ServerAcceptMode.CloseExisting, endpoint, listener);
            }
            return manager.AddSerial(Alias, LogFlags, retry,
                new SerialSettings(SerialDevice, Baud, DataBits, StopBits, Parity, FlowControl), listener);
        }
    }

    public class TlsConfiguration
    {
        public string PeerCertificate = "";
        public string LocalCertificate = "";
        public string PrivateKey = "";
        public string CipherList = "";
        public bool Tls10;
        public bool Tls11;
        public bool Tls12 = true;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(PeerCertificate) || string.IsNullOrWhiteSpace(LocalCertificate) ||
                string.IsNullOrWhiteSpace(PrivateKey) || !(Tls10 || Tls11 || Tls12))
                throw new InvalidDataException("Specify TLS certificate/key paths and at least one TLS version.");
            Path.GetFullPath(PeerCertificate);
            Path.GetFullPath(LocalCertificate);
            Path.GetFullPath(PrivateKey);
        }

        public TLSConfig ToNative()
        {
            Validate();
            return new TLSConfig(PeerCertificate, LocalCertificate, PrivateKey, Tls10, Tls11, Tls12, false, CipherList);
        }
    }

    public static class ConfigurationXml
    {
        static DataContractSerializer Serializer(Type type)
        {
            return new DataContractSerializer(type, new DataContractSerializerSettings {
                DataContractSurrogate = new Dnp3ConfigurationSurrogate()
            });
        }

        public static string Write<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                Serializer(typeof(T)).WriteObject(stream, value);
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static T Read<T>(string xml)
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            using (var text = new StringReader(xml))
            using (var reader = XmlReader.Create(text, settings))
                return (T)Serializer(typeof(T)).ReadObject(reader);
        }

        public static T Clone<T>(T value) { return Read<T>(Write(value)); }
    }

    // The native API exposes these values as readonly fields without a default constructor.
    // Use explicit data contracts instead of serializing runtime objects or type names.
    public class RetryConfiguration
    {
        public bool Infinite;
        public int Count;
    }

    public class StaticTypesConfiguration
    {
        public ushort Mask;
    }

    sealed class Dnp3ConfigurationSurrogate : IDataContractSurrogate
    {
        public Type GetDataContractType(Type type)
        {
            if (type == typeof(NumRetries)) return typeof(RetryConfiguration);
            if (type == typeof(StaticTypeBitField)) return typeof(StaticTypesConfiguration);
            return type;
        }

        public object GetObjectToSerialize(object value, Type targetType)
        {
            var retries = value as NumRetries;
            if (retries != null) return new RetryConfiguration { Infinite = retries.isInfinite, Count = retries.maxNumRetries };
            if (value is StaticTypeBitField) return new StaticTypesConfiguration { Mask = ((StaticTypeBitField)value).mask };
            return value;
        }

        public object GetDeserializedObject(object value, Type targetType)
        {
            var retries = value as RetryConfiguration;
            if (retries != null)
            {
                if (retries.Count < 0) throw new InvalidDataException("Invalid retry count.");
                return retries.Infinite ? NumRetries.Infinite() : NumRetries.Fixed(retries.Count);
            }
            var types = value as StaticTypesConfiguration;
            return types == null ? value : new StaticTypeBitField(types.Mask);
        }

        public object GetCustomDataToExport(Type clrType, Type dataContractType) { return null; }
        public object GetCustomDataToExport(System.Reflection.MemberInfo member, Type dataContractType) { return null; }
        public void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<Type> types) { }
        public Type GetReferencedTypeOnImport(string name, string ns, object customData) { return null; }
        public System.CodeDom.CodeTypeDeclaration ProcessImportedType(System.CodeDom.CodeTypeDeclaration type,
            System.CodeDom.CodeCompileUnit unit) { return type; }
    }

    class ConfiguredNode : ISimulatorNode, IConfigurableSimulatorNode
    {
        internal readonly ISimulatorNode Inner;
        readonly DNP3SimulatorPlugin plugin;
        readonly NodeConfiguration configuration;

        public ConfiguredNode(ISimulatorNode inner, DNP3SimulatorPlugin plugin, NodeConfiguration configuration)
        {
            Inner = inner;
            this.plugin = plugin;
            this.configuration = configuration;
        }

        public string SaveConfiguration()
        {
            var master = Inner as MasterNode;
            if (master != null) configuration.Scans = master.ScanConfigurations;
            return ConfigurationXml.Write(configuration);
        }

        public string EditConfiguration()
        {
            return plugin.EditConfiguration(ConfigurationXml.Read<NodeConfiguration>(SaveConfiguration()));
        }

        public void Remove() { Inner.Remove(); }
        public string Alias { get { return Inner.Alias; } }
        public string DisplayName { get { return Inner.DisplayName; } }
        public IEnumerable<Metric> Metrics { get { return Inner.Metrics; } }
        public IEnumerable<ISimulatorNodeAction> Actions { get { return Inner.Actions; } }
        public IEnumerable<ISimulatorNodeFactory> Children { get { return Inner.Children; } }
    }
}
