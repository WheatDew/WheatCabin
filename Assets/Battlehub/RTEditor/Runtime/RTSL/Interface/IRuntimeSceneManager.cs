using System;

namespace Battlehub.RTSL.Interface
{
    public interface IRuntimeSceneManager
    {
        event EventHandler NewSceneCreating;
        event EventHandler NewSceneCreated;
        void CreateNewScene();
        void ClearScene();
    }
}
