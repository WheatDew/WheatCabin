using Battlehub.RTCommon;
using System;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [Obsolete("Use MenuDefinitionAttribute and MenuCommandAttribute to create/update main menu")]
    public interface IGameObjectCmd
    {
        [Obsolete]
        bool CanExec(string cmd);

        [Obsolete]
        void Exec(string cmd);
    }
}


