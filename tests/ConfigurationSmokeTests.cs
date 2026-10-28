using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using Automatak.DNP3.Interface;
using Automatak.Simulator;
using Automatak.Simulator.API;
using Automatak.Simulator.DNP3;
using Automatak.Simulator.DNP3.Components;

class ConfigurationSmokeTests
{
    static int assertions;
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        ++assertions;
    }

    static object Invoke(object target, string method, params object[] args)
    {
        try
        {
            return target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        }
        catch (TargetInvocationException ex) { throw ex.InnerException; }
    }

    static object Field(object target, string field)
    {
        return target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    }

    static void Reject(Action action, string message)
    {
        try { action(); }
        catch { ++assertions; return; }
        throw new Exception(message);
    }

    static NodeConfiguration Master()
    {
        var master = new MasterStackConfig();
        master.link.keepAliveTimeout = TimeSpan.FromSeconds(73);
        master.master.responseTimeout = TimeSpan.FromMilliseconds(4321);
        master.master.startupIntegrityClassMask = new ClassField(5);
        master.master.eventScanOnEventsAvailableClassMask = new ClassField(8);
        return new NodeConfiguration { Kind = "Master", Alias = "master & test", Master = master,
            Scans = new List<ScanConfiguration> { new ScanConfiguration { Classes = 6, Period = TimeSpan.FromSeconds(27) } } };
    }

    static NodeConfiguration Outstation()
    {
        var config = new OutstationStackConfig();
        config.databaseTemplate.binary.Add(7, new BinaryConfig());
        config.databaseTemplate.binary.Add(8, new BinaryConfig { staticVariation = StaticBinaryVariation.Group1Var1 });
        config.databaseTemplate.analog.Add(42, new AnalogConfig());
        config.outstation.buffer.maxAnalogEvents = 432;
        config.outstation.buffer.maxOctetStringEvents = 777;
        config.outstation.config.allowUnsolicited = true;
        config.outstation.config.maxControlsPerRequest = 16;
        config.outstation.config.numUnsolRetries = NumRetries.Fixed(9);
        config.outstation.config.typesAllowedInClass0 = new StaticTypeBitField(37);
        return new NodeConfiguration { Kind = "Outstation", Alias = "outstation", Module = "DefaultOutstation", Outstation = config };
    }

    static NodeConfiguration Channel(int type)
    {
        ChannelConfiguration settings;
        using (var dialog = new ChannelDialog()) settings = dialog.Configuration;
        settings.Type = type;
        settings.Alias = "channel " + type;
        settings.ClientHost = "127.0.0.1";
        settings.ServerHost = "127.0.0.1";
        settings.ClientPort = 29997;
        settings.ServerPort = 29998;
        settings.SerialDevice = "COM42";
        settings.MinRetryMs = 1200;
        settings.MaxRetryMs = 2400;
        settings.ClientTlsOptions.PeerCertificate = "C:\\certs\\peer.pem";
        settings.ClientTlsOptions.LocalCertificate = "C:\\certs\\local.pem";
        settings.ClientTlsOptions.PrivateKey = "C:\\certs\\key.pem";
        return new NodeConfiguration { Kind = "Channel", Alias = settings.Alias, Channel = settings };
    }

    static XElement Node(NodeConfiguration configuration, params XElement[] children)
    {
        return new XElement("Node", new XAttribute("expanded", configuration.Kind == "Channel"),
            new XAttribute("selected", configuration.Kind == "Master"),
            new XElement("Configuration", ConfigurationXml.Write(configuration)), children);
    }

    static void TestDialogs()
    {
        var original = Master();
        var master = ConfigurationXml.Clone(original);
        Check(master.Master.master.startupIntegrityClassMask.ClassMask == 5, "Class mask lost");
        Check(master.Scans[0].Period == TimeSpan.FromSeconds(27), "Scan period lost");
        using (var dialog = new MasterDialog(master.Alias, master.Master))
        {
            Check(ConfigurationXml.Write(dialog.Configuration) == ConfigurationXml.Write(master.Master), "Master dialog changes settings on reopen");
        }
        for (int type = 0; type < 3; ++type)
        {
            var channel = ConfigurationXml.Clone(Channel(type));
            channel.Channel.Validate();
            using (var dialog = new ChannelDialog(channel.Channel))
                Check(ConfigurationXml.Write(dialog.Configuration) == ConfigurationXml.Write(channel.Channel), "Channel dialog round trip failed: " + type + "\nExpected: " + ConfigurationXml.Write(channel.Channel) + "\nActual: " + ConfigurationXml.Write(dialog.Configuration));
        }
        var outstation = ConfigurationXml.Clone(Outstation());
        Check(outstation.Outstation.databaseTemplate.analog.ContainsKey(42), "Sparse database index lost");
        Check(outstation.Outstation.outstation.buffer.maxAnalogEvents == 432, "Event buffer lost");
        Check(outstation.Outstation.outstation.config.numUnsolRetries.maxNumRetries == 9, "Retry count lost");
        Check(outstation.Outstation.outstation.config.typesAllowedInClass0.mask == 37, "Static type mask lost");
        var assembly = typeof(NodeConfiguration).Assembly;
        var configType = assembly.GetType("Automatak.Simulator.DNP3.DNP3Config");
        var module = Automatak.Simulator.DNP3.DefaultOutstationPlugin.OutstationModule.Instance;
        var config = Activator.CreateInstance(configType, new object[] { new[] { module } });
        var dialogType = assembly.GetType("Automatak.Simulator.DNP3.Components.OutstationDialog");
        using (var dialog = (Form)Activator.CreateInstance(dialogType, new object[] { config, module, outstation.Alias, outstation.Outstation }))
        {
            var result = (OutstationStackConfig)dialogType.GetProperty("Configuration").GetValue(dialog, null);
            Check(ConfigurationXml.Write(result) == ConfigurationXml.Write(outstation.Outstation), "Outstation dialog changes settings on reopen");
            var staticControl = (Control)Field(dialog, "staticResponseTypeControl1");
            var binary = (ComboBox)staticControl.Controls.Find("comboBoxBinary", true)[0];
            Check(binary.SelectedItem == null, "Mixed per-point variations must be preserved");
            var analog = (ComboBox)staticControl.Controls.Find("comboBoxAnalog", true)[0];
            Check((StaticAnalogVariation)analog.SelectedItem == outstation.Outstation.databaseTemplate.analog[42].staticVariation, "Static defaults do not show current configuration");
            analog.SelectedItem = StaticAnalogVariation.Group30Var6;
            result = (OutstationStackConfig)dialogType.GetProperty("Configuration").GetValue(dialog, null);
            Check(result.databaseTemplate.analog[42].staticVariation == StaticAnalogVariation.Group30Var6, "Static variation edit not applied");
            Check(outstation.Outstation.databaseTemplate.analog[42].staticVariation != StaticAnalogVariation.Group30Var6, "Dialog modified live database before Apply");
        }
        Reject(() => ConfigurationXml.Read<NodeConfiguration>("<!DOCTYPE x [<!ENTITY y SYSTEM 'file:///test'>]><x>&y;</x>"), "DTD accepted");
    }

    static SimulatorForm CreateForm(string session)
    {
        var form = new SimulatorForm(new[] { DNP3SimulatorPluginFactory.Instance }, false);
        form.GetType().GetField("sessionPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(form, session);
        var handle = form.Handle;
        Invoke(form, "SimulatorForm_Load", null, EventArgs.Empty);
        return form;
    }

    static void TestSession(string directory)
    {
        var session = Path.Combine(directory, "session.xml");
        var input = Path.Combine(directory, "input.xml");
        using (var form = CreateForm(session))
        {
            var trees = (Dictionary<string, TreeView>)Field(form, "pluginTrees");
            var plugin = (IConfigurableSimulatorPlugin)trees["DNP3"].Tag;
            var template = Outstation().Outstation.databaseTemplate;
            var document = new XDocument(new XElement("SimulatorConfiguration", new XAttribute("version", 1),
                new XAttribute("selectedPlugin", "DNP3"), new XElement("Plugin", new XAttribute("id", "DNP3"),
                    new XElement("Settings", ConfigurationXml.Write(new Dictionary<string, DatabaseTemplate> { { "device", template } })),
                    Node(Channel(0), Node(Master()), Node(Outstation())), Node(Channel(1)))));
            document.Save(input);
            Invoke(form, "LoadConfiguration", input);
            Application.DoEvents();
            var tree = trees["DNP3"];
            Check(tree.Nodes.Count == 2 && tree.Nodes[0].Nodes.Count == 2, "Tree hierarchy not restored");
            Check(tree.Nodes[0].IsExpanded, "Expanded state not restored");
            Check(tree.SelectedNode == tree.Nodes[0].Nodes[0], "Selection not restored");
            Check(tree.Nodes.Cast<TreeNode>().Concat(tree.Nodes[0].Nodes.Cast<TreeNode>()).All(n => n.ContextMenu.MenuItems[0].Text == "Settings..."), "Settings menu missing");
            var saved = (XDocument)Invoke(form, "CaptureConfiguration");
            Check(XNode.DeepEquals(document, saved), "Configuration changed after native restore/capture");

            // Preflight must reject malformed files without replacing live objects.
            var live = tree.Nodes[0].Tag;
            var bad = new XDocument(document);
            bad.Root.SetAttributeValue("version", 99);
            bad.Save(input);
            Reject(() => Invoke(form, "LoadConfiguration", input), "Unknown version accepted");
            Check(ReferenceEquals(live, tree.Nodes[0].Tag), "Invalid file removed current nodes");
            File.WriteAllText(input, "<broken");
            Reject(() => Invoke(form, "LoadConfiguration", input), "Broken XML accepted");
            Check(ReferenceEquals(live, tree.Nodes[0].Tag), "Broken XML removed current nodes");

            // A native creation failure after validation must roll the whole session back.
            bad = new XDocument(document);
            bad.Root.Element("Plugin").Element("Node").Add(Node(Outstation()));
            bad.Save(input);
            Reject(() => Invoke(form, "LoadConfiguration", input), "Duplicate native link accepted");
            Check(XNode.DeepEquals(saved, (XDocument)Invoke(form, "CaptureConfiguration")), "Failed import did not restore previous session");

            var editedMaster = Master();
            editedMaster.Alias = "edited master";
            editedMaster.Master.link.localAddr = 2;
            Invoke(form, "ApplyNodeConfiguration", tree.Nodes[0].Nodes[0], ConfigurationXml.Write(editedMaster));
            Check(tree.Nodes[0].Nodes[0].Text == "edited master", "Master edit was not applied or sibling order changed");
            Check(tree.Nodes[0].Nodes[1].Text == "outstation", "Master edit removed sibling outstation");
            var editedOutstation = Outstation();
            editedOutstation.Alias = "edited outstation";
            editedOutstation.Outstation.outstation.config.maxTxFragSize = 1024;
            Invoke(form, "ApplyNodeConfiguration", tree.Nodes[0].Nodes[1], ConfigurationXml.Write(editedOutstation));
            Check(tree.Nodes[0].Nodes[1].Text == "edited outstation", "Outstation edit was not applied");
            var editedChannel = Channel(0);
            editedChannel.Alias = editedChannel.Channel.Alias = "edited channel";
            editedChannel.Channel.ClientPort = 29996;
            Invoke(form, "ApplyNodeConfiguration", tree.Nodes[0], ConfigurationXml.Write(editedChannel));
            Check(tree.Nodes[0].Text == "edited channel" && tree.Nodes[0].Nodes.Count == 2, "Channel edit lost children or sibling order");
            Check(tree.Nodes[0].IsExpanded, "Channel edit lost expanded state");
            // Make a master collide with the existing outstation's native route.
            editedMaster.Master.link.localAddr = editedOutstation.Outstation.link.localAddr;
            editedMaster.Master.link.remoteAddr = editedOutstation.Outstation.link.remoteAddr;
            Reject(() => Invoke(form, "ApplyNodeConfiguration", tree.Nodes[0].Nodes[0], ConfigurationXml.Write(editedMaster)), "Duplicate route edit accepted");
            var current = (IConfigurableSimulatorNode)tree.Nodes[0].Nodes[0].Tag;
            Check(ConfigurationXml.Read<NodeConfiguration>(current.SaveConfiguration()).Master.link.localAddr == 2, "Failed edit did not roll back settings");
            Check(tree.Nodes[0].Nodes.Count == 2, "Failed edit lost children");

            var closing = new FormClosingEventArgs(CloseReason.UserClosing, false);
            Invoke(form, "SimulatorForm_FormClosing", null, closing);
            Check(!closing.Cancel && File.Exists(session), "Session not saved at close");
        }
        using (var form = CreateForm(session))
        {
            var trees = (Dictionary<string, TreeView>)Field(form, "pluginTrees");
            Check(trees["DNP3"].Nodes.Count == 2, "Session not automatically restored");
            Check(trees["DNP3"].Nodes[0].Text == "edited channel", "Edited channel not persisted");
            Check(trees["DNP3"].Nodes[0].Nodes.Count == 2, "Children not automatically restored");
            var settings = (IConfigurableSimulatorNode)trees["DNP3"].Nodes[0].Nodes[0].Tag;
            Check(ConfigurationXml.Read<NodeConfiguration>(settings.SaveConfiguration()).Scans.Count == 1, "Periodic scan not restored");
            Invoke(form, "SimulatorForm_FormClosing", null, new FormClosingEventArgs(CloseReason.UserClosing, false));
            Check(XDocument.Load(session).Root != null, "Atomic overwrite failed");
        }
    }

    [STAThread]
    static int Main()
    {
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configuration-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            TestDialogs();
            TestSession(directory);
            Console.WriteLine("PASS: " + assertions + " configuration checks");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally { Directory.Delete(directory, true); }
    }
}
