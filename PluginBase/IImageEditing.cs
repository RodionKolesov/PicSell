using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PluginBase
{
    public interface IImageEditing : IPlugin
    {
        Image ProcessImage(Image image, string settings=null);

        string SetSettings();
    }
}
