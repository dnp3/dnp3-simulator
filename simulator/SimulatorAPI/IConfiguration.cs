namespace Automatak.Simulator.API
{
    // Optional persistence support for simulator plugins and their nodes.
    public interface IConfigurableSimulatorPlugin
    {
        string SaveSettings();
        void ValidateSettings(string settings);
        void LoadSettings(string settings);
        void ValidateNode(string configuration, bool isRoot);
        ISimulatorNode RestoreNode(string configuration, ISimulatorNode parent, ISimulatorNodeCallbacks callbacks);
    }

    public interface IConfigurableSimulatorNode
    {
        string SaveConfiguration();
        // Returns null when the user cancels. Does not change the running node.
        string EditConfiguration();
    }
}
