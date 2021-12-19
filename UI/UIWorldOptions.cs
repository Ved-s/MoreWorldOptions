using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.IO;
using Terraria.UI;

namespace MoreWorldOptions.UI
{
    public class UIWorldOptions : UIState
    {
        private readonly WorldFileData Data;

        public UIWorldOptions(WorldFileData data)
        {
            this.Data = data;
        }
    }
}
