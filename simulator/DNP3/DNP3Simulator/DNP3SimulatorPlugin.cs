using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;

using Automatak.Simulator.Commons;
using Automatak.Simulator.API;
using Automatak.Simulator.DNP3.API;

namespace Automatak.Simulator.DNP3
{
    static class IconIndex
    {
        public const int Channel = 0;                
        public const int Master = 1;
        public const int Outstation = 2;
    };


    class DNP3SimulatorPlugin : ISimulatorPlugin, IConfigurableSimulatorPlugin
    {
        readonly ImageList imgList = new ImageList();
        readonly ILogHandler logHandler;
        readonly IDNP3Manager manager;
        
        readonly DNP3Config config = new DNP3Config(
           new IOutstationModule[]{ 
               Automatak.Simulator.DNP3.DefaultOutstationPlugin.OutstationModule.Instance,
               Automatak.Simulator.DNP3.RelayOutstationPlugin.OutstationModule.Instance
           }
        );

        public DNP3SimulatorPlugin(ILog log)
        {
            this.logHandler = new ForwardingLogHandler(log);

            this.manager = DNP3ManagerFactory.CreateManager(this.logHandler);

            imgList.Images.Add(Properties.Resources.satellite_dish);            
            imgList.Images.Add(Properties.Resources.network_monitor);
            imgList.Images.Add(Properties.Resources.send_container);

            /*
                Load outstation plugins here 
            */
        }

        string ISimulatorPlugin.UniqueId
        {
            get { return "DNP3"; }
        }

        System.Drawing.Image ISimulatorPlugin.PluginImage
        {
            get { return Properties.Resources.satellite_dish_add; }
        }


        string ISimulatorPlugin.RootDisplayName
        {
            get { return "DNP3 Channel"; }
        }


        ISimulatorNode ISimulatorPlugin.Create(ISimulatorNodeCallbacks callbacks)
        {
            using (var dialog = new Components.ChannelDialog())
            {
                dialog.ShowDialog();
                if (dialog.DialogResult == DialogResult.OK)
                {
                    var settings = new NodeConfiguration { Kind = "Channel", Alias = dialog.SelectedAlias, Channel = dialog.Configuration };
                    return RestoreNode(ConfigurationXml.Write(settings), null, callbacks);
                }
                else
                {
                    return null;    
                }
            }            
       }

       public string SaveSettings() { return config.SaveSettings(); }

       public void ValidateSettings(string settings)
       {
           var templates = ConfigurationXml.Read<Dictionary<string, DatabaseTemplate>>(settings);
           if (templates == null || templates.Any(kv => string.IsNullOrWhiteSpace(kv.Key) || kv.Value == null))
               throw new System.IO.InvalidDataException("Invalid database templates.");
       }

       public void LoadSettings(string settings)
       {
           ValidateSettings(settings);
           config.LoadSettings(settings);
       }

       public void ValidateNode(string configuration, bool isRoot)
       {
           var settings = ConfigurationXml.Read<NodeConfiguration>(configuration);
           if (settings == null || string.IsNullOrWhiteSpace(settings.Alias))
               throw new System.IO.InvalidDataException("A node must have an alias.");
           if (isRoot != (settings.Kind == "Channel"))
               throw new System.IO.InvalidDataException("Channels must be at the root of the tree.");
           switch (settings.Kind)
           {
               case "Channel":
                   if (settings.Channel == null || settings.Alias != settings.Channel.Alias)
                       throw new System.IO.InvalidDataException("Invalid channel configuration.");
                   settings.Channel.Validate();
                   using (var dialog = new Components.ChannelDialog(settings.Channel)) { }
                   break;
               case "Master":
                   if (settings.Master == null || settings.Master.link == null || !settings.Master.link.isMaster || settings.Master.master == null)
                       throw new System.IO.InvalidDataException("Invalid master configuration.");
                   using (var dialog = new Components.MasterDialog(settings.Alias, settings.Master)) { }
                   if (settings.Scans == null || settings.Scans.Any(s => s == null || s.Period <= TimeSpan.Zero || (s.Classes & ~15) != 0))
                       throw new System.IO.InvalidDataException("Invalid master scans.");
                   break;
               case "Outstation":
                   var module = GetModule(settings.Module);
                   if (settings.Outstation == null || settings.Outstation.link == null || settings.Outstation.link.isMaster ||
                       settings.Outstation.outstation == null || settings.Outstation.databaseTemplate == null)
                       throw new System.IO.InvalidDataException("Invalid outstation configuration.");
                   using (var dialog = new Components.OutstationDialog(config.Copy(), module, settings.Alias, settings.Outstation)) { }
                   break;
               default: throw new System.IO.InvalidDataException("Unknown node type: " + settings.Kind);
           }
       }

