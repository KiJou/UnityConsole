﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityConsole
{
    internal static class CommandDatabase
    {
        private static Dictionary<string, MethodInfo> methodInfoCache;

        public static void ExecuteCommand (string methodName, params string[] args)
        {
            if (methodInfoCache == null || !methodInfoCache.ContainsKey(methodName))
            {
                Debug.LogError($"UnityConsole: Command `{methodName}` is not registered in the database.");
                return;
            }
            var methodInfo = methodInfoCache[methodName];
            var parametersInfo = methodInfo.GetParameters();
            if (parametersInfo.Length != args.Length)
            {
                Debug.LogError($"UnityConsole: Command `{methodName}` requires {parametersInfo.Length} args, while {args.Length} were provided.");
                return;
            }
            var parameters = new object[parametersInfo.Length];
            for (int i = 0; i < args.Length; i++)
                parameters[i] = Convert.ChangeType(args[i], parametersInfo[i].ParameterType);
            methodInfo.Invoke(null, parameters);
        }

        #if CONSOLE_ENABLED
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterCommands ()
        {
            methodInfoCache = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(method => method.GetCustomAttribute<CommandAttribute>() != null)
                .ToDictionary(method => method.Name, StringComparer.OrdinalIgnoreCase);
        }
        #endif
    }
}