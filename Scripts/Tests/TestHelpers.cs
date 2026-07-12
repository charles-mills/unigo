using System;
using System.Reflection;

namespace Tests
{
    public class TestHelpers
    {
        public static void SetPrivate<T>(object target, string fieldName, T value)
        {
            var type = target.GetType();
            var field =
                type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(type.FullName, fieldName);
            field.SetValue(target, value);
        }

        public static T GetPrivate<T>(object target, string fieldName)
        {
            var field =
                target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(target.GetType().FullName, fieldName);
            return (T)field.GetValue(target);
        }
    }
}