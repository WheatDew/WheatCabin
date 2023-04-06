using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System.Collections;
using UnityEngine;

namespace Battlehub.RTSL.Demo
{
    /*
    [System.Serializable]
    public class MyObject
    {
        public string Data;
    }
    */

    public class SetGetValueUsageExample : MonoBehaviour
    {
        /*
        public IEnumerator Start()
        {
           
            IProject project = IOC.Resolve<IProject>();

            var openProject = project.OpenProject("SetGetValueUsageExample");
            yield return openProject;

            MyObject obj = new MyObject();
            obj.Data = "MyData";

            var setValue = project.SetValue("MyObject", obj);
            yield return setValue;

            var getValue = project.GetValue<MyObject>("MyObject");
            yield return getValue;

            Debug.Log("Loaded: " + getValue.Result.Data);
            project.CloseProject();

            var deleteProject = project.DeleteProject("SetGetValueUsageExample");
            yield return deleteProject;
        }*/

        public async void Start()
        {
            IProjectAsync projectAsync = IOC.Resolve<IProjectAsync>();

            const string projectName = "SetGetValueUsageExample";

            await projectAsync.OpenProjectAsync(projectName);

            await projectAsync.SetValueAsync("TestKey", "Test Value");

            string value = await projectAsync.GetValueAsync<string>("TestKey");

            Debug.Log(value);

            await projectAsync.DeleteProjectAsync(projectName);

            await projectAsync.CloseProjectAsync();
        }
    }
}

