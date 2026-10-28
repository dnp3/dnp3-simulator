using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Automatak.Simulator.API;

namespace Automatak.Simulator
{
    public partial class SimulatorForm
    {
        readonly Dictionary<string, TreeView> pluginTrees = new Dictionary<string, TreeView>();
        readonly string sessionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Automatak", "DNP3Simulator", "session.xml");
        bool automaticSaveEnabled = true;

        void ConfigurationAction(Action action)
        {
            try { action(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        XElement CaptureNode(TreeNode node)
        {
            var settings = node.Tag as IConfigurableSimulatorNode;
            if (settings == null) throw new InvalidOperationException("This node does not support saving configuration.");
            return new XElement("Node", new XAttribute("expanded", node.IsExpanded),
                new XAttribute("selected", node.TreeView != null && node.TreeView.SelectedNode == node),
                new XElement("Configuration", settings.SaveConfiguration()),
                node.Nodes.Cast<TreeNode>().Select(CaptureNode));
        }

        XDocument CaptureConfiguration()
        {
            var root = new XElement("SimulatorConfiguration", new XAttribute("version", 1),
                new XAttribute("selectedPlugin", tabControlPlugins.SelectedTab == null ? "" : tabControlPlugins.SelectedTab.Text));
            foreach (var entry in pluginTrees)
            {
                var plugin = entry.Value.Tag as IConfigurableSimulatorPlugin;
                if (plugin == null) throw new InvalidOperationException("Plugin does not support saving: " + entry.Key);
                root.Add(new XElement("Plugin", new XAttribute("id", entry.Key),
                    new XElement("Settings", plugin.SaveSettings()), entry.Value.Nodes.Cast<TreeNode>().Select(CaptureNode)));
            }
            return new XDocument(root);
        }

        internal static void WriteConfigurationFile(string path, XDocument document)
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    document.Save(stream);
                    stream.Flush(true);
                }
                if (File.Exists(path)) File.Replace(temporaryPath, path, null);
                else File.Move(temporaryPath, path);
            }
            finally { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); }
        }

        internal static XDocument ReadConfigurationFile(string path)
        {
            using (var reader = XmlReader.Create(path, new XmlReaderSettings {
                DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 32 * 1024 * 1024 }))
                return XDocument.Load(reader);
        }

        static string RequiredElement(XElement element, string name)
        {
            var children = element.Elements(name).ToList();
            if (children.Count != 1) throw new InvalidDataException("Expected one " + name + " element.");
            return children[0].Value;
        }

        void ValidateNode(XElement node, IConfigurableSimulatorPlugin plugin, int depth)
        {
            if (depth > 1) throw new InvalidDataException("Only channels can contain child nodes.");
            plugin.ValidateNode(RequiredElement(node, "Configuration"), depth == 0);
            // Check tree attributes before any running nodes are removed.
            var expanded = (bool?)node.Attribute("expanded") ?? false;
            var selected = (bool?)node.Attribute("selected") ?? false;
            foreach (var child in node.Elements("Node")) ValidateNode(child, plugin, depth + 1);
        }

        void ValidateConfiguration(XDocument document)
        {
            var root = document.Root;
            if (root == null || root.Name != "SimulatorConfiguration" || (string)root.Attribute("version") != "1")
                throw new InvalidDataException("Unsupported simulator configuration format/version.");
            var ids = new HashSet<string>();
            foreach (var element in root.Elements("Plugin"))
            {
                var id = (string)element.Attribute("id");
                TreeView tree;
                if (id == null || !ids.Add(id) || !pluginTrees.TryGetValue(id, out tree))
                    throw new InvalidDataException("Unknown or duplicate plugin: " + id);
                var plugin = tree.Tag as IConfigurableSimulatorPlugin;
                if (plugin == null) throw new InvalidDataException("Plugin does not support loading: " + id);
                plugin.ValidateSettings(RequiredElement(element, "Settings"));
                foreach (var node in element.Elements("Node")) ValidateNode(node, plugin, 0);
            }
            if (ids.Count != pluginTrees.Count) throw new InvalidDataException("Configuration is missing a simulator plugin.");
        }

        void ClearTree(TreeView tree)
        {
            foreach (TreeNode node in tree.Nodes) ShutdownFrom(node);
            tree.Nodes.Clear();
        }

