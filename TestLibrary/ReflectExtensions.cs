using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TestLibrary
{
    internal static class ReflectExtensions
    {

        internal static FieldInfo GetInstanceField(this Type t, string fieldName)
        {
			return ReflectExtensions.GetField(t, fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

		internal static FieldInfo GetStaticField(this Type t, string fieldName)
		{
			return ReflectExtensions.GetField(t, fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}


		internal static FieldInfo GetField(this Type t, string fieldName, BindingFlags flags)
		{
			if( t == null ) 
				throw new ArgumentNullException("t");
			
			if( string.IsNullOrEmpty(fieldName) ) 
				throw new ArgumentNullException("fieldName");
			

			return t.GetField(fieldName, flags);
		}

		internal static ConstructorInfo GetSpecificCtor(this Type t, params Type[] types)
		{
			if( t == null )
				throw new ArgumentNullException("t");


			return t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
				types, null);
		}
    }

}
