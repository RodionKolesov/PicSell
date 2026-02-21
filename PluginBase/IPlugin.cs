namespace PluginBase
{
    public interface IPlugin
    {
        public string GetInfo();
        public string GetGUID();
        public string GetGUIinfo();
        public string GetPluginType();
    }
}