        TreeNode RestoreNode(XElement element, IConfigurableSimulatorPlugin plugin, TreeNodeCollection collection,
            ISimulatorNode parent, TreeView tree)
        {
            var callbacks = new TreeNodeCallbacks(this);
            var instance = plugin.RestoreNode(RequiredElement(element, "Configuration"), parent, callbacks);
            if (instance == null) throw new InvalidOperationException("Unable to restore a simulator node.");
            var node = callbacks.node;
            BindNode(instance, node, collection);
            try
            {
                foreach (var child in element.Elements("Node")) RestoreNode(child, plugin, node.Nodes, instance, tree);
                if ((bool?)element.Attribute("expanded") == true) node.Expand();
                if ((bool?)element.Attribute("selected") == true) tree.SelectedNode = node;
                return node;
            }
            catch
            {
                ShutdownFrom(node);
                node.Remove();
                throw;
            }
        }

        void ApplyConfiguration(XDocument document)
        {
            foreach (var tree in pluginTrees.Values) ClearTree(tree);
            foreach (var element in document.Root.Elements("Plugin"))
            {
                var tree = pluginTrees[(string)element.Attribute("id")];
                var plugin = (IConfigurableSimulatorPlugin)tree.Tag;
                plugin.LoadSettings(RequiredElement(element, "Settings"));
                foreach (var node in element.Elements("Node")) RestoreNode(node, plugin, tree.Nodes, null, tree);
            }
            var selected = (string)document.Root.Attribute("selectedPlugin");
            foreach (TabPage page in tabControlPlugins.TabPages)
                if (page.Text == selected) tabControlPlugins.SelectedTab = page;
        }

        void LoadConfiguration(string path)
        {
            var document = ReadConfigurationFile(path);
            ValidateConfiguration(document);
            var previous = CaptureConfiguration();
            try { ApplyConfiguration(document); }
            catch (Exception loadError)
            {
                try { ApplyConfiguration(previous); }
                catch (Exception restoreError)
                {
                    automaticSaveEnabled = false;
                    throw new InvalidOperationException("Loading failed: " + loadError.Message +
                        "\nRestoring the previous session also failed: " + restoreError.Message +
                        "\nAutomatic saving is disabled to preserve the saved configuration.", restoreError);
                }
                throw;
            }
            automaticSaveEnabled = true;
        }

        void EditNode(TreeNode node)
        {
            var settings = (IConfigurableSimulatorNode)node.Tag;
            var edited = settings.EditConfiguration();
            if (edited == null) return;
            ApplyNodeConfiguration(node, edited);
        }

        void ApplyNodeConfiguration(TreeNode node, string edited)
        {
            var previous = CaptureNode(node);
            var replacement = new XElement(previous);
            replacement.Element("Configuration").Value = edited;
            var tree = node.TreeView;
            var plugin = (IConfigurableSimulatorPlugin)tree.Tag;
            ValidateNode(replacement, plugin, node.Parent == null ? 0 : 1);
            var parent = node.Parent;
            var collection = parent == null ? tree.Nodes : parent.Nodes;
            var parentInstance = parent == null ? null : (ISimulatorNode)parent.Tag;
            var index = node.Index;
            ShutdownFrom(node);
            node.Remove();
            TreeNode restored;
            try { restored = RestoreNode(replacement, plugin, collection, parentInstance, tree); }
            catch (Exception editError)
            {
                try
                {
                    restored = RestoreNode(previous, plugin, collection, parentInstance, tree);
                    restored.Remove();
                    collection.Insert(index, restored);
                    tree.SelectedNode = restored;
                }
                catch (Exception restoreError)
                {
                    automaticSaveEnabled = false;
                    throw new InvalidOperationException("Applying settings failed: " + editError.Message +
                        "\nRestoring the node failed: " + restoreError.Message + "\nAutomatic saving is disabled.", restoreError);
                }
                throw;
            }
            // Keep sibling order when replacing a running node.
            restored.Remove();
            collection.Insert(index, restored);
            tree.SelectedNode = restored;
        }

        void SaveConfiguration_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog { Filter = "Simulator configuration (*.xml)|*.xml", DefaultExt = "xml", FileName = "simulator.xml" })
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    ConfigurationAction(() => {
                        WriteConfigurationFile(dialog.FileName, CaptureConfiguration());
                        automaticSaveEnabled = true;
                    });
        }

        void LoadConfiguration_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "Simulator configuration (*.xml)|*.xml", CheckFileExists = true })
                if (dialog.ShowDialog(this) == DialogResult.OK) ConfigurationAction(() => LoadConfiguration(dialog.FileName));
        }

        void RestoreSession()
        {
            if (!File.Exists(sessionPath)) return;
            try { LoadConfiguration(sessionPath); }
            catch (Exception ex)
            {
                automaticSaveEnabled = false;
                MessageBox.Show(this, "Could not restore the previous session:\n" + ex.Message +
                    "\n\nThe saved file will be preserved. Use File > Load or File > Save to enable automatic saving again.\n" + sessionPath,
                    "Restore session", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
