using System;

namespace Battlehub.RTEditor
{
    [Obsolete("Use MenuDefinitionAttribute and MenuCommandAttribute to create/update main menu")]
    public interface IEditCmd
    {
        [Obsolete]
        bool CanExec(string cmd);
        [Obsolete]
        void Exec(string cmd);
    }

}
