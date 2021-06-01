using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace SGP.Shared.ContractResolvers
{
    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jProperty = base.CreateProperty(member, memberSerialization);
            if (jProperty.Writable)
            {
                return jProperty;
            }

            var property = member as PropertyInfo;
            jProperty.Writable = property?.SetMethod != null;
            return jProperty;
        }
    }
}
