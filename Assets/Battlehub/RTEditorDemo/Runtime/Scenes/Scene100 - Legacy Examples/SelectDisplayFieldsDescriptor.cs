using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Battlehub.RTEditor.Demo
{
    public class SelectDisplayFieldsDescriptor : ComponentDescriptorBase<SelectDisplayFieldsBehaviour>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo groupInfo = Strong.MemberInfo((SelectDisplayFieldsBehaviour x) => x.Group);
            MemberInfo field1Info = Strong.MemberInfo((SelectDisplayFieldsBehaviour x) => x.Field1);
            MemberInfo field2Info = Strong.MemberInfo((SelectDisplayFieldsBehaviour x) => x.Field2);

            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
            descriptors.Add(new PropertyDescriptor("Group", editor.Components, groupInfo)
            {
                ValueChangedCallback = () => editor.BuildEditor()
            });

            IEnumerable<FieldGroup> groups = editor.Components.Where(component => component != null).OfType<SelectDisplayFieldsBehaviour>().Select(behavior => behavior.Group).Distinct();
            foreach(FieldGroup group in groups)
            {
                switch (group)
                {
                    case FieldGroup.Group1:
                        descriptors.Add(new PropertyDescriptor("Field1", editor.Components, field1Info));
                        break;
                    case FieldGroup.Group2:
                        descriptors.Add(new PropertyDescriptor("Field2", editor.Components, field2Info));
                        break;
                }
            }
            
            return descriptors.ToArray();
        }
    }
}
