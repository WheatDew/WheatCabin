using Battlehub.UIControls.DockPanels;
using System.Threading.Tasks;

namespace Battlehub.RTEditor.Models
{
    public interface ILayoutStorageModel
    {
        string DefaultLayoutName
        {
            get;
        }

        bool LayoutExists(string path);
        LayoutInfo LoadLayout(string path);
        void SaveLayout(string path, LayoutInfo layout);
        string[] GetLayouts();
        void DeleteLayout(string path);
    }
}