       IOutstationModule GetModule(string name)
       {
           var module = config.OutstationModules.FirstOrDefault(m => m.Name == name);
           if (module == null) throw new System.IO.InvalidDataException("Unknown outstation module: " + name);
           return module;
       }

       internal ISimulatorNode WrapNode(ISimulatorNode node, NodeConfiguration settings)
       {
           return new ConfiguredNode(node, this, ConfigurationXml.Clone(settings));
       }

       public ISimulatorNode RestoreNode(string configuration, ISimulatorNode parent, ISimulatorNodeCallbacks callbacks)
       {
           ValidateNode(configuration, parent == null);
           var settings = ConfigurationXml.Read<NodeConfiguration>(configuration);
           if (settings.Kind == "Channel")
           {
               var listener = new ChannelListenerAdapter(state => callbacks.ChangeState(ChannelNode.GetNodeState(state)));
               var channel = settings.Channel.Create(manager, listener);
               if (channel == null) throw new InvalidOperationException("Unable to create channel " + settings.Alias);
               return WrapNode(new ChannelNode(this, config, channel, callbacks, settings.Alias), settings);
           }

           var parentNode = parent as ConfiguredNode;
           var channelNode = parentNode == null ? null : parentNode.Inner as ChannelNode;
           if (channelNode == null) throw new System.IO.InvalidDataException("A master/outstation must belong to a channel.");
           if (settings.Kind == "Master")
           {
               var cache = new Commons.MeasurementCache();
               var master = channelNode.Channel.AddMaster(settings.Alias, cache, DefaultMasterApplication.Instance, settings.Master);
               if (master == null) throw new InvalidOperationException("Unable to create master " + settings.Alias);
               try
               {
                   var node = new MasterNode(cache, master, callbacks, settings.Alias, settings.Scans);
                   master.Enable();
                   return WrapNode(node, settings);
               }
               catch { master.Shutdown(); throw; }
           }
           var factory = GetModule(settings.Module).CreateFactory();
           var outstation = channelNode.Channel.AddOutstation(settings.Alias, factory.CommandHandler, factory.Application, settings.Outstation);
           if (outstation == null) throw new InvalidOperationException("Unable to create outstation " + settings.Alias);
           try
           {
               var instance = factory.CreateInstance(outstation, settings.Alias, settings.Outstation);
               outstation.Enable();
               return WrapNode(new OutstationNode(outstation, instance, callbacks), settings);
           }
           catch { outstation.Shutdown(); throw; }
       }

       internal string EditConfiguration(NodeConfiguration settings)
       {
           switch (settings.Kind)
           {
               case "Channel":
                   using (var dialog = new Components.ChannelDialog(settings.Channel))
                   {
                       if (dialog.ShowDialog() != DialogResult.OK) return null;
                       settings.Channel = dialog.Configuration;
                       settings.Alias = dialog.SelectedAlias;
                   }
                   break;
               case "Master":
                   using (var dialog = new Components.MasterDialog(settings.Alias, settings.Master))
                   {
                       if (dialog.ShowDialog() != DialogResult.OK) return null;
                       settings.Master = dialog.Configuration;
                       settings.Alias = dialog.SelectedAlias;
                   }
                   break;
               case "Outstation":
                   using (var dialog = new Components.OutstationDialog(config.Copy(), GetModule(settings.Module), settings.Alias, settings.Outstation))
                   {
                       if (dialog.ShowDialog() != DialogResult.OK) return null;
                       settings.Outstation = dialog.Configuration;
                       settings.Alias = dialog.SelectedAlias;
                   }
                   break;
           }
           var result = ConfigurationXml.Write(settings);
           ValidateNode(result, settings.Kind == "Channel");
           return result;
       }

       System.Windows.Forms.ImageList ISimulatorPlugin.NodeImageList
       {
           get { return imgList; }
       }
    }
}
